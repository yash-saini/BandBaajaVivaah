using BandBaaajaVivaah.Data.Repositories;
using BandBaajaVivaah.Contracts.DTOs;

namespace BandBaajaVivaah.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksByWeddingIdAsync(int weddingId, int userId);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, int userId);
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

            var task = new BandBaaajaVivaah.Data.Models.Task // 'Task' now resolves correctly to BandBaajaVivaah.Models.Task
            {
                Title = taskDto.Title,
                WeddingId = taskDto.WeddingID
            };

            await _unitOfWork.Tasks.AddAsync(task);
            await _unitOfWork.CompleteAsync();

            return new TaskDto
            {
                TaskID = task.TaskId,
                Title = task.Title,
                Status = task.Status
            };
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByWeddingIdAsync(int weddingId, int userId)
        {
            var wedding = await _unitOfWork.Weddings.GetByIdAsync(weddingId);
            if (wedding == null || wedding.OwnerUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view tasks for this wedding.");
            }

            var tasks = await _unitOfWork.Tasks.FindAllAsync(t => t.WeddingId == weddingId); // Requires custom repository method
            return tasks.Select(t => new TaskDto
            {
                TaskID = t.TaskId,
                Title = t.Title,
                Status = t.Status
            });
        }
    }
}
