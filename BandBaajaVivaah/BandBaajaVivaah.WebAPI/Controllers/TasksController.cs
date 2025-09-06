using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Services; // Assuming you create an ITaskService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BandBaajaVivaah.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    // We will create an ITaskService similar to how we created IUserService
    //private readonly ITaskService _taskService;

    //public TasksController(ITaskService taskService)
    //{
    //    _taskService = taskService;
    //}

   // GET: api/tasks/wedding/5
   // [HttpGet("wedding/{weddingId}")]
   // public async Task<IActionResult> GetTasksForWedding(int weddingId)
   // {
   //     You would add logic here to ensure the logged -in user owns this wedding
   //    var tasks = await _taskService.GetTasksByWeddingIdAsync(weddingId);
   //     return Ok(tasks);
   // }

   // POST: api/tasks
   //[HttpPost]
   // public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto taskDto)
   // {
   //     Add ownership validation here as well
   //     var newTask = await _taskService.CreateTaskAsync(taskDto);
   //     return CreatedAtAction(nameof(GetTasksForWedding), new { weddingId = newTask.WeddingID }, newTask);
   // }
}