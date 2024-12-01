using Core.DTOs;
using Core.Entities;
using Core.Services;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.DTOs;

namespace Service.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager) {
        _userManager = userManager;
    }

    public async Task<Response<UserDto>> CreateUserAsync(CreateUserDto createUserDto) {
        var user = new User {
            Email = createUserDto.Email,
            UserName = createUserDto.UserName
        };
        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description).ToList();
            return Response<UserDto>.Fail(new ErrorDto(errors, true), 400);
        }
        return Response<UserDto>.Success(ObjectMapper.Mapper.Map<UserDto>(user), 200);
    }

    public async Task<Response<UserDto>> GetUserByUserNameAsync(string userName) {
        var user = await _userManager.FindByNameAsync(userName);
        
        if (user == null) return Response<UserDto>.Fail("User not found", 404, true);
         
        return Response<UserDto>.Success(ObjectMapper.Mapper.Map<UserDto>(user), 200);
    }
}