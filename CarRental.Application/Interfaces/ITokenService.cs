using CarRental.Application.DTOs;

namespace CarRental.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserDto user);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
    }
}