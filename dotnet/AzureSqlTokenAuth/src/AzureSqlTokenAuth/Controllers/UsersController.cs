using AzureSqlTokenAuth.Models;
using AzureSqlTokenAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureSqlTokenAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers(
        CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        long id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _userService.UpdateUserAsync(id, request, cancellationToken);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }
}