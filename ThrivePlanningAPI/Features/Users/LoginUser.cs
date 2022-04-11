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
    public class LoginUser
    {
        public class Command : IRequest<LoginUserResponse>
        {
            public Command(LoginRequest login)
            {
                Login = login;
            }
            public LoginRequest Login { get; set; }
        }

        public class LoginUserResponse : BaseResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public List<string> Groups { get; set; }
        }

        public class Handler : IRequestHandler<Command, LoginUserResponse>
        {
            private readonly ICognitoUserManagement _cognitoUserManagement;
            private readonly ILogger<LoginUser> _logger;
            private readonly ThrivePlanContext _context;

            public Handler(ICognitoUserManagement cognitoUserManagement, ILogger<LoginUser> logger, ThrivePlanContext context)
            {
                _cognitoUserManagement = cognitoUserManagement;
                _logger = logger;
                _context = context;
            }

            public async Task<LoginUserResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Attempting to login user");
                var result = new LoginUserResponse();
                var loginRequest = request.Login;

                try
                {
                    // attempt to sign in user through cognito
                    var loginResponse = await _cognitoUserManagement.AdminAuthenticateUserAsync(loginRequest.Username,
                        loginRequest.Username,
                        cancellationToken);

                    var groups = await _cognitoUserManagement.AdminGetUserGroupsAsync(loginRequest.Username, cancellationToken);

                    result.Successful = true;
                    result.Error = String.Empty;
                    result.AccessToken = loginResponse.AuthenticationResult.AccessToken;
                    result.RefreshToken = loginResponse.AuthenticationResult.RefreshToken;
                    result.Groups = groups;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred when trying to login user.");
                    result.Successful = false;
                    result.Error = "Failed to login.";
                }

                return result;
            }
        }
    }
}
