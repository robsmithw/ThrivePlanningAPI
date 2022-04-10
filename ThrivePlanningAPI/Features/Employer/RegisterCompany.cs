using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThrivePlanningAPI.Models;
using ThrivePlanningAPI.Models.Entities;
using ThrivePlanningAPI.Models.Requests;

namespace ThrivePlanningAPI.Features.Employer
{
    public class RegisterCompany
    {
        public class Command : IRequest<RegisterCompanyResult>
        {
            public Command(CompanyRequest company)
            {
                Company = company;
            }
            public CompanyRequest Company { get; set; }
        }

        public class RegisterCompanyResult
        {
            public RegisterCompanyResult(bool successful, string error)
            {
                Successful = successful;
                Error = error;
            }
            public bool Successful { get; set; }
            public string Error { get; set; }
            public Guid CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Command, RegisterCompanyResult>
        {
            private readonly ILogger<RegisterCompany> _logger;
            private readonly ThrivePlanContext _context;

            public Handler(ILogger<RegisterCompany> logger, ThrivePlanContext context)
            {
                _logger = logger;
                _context = context;
            }

            public async Task<RegisterCompanyResult> Handle(Command request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Creating company.");
                var result = new RegisterCompanyResult(false, "Unknown Error");
                var companyRequest = request.Company;

                var newCompany = CreateCompany(companyRequest.CompanyAdminFirstName,
                    companyRequest.CompanyAdminLastName,
                    companyRequest.CompanyName,
                    companyRequest.Email,
                    companyRequest.PhoneNumber,
                    companyRequest.TaxId,
                    companyRequest.Industry);

                await _context.Company.AddAsync(newCompany, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                result.Successful = true;
                result.Error = String.Empty;
                result.CompanyId = newCompany.Id;

                return result;
            }

            private Company CreateCompany(string companyAdminFirstName, 
                string companyAdminLastName, 
                string companyName, 
                string email, 
                string phoneNumber, 
                string taxId, 
                string industry)
            {
                return new Company()
                {
                    Id = Guid.NewGuid(),
                    CompanyAdminFirstName = companyAdminFirstName,
                    CompanyAdminLastName = companyAdminLastName,
                    CompanyName = companyName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    TaxId = taxId,
                    Industry = industry,
                    CreatedDate = DateTime.UtcNow,
                    IsConfirmed = false
                };
            }
        }
    }
}
