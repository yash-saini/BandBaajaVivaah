using BandBaaajaVivaah.Data.Models;
using BandBaaajaVivaah.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandBaajaVivaah.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User> CreateUserAsync(User newUser);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Inject the Unit of Work instead of the DbContext
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public async Task<User> CreateUserAsync(User newUser)
        {
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync(); // Save changes
            return newUser;
        }
    }
}
