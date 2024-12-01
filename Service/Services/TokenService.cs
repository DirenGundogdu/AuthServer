using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Core.Configuration;
using Core.DTOs;
using Core.Entities;
using Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configurations;

namespace Service.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly CustomTokenOption _customTokenOption;

    private string CreateRefreshToken() {
        var numberByte = new Byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(numberByte);
        return Convert.ToBase64String(numberByte);
    }

    private IEnumerable<Claim> GetClaims(User user, List<string> audiences) {
        var userList = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud,x)));
        return userList;
    }

    private IEnumerable<Claim> GetClaimsByClient(Client client) {
        var claims = new List<Claim>();
        claims.AddRange(client.Audiences.Select(x=> new Claim(JwtRegisteredClaimNames.Aud,x)));

        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString());

        return claims;
    }

    public TokenService(UserManager<User> userManager, IOptions<CustomTokenOption> options) {
        _userManager = userManager;
        _customTokenOption = options.Value;
    }

    public TokenDto CreateToken(User user) {
        var accessTokenExpration = DateTime.Now.AddMinutes(_customTokenOption.AccessTokenExpiration);
        var refreshTokenExpration = DateTime.Now.AddMinutes(_customTokenOption.RefreshTokenExpiration);
        var securityKey = SignService.GetSymmetricSecurityKey(_customTokenOption.SecurityKey);

        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            issuer: _customTokenOption.Issuer,
            expires: accessTokenExpration,
            notBefore: DateTime.Now,
            claims: GetClaims(user,_customTokenOption.Audience),
            signingCredentials:signingCredentials
        );
        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(jwtSecurityToken);

        var tokenDto = new TokenDto {
            AccessToken = token,
            RefreshToken = CreateRefreshToken(),
            AccessTokenExpiration = accessTokenExpration,
            RefreshTokenExpiration = refreshTokenExpration
        };
        return tokenDto;
    }

    public ClientTokenDto CreateTokenByClient(Client client) {
        var accessTokenExpration = DateTime.Now.AddMinutes(_customTokenOption.AccessTokenExpiration);
        var securityKey = SignService.GetSymmetricSecurityKey(_customTokenOption.SecurityKey);

        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            issuer: _customTokenOption.Issuer,
            expires: accessTokenExpration,
            notBefore: DateTime.Now,
            claims: GetClaimsByClient(client),
            signingCredentials:signingCredentials
        );
        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(jwtSecurityToken);

        var tokenDto = new ClientTokenDto() {
            AccessToken = token,
            AccessTokenExpiration = accessTokenExpration,
        };
        return tokenDto;
    }
}