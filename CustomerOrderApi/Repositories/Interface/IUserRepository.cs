using CustomerOrderApi.DTOs;
using CustomerOrderApi.Models;

namespace CustomerOrderApi.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task AddUserAsync(User user);
        Task<bool> CheckUserExistsAsync(string email);
        Task<User> GetUserByEmailVerificationTokenAsync(string token);
        Task<User> GetUserByPasswordResetTokenAsync(string token);

        Task<List<User>> GetAllUsersAsync();
    }
}
