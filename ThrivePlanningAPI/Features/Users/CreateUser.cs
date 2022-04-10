using Amazon.CognitoIdentityProvider.Model;
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
using ThrivePlanningAPI.Features.UserManagement;
using ThrivePlanningAPI.Models;
using ThrivePlanningAPI.Models.Entities;
using ThrivePlanningAPI.Models.Requests;

namespace ThrivePlanningAPI.Features.Users
{
    public class CreateUser
    {
        public class Command : IRequest<CreateUserResult>
        {
            public Command(UserRequest user)
            {
                User = user;
            }
            public UserRequest User { get; set; }
        }

        public class CreateUserResult
        {
            public CreateUserResult(bool successful, string error)
            {
                Successful = successful;
                Error = error;
            }
            public bool Successful { get; set; }
            public string Error { get; set; }
            public Guid UserId { get; set; }
        }

        public class Handler : IRequestHandler<Command, CreateUserResult>
        {
            private readonly ICognitoUserManagement _cognitoUserManagement;
            private readonly ILogger<CreateUser> _logger;
            private readonly ThrivePlanContext _context;

            public Handler(ICognitoUserManagement cognitoUserManagement, ILogger<CreateUser> logger, ThrivePlanContext context)
            {
                _cognitoUserManagement = cognitoUserManagement;
                _logger = logger;
                _context = context;
            }

            public async Task<CreateUserResult> Handle(Command request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Creating user.");
                var result = new CreateUserResult(false, "Unknown Error");
                var userRequest = request.User;

                var emailAttribute = new AttributeType()
                {
                    Name = "email",
                    Value = userRequest.Email
                };

                // Create user in cognito
                await _cognitoUserManagement.AdminCreateUserAsync(userRequest.Username,
                    userRequest.Password,
                    new List<AttributeType> { emailAttribute });

                // Create user in database
                var newUser = CreateUser(userRequest.FirstName,
                    userRequest.LastName,
                    userRequest.CompanyId,
                    userRequest.Email,
                    userRequest.Phone,
                    userRequest.Type,
                    userRequest.Username);

                await _context.Users.AddAsync(newUser, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Send email request message to SQS (to send confirmation email)

                result.Successful = true;
                result.Error = String.Empty;
                result.UserId = newUser.Id;

                return result;
            }

            private User CreateUser(string firstName,
                string lastName,
                Guid companyId,
                string email,
                string phoneNumber,
                Models.Entities.UserType type,
                string username)
            {
                return new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    CompanyId = companyId,
                    Email = email,
                    Phone = phoneNumber,
                    Type = type,
                    Username = username,
                    CreatedDate = DateTime.UtcNow,
                    IsConfirmed = false
                };
            }
        }
    }
}
