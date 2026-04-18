using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;
using sms.api.Models.DTOs;

namespace sms.api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;

        public UserController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _userManager.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetById(Guid id)
        {
            var user = await _userManager.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("username/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetByUsername(string username)
        {
            var user = await _userManager.GetByUsernameAsync(username);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetByEmail(string email)
        {
            var user = await _userManager.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("role/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetByRole(string role)
        {
            var users = await _userManager.GetUsersByRoleAsync(role);
            return Ok(users);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] LoginDTO loginDto)
        {
            var token = await _userManager.AuthenticateAsync(loginDto.Username, loginDto.Password);
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            return Ok(new { Token = token });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> Create([FromBody] CreateUserDTO userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if username is unique
            var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(userDto.Username);
            if (!isUsernameUnique)
            {
                return BadRequest(new { message = $"Username '{userDto.Username}' is already taken. Please choose a different username." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Username = userDto.Username,
                PhoneNumber = userDto.PhoneNumber,
                Address = userDto.Address,
                ProfilePicture = userDto.ProfilePicture,
                IsActive = true
            };

            var createdUser = await _userManager.CreateUserWithRoleAsync(user, userDto.Password, userDto.Role);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> Update(Guid id, [FromBody] UpdateUserDTO userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            // Check if username is unique (excluding current user)
            if (userDto.Username != user.Username)
            {
                var isUsernameUnique = await _userManager.IsUsernameUniqueAsync(userDto.Username, user.Id);
                if (!isUsernameUnique)
                {
                    return BadRequest(new { message = $"Username '{userDto.Username}' is already taken. Please choose a different username." });
                }
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Username = userDto.Username;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Address = userDto.Address;
            user.ProfilePicture = userDto.ProfilePicture;
            user.IsActive = userDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userManager.UpdateAsync(user);
            return Ok(updatedUser);
        }

        [HttpPatch("{id}/change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDTO passwordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _userManager.ChangePasswordAsync(id, passwordDto.CurrentPassword, passwordDto.NewPassword);
            if (!success)
                return BadRequest("Invalid current password");

            return NoContent();
        }

        [HttpPatch("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDTO passwordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _userManager.ResetPasswordAsync(id, passwordDto.NewPassword);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var user = await _userManager.GetByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Check if trying to delete own account
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == id.ToString())
                    return BadRequest(new { message = "Cannot delete your own account" });

                // Delete user - cascade delete will handle all related data
                var result = await _userManager.DeleteAsync(user.Id);
                if (!result)
                    return BadRequest(new { message = "Failed to delete user" });

                return Ok(new { message = $"User '{user.FullName}' and all associated data deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
            }
        }
    }
}
