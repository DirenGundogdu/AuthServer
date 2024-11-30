using Core.DTOs;
using SharedLibrary.DTOs;

namespace Core.Services;

public interface IUserService
{
    Task<Response<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
    
     Task<Response<UserDto>> GetUserByUserNameAsync(string userName);
    
}