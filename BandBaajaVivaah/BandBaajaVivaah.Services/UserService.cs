using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTO;

namespace BandBaajaVivaah.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<User> RegisterUserAsync(string fullName, string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Inject the Unit of Work instead of the DbContext
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                UserID = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<User> RegisterUserAsync(string fullName, string email, string password)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Role = "User"
            };
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync();
            return newUser;
        }
    }
}
