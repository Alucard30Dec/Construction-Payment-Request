using ConstructionPayment.Domain.Entities;

namespace ConstructionPayment.Application.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(AppUser user, string password);
    bool VerifyPassword(AppUser user, string hashedPassword, string providedPassword);
}
