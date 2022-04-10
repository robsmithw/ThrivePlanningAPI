using Microsoft.AspNetCore.Mvc;
using System;
using ThrivePlanningAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
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

