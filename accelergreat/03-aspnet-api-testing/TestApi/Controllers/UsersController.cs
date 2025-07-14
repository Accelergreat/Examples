using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestApi.Models;
using TestApi.Services;

namespace TestApi.Controllers;

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

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
            return BadRequest("Name is required");

        if (string.IsNullOrWhiteSpace(user.Email))
            return BadRequest("Email is required");

        var createdUser = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
            return BadRequest("Name is required");

        if (string.IsNullOrWhiteSpace(user.Email))
            return BadRequest("Email is required");

        var updatedUser = await _userService.UpdateUserAsync(id, user);
        if (updatedUser == null)
            return NotFound($"User with ID {id} not found");

        return Ok(updatedUser);
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