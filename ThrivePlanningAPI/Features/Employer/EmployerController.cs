using Microsoft.AspNetCore.Mvc;
using System;
using ThrivePlanningAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace ThrivePlanningAPI.Features.Employer
{
    [ApiController]
    [Route("[controller]")]
    public class EmployerController : Controller
    {
        private IMediator _mediator;
        public EmployerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> Register([FromBody] EmployerRequest employer)
        {
            var response = await _mediator.Send(new RegisterEmployer.Command(employer));

            if (!response.Successful)
            {
                return BadRequest(response.Error);
            }

            return Ok();
        }
    }
}
