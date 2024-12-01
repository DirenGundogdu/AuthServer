using Core.Configuration;
using Core.DTOs;
using Core.Entities;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.DTOs;

namespace Service.Services;

public class AuthenticationService : IAuthenticationService 
{
    private readonly List<Client> _clients;
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;

    public AuthenticationService(IOptions<List<Client>> optionsClient, ITokenService tokenService, UserManager<User> userManager, IUnitOfWork unitOfWork, IRepository<RefreshToken> refreshTokenRepository) {
        _clients = optionsClient.Value;
        _tokenService = tokenService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto) {
        if (loginDto == null) throw new ArgumentNullException(nameof(loginDto));
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if(user == null) return Response<TokenDto>.Fail("Email or password is wrong", 400, true);

        if (!await _userManager.CheckPasswordAsync(user,loginDto.Password))
        {
            return Response<TokenDto>.Fail("Email or password is wrong", 400, true);
        }

        var token = _tokenService.CreateToken(user);

        var userRefreshToken = await _refreshTokenRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

        if (userRefreshToken == null)
        {
            await _refreshTokenRepository.AddAsync(new RefreshToken {
                UserId = user.Id,
                Code = token.RefreshToken,
                Expiration = token.RefreshTokenExpiration
            });
        }
        else
        {
            userRefreshToken.Code = token.RefreshToken;
            userRefreshToken.Expiration = token.RefreshTokenExpiration;
        }

        await _unitOfWork.SaveChangesAsync();
        return Response<TokenDto>.Success(token, 200);
    }

    public  Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto) {
        var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);
        if(client == null) return Response<ClientTokenDto>.Fail("ClientId or ClientSecret is wrong", 404, true);
        var token = _tokenService.CreateTokenByClient(client);
        return Response<ClientTokenDto>.Success(token, 200);
    }
    
    public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken) {
        var existRefreshToken = await _refreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
        
        if(existRefreshToken == null) return Response<TokenDto>.Fail("Refresh token not found", 404, true);
        
        var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);
        
        if(user == null) return Response<TokenDto>.Fail("User not found", 404, true);
        
        var token = _tokenService.CreateToken(user);
        
        existRefreshToken.Code = token.RefreshToken;
        existRefreshToken.Expiration = token.RefreshTokenExpiration;

        await _unitOfWork.SaveChangesAsync();

        return Response<TokenDto>.Success(token, 200);
    }

    public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken) {
        var existRefreshToken = await _refreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
        
        if(existRefreshToken == null) return Response<NoDataDto>.Fail("Refresh token not found", 404, true);

        _refreshTokenRepository.Remove(existRefreshToken);

        await _unitOfWork.SaveChangesAsync();
        return Response<NoDataDto>.Success(200);
    }

    
}