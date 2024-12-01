using System.Linq.Expressions;
using SharedLibrary.DTOs;

namespace Core.Services;

public interface IService<TEntity,TDto> where TEntity: class where TDto: class
{
    Task<Response<TDto>> GetByIdAsync(int id); 
    
    Task<Response<IEnumerable<TDto>>> GetAllAsync();

    Response<IEnumerable<TDto>> Where(Expression<Func<TEntity, bool>> predicate);
    
    Task<Response<TDto>> AddAsync(TDto entity);
    
    Task<Response<NoDataDto>> Remove(int id);
    
    Task<Response<NoDataDto>> Update(TDto entity,int id);
}