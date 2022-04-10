using Microsoft.AspNetCore.Mvc;
using System;
using ThrivePlanningAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ThrivePlanningAPI.Models.Requests;

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
        public async Task<ActionResult<Guid>> RegisterCompany([FromBody] CompanyRequest company)
        {
            var response = await _mediator.Send(new RegisterCompany.Command(company));

            if (!response.Successful)
            {
                return BadRequest(response.Error);
            }

            return Ok(response.CompanyId);
        }
    }
}
