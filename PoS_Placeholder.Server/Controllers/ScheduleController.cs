using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoS_Placeholder.Server.Data;
using PoS_Placeholder.Server.Models;
using PoS_Placeholder.Server.Models.Dto;
using PoS_Placeholder.Server.Models.Enum;
using PoS_Placeholder.Server.Repositories;
using Stripe;

namespace PoS_Placeholder.Server.Controllers;

[Route("api/schedule")]
[ApiController]
public class ScheduleController : ControllerBase
{
    private readonly BusinessRepository _businessRepository;
    private readonly UserRepository _userRepository;
    private readonly UserWorkTimeRepository _userWorkTimeRepository;
    private readonly UserManager<User> _userManager;

    public ScheduleController(BusinessRepository businessRepository, UserManager<User> userManager, UserRepository userRepository, UserWorkTimeRepository userWorkTimeRepository)
    {
        _businessRepository = businessRepository;
        _userManager = userManager;
        _userRepository = userRepository;
        _userWorkTimeRepository = userWorkTimeRepository;
    }


    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> GetSchedules()
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        // Get the current user's business ID
        var businessId = user.BusinessId;

        // Retrieve all employees associated with the business
        var employees = await _userRepository.GetEmployeesByBusinessIdAsync(businessId);
        if (employees == null || !employees.Any())
        {
            return NotFound("No employees found for this business.");
        }

        // Extract the user IDs of the employees
        var employeeIds = employees.Select(e => e.Id).ToList();

        // Retrieve schedules for the employees
        var schedules = await _userWorkTimeRepository.GetWhereAsync(schedule => employeeIds.Contains(schedule.UserId));

        if (schedules == null || !schedules.Any())
        {
            return NotFound("No schedules found for this business.");
        }

        return Ok(schedules); // Return the list of schedules
    }


    [HttpPost("/user/{employee_id}/schedule")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> AssignSchedule(string employee_id, [FromBody] AssignScheduleDto scheduleDto)
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        // Get the current user's business ID
        var business_id = user.BusinessId;

        var employee = await _userRepository.GetEmployeeByIdAndBusinessAsync(employee_id, business_id);
        if (employee == null)
        {
            return NotFound("Employee not found or does not belong to the business.");
        }

        // Validate input data
        if (scheduleDto.StartTime >= scheduleDto.EndTime)
            return BadRequest("Start time must be before end time.");

        if (scheduleDto.BreakStart >= scheduleDto.BreakEnd)
            return BadRequest("Break start time must be before end time.");

        // Create the schedule
        var userWorkTime = new UserWorkTime
        {
            Day = scheduleDto.Day,
            StartTime = scheduleDto.StartTime,
            EndTime = scheduleDto.EndTime,
            BreakStart = scheduleDto.BreakStart,
            BreakEnd = scheduleDto.BreakEnd,
            UserId = employee_id
        };

        try
        {
            _userWorkTimeRepository.Add(userWorkTime);
            await _userWorkTimeRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(AssignSchedule), new { id = userWorkTime.Id }, userWorkTime); // 201 Created
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating the schedule.");
        }
    }
    // Endpoint to update a schedule
    [HttpPut("{schedule_id:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> UpdateSchedule(int schedule_id, [FromBody] UpdateScheduleDto scheduleDto)
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }
        // Validate schedule existence
        var schedule = await _userWorkTimeRepository.GetByIdAsync(schedule_id);
        if (schedule == null)
        {
            return NotFound("Schedule not found.");
        }

        // Validate user belongs to the business
        var employee = await _userRepository.GetUserByIdAsync(schedule.UserId);
        if (employee == null)
        {
            return NotFound("Employee not found.");
        }
        // Validate business existence
        var business = await _businessRepository.GetByIdAsync(employee.BusinessId);
        if (business == null)
        {
            return NotFound("Could not find the business.");
        }

        if(business.Id != user.BusinessId)
        {
            return BadRequest("Employee does not belong to business");
        }


        // Validate input data
        if (scheduleDto.StartTime >= scheduleDto.EndTime)
            return BadRequest("Start time must be before end time.");

        // Update the schedule
        if (scheduleDto.Day != default) schedule.Day = scheduleDto.Day ?? schedule.Day;
        if (scheduleDto.StartTime != default) schedule.StartTime = scheduleDto.StartTime ?? schedule.StartTime;
        if (scheduleDto.EndTime != default) schedule.EndTime = scheduleDto.EndTime ?? schedule.EndTime;
        if (scheduleDto.BreakStart != default) schedule.BreakStart = scheduleDto.BreakStart;
        if (scheduleDto.BreakEnd != default) schedule.BreakEnd = scheduleDto.BreakEnd;


        try
        {
            _userWorkTimeRepository.Update(schedule);
            await _userWorkTimeRepository.SaveChangesAsync();

            return Ok(schedule); // 200 OK
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating the schedule.");
        }
    }

    // Endpoint to delete a schedule
    [HttpDelete("{schedule_id:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> DeleteSchedule(int schedule_id)
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }
        // Validate schedule existence
        var schedule = await _userWorkTimeRepository.GetByIdAsync(schedule_id);
        if (schedule == null)
        {
            return NotFound("Schedule not found.");
        }

        // Validate user belongs to the business
        var employee = await _userRepository.GetUserByIdAsync(schedule.UserId);
        if (employee == null)
        {
            return NotFound("Employee not found.");
        }
        // Validate business existence
        var business = await _businessRepository.GetByIdAsync(employee.BusinessId);
        if (business == null)
        {
            return NotFound("Could not find the business.");
        }

        if (business.Id != user.BusinessId)
        {
            return BadRequest("Employee does not belong to business");
        }

        try
        {
            _userWorkTimeRepository.Remove(schedule);
            await _userWorkTimeRepository.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while deleting the schedule.");
        }
    }
}

