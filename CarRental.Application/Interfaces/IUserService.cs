using CarRental.Application.DTOs;

namespace CarRental.Application.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> RegisterAsync(CreateUserDto createUserDto);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpdateUserAsync(int id, CreateUserDto updateUserDto);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
    }
}