using ConstructionPayment.Domain.Entities;

namespace ConstructionPayment.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAtUtc) GenerateToken(AppUser user);
}
