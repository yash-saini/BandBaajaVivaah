using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;

namespace BandBaajaVivaah.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksByWeddingIdAsync(int weddingId, int userId);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, int userId);
        Task<bool> UpdateTaskAsync(int taskId, CreateTaskDto taskDto, int userId);
        Task<bool> DeleteTaskAsync(int taskId, int userId);

        Task<TaskDto> CreateTasksAsAdminAsync(CreateTaskDto taskDto);
        Task<bool> UpdateTasksAsAdminAsync(int taskId, CreateTaskDto taskDto);
        Task<bool> DeleteTasksAsAdminAsync(int taskId);
    }

    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, int userId)
        {
            // Security Check: Ensure the user owns the wedding they're adding a task to
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(taskDto.WeddingID);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to add a task to this wedding.");
            }

            return await CreateTasksAsAdminAsync(taskDto);
        }

        public async Task<TaskDto> CreateTasksAsAdminAsync(CreateTaskDto taskDto)
        {
            var task = new BandBaaajaVivaah.Data.Models.Task
            {
                Title = taskDto.Title,
                WeddingId = taskDto.WeddingID,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Status = taskDto.Status,
            };

            await _unitOfWork.Tasks.AddAsync(task);
            await _unitOfWork.CompleteAsync();

            return new TaskDto
            {
                TaskID = task.TaskId,
                Title = task.Title,
                Status = task.Status,
                Description = task.Description,
                DueDate = task.DueDate,
            };
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByWeddingIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
            {
                throw new UnauthorizedAccessException("You are not authorized to view guests for this wedding.");
            }

            var tasks = await _unitOfWork.Tasks.FindAllAsync(t => t.WeddingId == weddingId); // Requires custom repository method
            return tasks.Select(t => new TaskDto
            {
                TaskID = t.TaskId,
                Title = t.Title,
                Status = t.Status,
                Description = t.Description,
                DueDate = t.DueDate
            });
        }

        public async Task<bool> UpdateTaskAsync(int taskId, CreateTaskDto taskDto, int userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(task.WeddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                return false;
            }
            return await UpdateTasksAsAdminAsync(taskId, taskDto);
        }

        public async Task<bool> UpdateTasksAsAdminAsync(int taskId, CreateTaskDto taskDto)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.DueDate = taskDto.DueDate;
            task.Status = taskDto.Status;
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(task.WeddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                return false;
            }
            return await DeleteTasksAsAdminAsync(taskId);
        }

        public async Task<bool> DeleteTasksAsAdminAsync(int taskId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }
            await _unitOfWork.Tasks.DeleteAsync(taskId);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
