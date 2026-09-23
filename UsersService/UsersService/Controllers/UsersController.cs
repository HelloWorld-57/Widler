using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersService.Application.Commands;
using UsersService.Application.Interfaces;
using UsersService.DTOs.Requests;
using UsersService.DTOs.Response;

namespace UsersService.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUser _currentUser;

        private readonly IUserReconciliationService _reconciliation;

        public UsersController(IUserService userService, ICurrentUser currentUser, IUserReconciliationService reconciliation)
        {
            _userService = userService;
            _currentUser = currentUser;
            _reconciliation = reconciliation;
        }

        // GET api/users
        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken ct)
        {
            var users = await _userService.GetAllAsync(ct);
            return Ok(users);
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken ct)
        {
            var user = await _userService.GetByIdAsync(id, ct);
            return Ok(user);
        }

        // POST api/users
        //[HttpPost]
        //public async Task<IActionResult> Create(CreateUserRequest request)
        //{
        //    var userId = await _userService.CreateAsync(
        //        new CreateUserCommand(
        //            request.Id,
        //            request.Username,
        //            request.Email
        //        )
        //    );

        //    return CreatedAtAction(
        //        nameof(GetById),
        //        new { id = userId },
        //        null
        //    );
        //}

        // PUT api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Replace(string id, ReplaceUserRequest request, CancellationToken ct)
        {
            await _userService.ReplaceAsync(
                new ReplaceUserCommand(
                    id,
                    request.Name,
                    request.SecondName,
                    request.BirthDate
                ),
                ct
            );

            return NoContent();
        }

        // PATCH api/users/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(string id, UpdateUserRequest request, CancellationToken ct)
        {
            await _userService.UpdateAsync(
                new UpdateUserCommand(
                    id,
                    request.Name,
                    request.SecondName,
                    request.BirthDate
                ),
                ct
            );

            return NoContent();
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            await _userService.DeleteAsync(id, ct);
            return NoContent();
        }

        // debug
        [HttpGet("debug")]
        public async Task<IActionResult> DebugAsync()
        {
            await _reconciliation.ReconcileAsync(CancellationToken.None);

            return Ok(new
            {
                abc = "abc"
            });
        }

    }
}
