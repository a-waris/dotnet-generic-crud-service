using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace GenericCRUDServiceExample;


public interface IGenericCRUDService<TModel, TDto>
{
    Task<IEnumerable<TDto>> GetAll(Expression<Func<TModel, bool>> where = null, params string[] includes);
    Task<TDto> GetById(int id, params string[] includes);
    Task<TDto> Add(TDto dto, params Expression<Func<TModel, object>>[] references);
    Task<TDto> Update(int id, TDto dto, Expression<Func<TModel, bool>> where = null,
        params Expression<Func<TModel, object>>[] references);
    Task<bool> Delete(int id);
}

public class GenericCRUDService<TModel, TDto> : IGenericCRUDService<TModel, TDto>
    where TModel : class
    where TDto : class
{
    private readonly IMapper _mapper;
    private readonly DbContext _dbContext;
    private readonly PropertyInfo _keyProperty;

    public GenericCRUDService(IMapper mapper, DbContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _keyProperty = GetKeyProperty();
    }

    public async Task<IEnumerable<TDto>> GetAll(Expression<Func<TModel, bool>> where = null,
        params string[] includes)
    {
        var query = ApplyIncludes(_dbContext.Set<TModel>(), includes);

        if (where != null)
        {
            query = query.Where(where);
        }

        var entities = await query.ToListAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public async Task<TDto> GetById(int id, params string[] includes)
    {
        var query = ApplyIncludes(_dbContext.Set<TModel>(), includes);
        var entity = await query.FirstOrDefaultAsync(e => GetKeyValue(e) == id);
        return _mapper.Map<TDto>(entity);
    }

    public async Task<TDto> Add(TDto dto, params Expression<Func<TModel, object>>[] references)
    {
        var entity = _mapper.Map<TModel>(dto);
        _dbContext.Set<TModel>().Add(entity);

        await LoadReferences(entity, references);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TDto>(entity);
    }

    public async Task<TDto> Update(int id, TDto dto, Expression<Func<TModel, bool>> where = null,
        params Expression<Func<TModel, object>>[] references)
    {
        var query = ApplyWhere(_dbContext.Set<TModel>(), where, id);
        var entity = await query.FirstOrDefaultAsync();

        _mapper.Map(dto, entity);
        await LoadReferences(entity, references);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TDto>(entity);
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await _dbContext.Set<TModel>().FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        _dbContext.Set<TModel>().Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private IQueryable<TModel> ApplyIncludes(IQueryable<TModel> query, IEnumerable<string> includes)
    {
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }

    private IQueryable<TModel> ApplyWhere(IQueryable<TModel> query, Expression<Func<TModel, bool>> where, int id)
    {
        query = query.Where(where ?? (entity => GetKeyValue(entity) == id));

        return query;
    }

    private int GetKeyValue(TModel entity)
    {
        return (int)_keyProperty.GetValue(entity)!;
    }

    private async Task LoadReferences(TModel entity, IEnumerable<Expression<Func<TModel, object>>> references)
    {
        foreach (var reference in references)
        {
            await _dbContext.Entry(entity).Reference(reference).LoadAsync();
        }
    }

    private static PropertyInfo GetKeyProperty()
    {
        var keyProperty = typeof(TModel).GetProperties().FirstOrDefault(p =>
            p.CustomAttributes.Any(a =>
                a.AttributeType == typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));

        if (keyProperty == null)
        {
            throw new Exception("No [Key] attribute found in the model");
        }

        return keyProperty;
    }
}