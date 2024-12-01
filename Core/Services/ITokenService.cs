using Core.Configuration;
using Core.DTOs;
using Core.Entities;

namespace Core.Services;

public interface ITokenService
{
    TokenDto CreateToken(User user);

    ClientTokenDto CreateTokenByClient (Client client);
}