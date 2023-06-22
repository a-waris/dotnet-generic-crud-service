using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace GenericCRUDServiceExample;

public interface IGenericCRUDService<TModel, TDto>
{
    Task<IEnumerable<TDto>> GetAll(Expression<Func<TModel, bool>>? where = null, params string[] includes);
    Task<TDto> GetById(int id);
    Task<TDto> Add(TDto dto, params Expression<Func<TModel, object>>[] references);

    Task<TDto> Update(int id, TDto dto, Expression<Func<TModel, bool>>? where = null,
        params Expression<Func<TModel, object>>[] references);

    Task<bool> Delete(int id);
}

public class GenericCRUDService<TModel, TDto> : IGenericCRUDService<TModel, TDto>
    where TModel : class
    where TDto : class
{
    private readonly IMapper _mapper;
    private readonly DbContext _dbContext;

    public GenericCRUDService(IMapper mapper, DbContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    // Example 1: var dtos = await _genericService.GetAll("NavigationProperty1", "NavigationProperty2");
    // Example 2: var customers = await _customerService.GetAll(c => c.Age > 18 && c.IsActive, "Orders");

    public async Task<IEnumerable<TDto>> GetAll(Expression<Func<TModel, bool>>? where = null,
        params string[] includes)
    {
        var query = _dbContext.Set<TModel>().AsQueryable();

        // Apply the 'where' predicate if provided
        if (where != null)
        {
            query = query.Where(where);
        }

        // Include related entities if the 'includes' parameter is provided
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        var entities = await query.ToListAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public async Task<TDto> GetById(int id)
    {
        var entity = await _dbContext.Set<TModel>().FindAsync(id);
        return _mapper.Map<TDto>(entity);
    }

    public async Task<TDto> Add(TDto dto, params Expression<Func<TModel, object>>[] references)
    {
        var entity = _mapper.Map<TModel>(dto);
        _dbContext.Set<TModel>().Add(entity);

        // Include related entities if the 'references' parameter is provided
        foreach (var reference in references)
        {
            await _dbContext.Entry(entity).Reference(reference!).LoadAsync();
        }

        await _dbContext.SaveChangesAsync();
        return _mapper.Map<TDto>(entity);
    }

    // Example: var updatedDto = await _genericService.Update(1, dto, c => c.IsActive, c => c.User);
    public async Task<TDto> Update(int id, TDto dto,
        Expression<Func<TModel, bool>>? where = null,
        params Expression<Func<TModel, object>>[] references)
    {
        var query = _dbContext.Set<TModel>().AsQueryable();

        // Apply the 'where' predicate if provided
        if (where == null)
        {
            // If no 'where' predicate is provided, findAsync the entity by id where id is the one that has [Key] annotation in the model so find the property that has [Key] annotation
            var keyProperty = typeof(TModel).GetProperties().FirstOrDefault(p =>
                p.CustomAttributes.Any(a =>
                    a.AttributeType == typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));
            if (keyProperty == null)
            {
                throw new Exception("No [Key] attribute found in the model");
            }

            query = query.Where(
                entity => (int)entity.GetType().GetProperty(keyProperty.Name)!.GetValue(entity)! == id);
        }
        else
        {
            // If a 'where' predicate is provided, apply it
            query = query.Where(where);
        }


        var entity = await query.FirstOrDefaultAsync();

        _mapper.Map(dto, entity);

        foreach (var reference in references)
        {
            if (entity != null) await _dbContext.Entry(entity).Reference(reference!).LoadAsync();
        }

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
}