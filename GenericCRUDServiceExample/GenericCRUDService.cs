using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace GenericCRUDServiceExample;

public interface IGenericCRUDService<TModel, TDto>
{
    Task<IEnumerable<TDto>> GetAll(Expression<Func<TModel, bool>>? where = null, params string[] includes);
    Task<TDto?> GetById(Expression<Func<TModel, bool>> predicateToGetId, params string[] includes);
    Task<TDto> Add(TDto dto, params Expression<Func<TModel, object>>[] references);

    Task<TDto> Update(TDto dto, Expression<Func<TModel, bool>>? where = null,
        params Expression<Func<TModel, object>>[] references);

    Task<bool> Delete(IGuid id);
}

public class GenericCRUDService<TModel, TDto> : IGenericCRUDService<TModel, TDto>
    where TModel : class
    where TDto : class
{
    private readonly IMapper _mapper;
    private readonly IContext _dbContext;

    public GenericCRUDService(IMapper mapper, IContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TDto>> GetAll(Expression<Func<TModel, bool>>? where = null,
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

    public async Task<TDto?> GetById(Expression<Func<TModel, bool>> predicateToGetId,
        params string[] includes)
    {
        var query = ApplyIncludes(_dbContext.Set<TModel>(), includes);

        var entity = await query.FirstOrDefaultAsync(predicateToGetId);
        return entity == null ? null : _mapper.Map<TDto>(entity);
    }


    public async Task<TDto> Add(TDto dto, params Expression<Func<TModel, object>>[] references)
    {
        var entity = _mapper.Map<TModel>(dto);
        _dbContext.Set<TModel>().Add(entity);

        await LoadReferences(entity, references);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<TDto>(entity);
    }

    public async Task<TDto> Update(TDto dto, Expression<Func<TModel, bool>>? where = null,
        params Expression<Func<TModel, object>>[] references)
    {
        var query = _dbContext.Set<TModel>().AsQueryable();

        if (where != null)
        {
            query = query.Where(where);
        }

        var entity = await query.FirstOrDefaultAsync();
        if (entity == null) throw new Exception("Entity not found");
        await LoadReferences(entity, references);
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<TDto>(entity);

    }

    public async Task<bool> Delete(IGuid id)
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

    private async Task LoadReferences(TModel entity, IEnumerable<Expression<Func<TModel, object>>> references)
    {
        foreach (var reference in references)
        {
            await _dbContext.Entry(entity).Reference(reference!).LoadAsync();
        }
    }
}