using Microsoft.AspNetCore.Mvc;
using MediatR;

using System;
using System.Threading.Tasks;

using ThrivePlanningAPI.Models.Requests;

namespace ThrivePlanningAPI.Features.Users
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> RegisterUser([FromBody] UserRequest user)
        {
            var response = await _mediator.Send(new CreateUser.Command(user));

            if (!response.Successful)
            {
                return BadRequest(response.Error);
            }

            return Ok(response.UserId);
        }
    }
}

