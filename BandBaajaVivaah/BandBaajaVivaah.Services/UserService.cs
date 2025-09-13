using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTO;
using System.Security.Cryptography;

namespace BandBaajaVivaah.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<User> RegisterUserAsync(string fullName, string email, string password);
        Task<User?> LoginAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
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

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<bool> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                return false; // User not found
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

            await _unitOfWork.CompleteAsync();
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            return true;

        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null || user.PasswordResetToken != token || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false; // Invalid token or user
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
