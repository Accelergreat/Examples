using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<User>> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound($"User with email {email} not found");

        return Ok(user);
    }

    [HttpGet("{id:int}/validate")]
    public async Task<ActionResult<bool>> ValidateUser(int id)
    {
        var isValid = await _userService.ValidateUserAsync(id);
        return Ok(isValid);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required");

        try
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required");

        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound($"User with ID {id} not found");

        return NoContent();
    }
} 