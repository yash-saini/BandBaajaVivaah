using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Grpc;
using BandBaajaVivaah.Services.GrpcServices;
using Google.Protobuf.WellKnownTypes;

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

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
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

           var result = new TaskDto
            {
                TaskID = task.TaskId,
                Title = task.Title,
                Status = task.Status,
                Description = task.Description,
                DueDate = task.DueDate,
            };

            await NotifyTaskChange(task, TaskUpdateEvent.Types.UpdateType.Created);
            return result;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByWeddingIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
            {
                throw new UnauthorizedAccessException("You are not authorized to view tasks for this wedding.");
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
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
            {
                return false; // User does not own the wedding this task belongs to
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
            await NotifyTaskChange(task, TaskUpdateEvent.Types.UpdateType.Updated);
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
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            if (wedding == null || (!isAdmin && wedding.OwnerUserId != userId))
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
            var weddingId = task.WeddingId;
            var taskDetails = CreateTaskDetails(task);
            await _unitOfWork.Tasks.DeleteAsync(taskId);
            await _unitOfWork.CompleteAsync();

            await TaskUpdateGrpcService.NotifyTaskChange(
                weddingId,
                TaskUpdateEvent.Types.UpdateType.Deleted,
                taskDetails);
            return true;
        }

        private async System.Threading.Tasks.Task NotifyTaskChange(BandBaaajaVivaah.Data.Models.Task task, TaskUpdateEvent.Types.UpdateType updateType)
        {
            var taskDetails = CreateTaskDetails(task);
            await TaskUpdateGrpcService.NotifyTaskChange(
                task.WeddingId,
                updateType,
                taskDetails);
        }

        private TaskDetails CreateTaskDetails(BandBaaajaVivaah.Data.Models.Task task)
        {
            return new TaskDetails
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description ?? string.Empty,
                Status = task.Status ?? string.Empty,
                DueDate = task.DueDate.HasValue
                        ? Timestamp.FromDateTime(task.DueDate.Value.ToUniversalTime())
                        : null,
                WeddingId=task.WeddingId    
            };
        }
    }
}
