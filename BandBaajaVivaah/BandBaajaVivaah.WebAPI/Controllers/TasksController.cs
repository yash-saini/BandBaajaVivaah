using BandBaajaVivaah.Contracts.DTOs;
using BandBaajaVivaah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BandBaajaVivaah.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new InvalidOperationException("User ID claim is missing.");
        }
        return int.Parse(userIdClaim);
    }

    [HttpGet("wedding/{weddingId}")]
    public async Task<IActionResult> GetTasksForWedding(int weddingId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var tasks = await _taskService.GetTasksByWeddingIdAsync(weddingId, userId);
            return Ok(tasks);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto taskDto)
    {
        try
        {
            if (User.IsInRole("Admin"))
            {
                var createdGuest = await _taskService.CreateTasksAsAdminAsync(taskDto);
                return Ok(createdGuest);
            }
            else
            {
                var userId = GetCurrentUserId();
                var createdTask = await _taskService.CreateTaskAsync(taskDto, userId);
                return Ok(createdTask);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] CreateTaskDto updateDto)
    {
        bool success;
        if (User.IsInRole("Admin"))
        {
            success = await _taskService.UpdateTasksAsAdminAsync(taskId, updateDto);
        }
        else
        {
            var userId = GetCurrentUserId();
            success = await _taskService.UpdateTaskAsync(taskId, updateDto, userId);
        }
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        bool success;
        if (User.IsInRole("Admin"))
        {
            success = await _taskService.DeleteTasksAsAdminAsync(taskId);
        }
        else
        {
            var userId = GetCurrentUserId();
            success = await _taskService.DeleteTaskAsync(taskId, userId);
            if (!success)
            {
                return NotFound();
            }
        }
        return success ? NoContent() : NotFound();
    }
    
}