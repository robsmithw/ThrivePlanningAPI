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

namespace ThrivePlanningAPI.Features.Employer
{
    public class RegisterEmployer
    {
        public class Command : IRequest<RegisterEmployerResult>
        {
            public Command(EmployerRequest employer)
            {
                Employer = employer;
            }
            public EmployerRequest Employer { get; set; }
        }

        public class RegisterEmployerResult
        {
            public RegisterEmployerResult(bool successful, string error)
            {
                Successful = successful;
                Error = error;
            }
            public bool Successful { get; set; }
            public string Error { get; set; }
        }

        public class Handler : IRequestHandler<Command, RegisterEmployerResult>
        {
            private readonly IAmazonDynamoDB _dynamo;
            private readonly ILogger<RegisterEmployer> _logger;
            private readonly string _tableName;

            public Handler(IAmazonDynamoDB dynamo, IConfiguration configuration, ILogger<RegisterEmployer> logger)
            {
                _dynamo = dynamo;
                _tableName = configuration["DynamoDB:TableName"];
                _logger = logger;
            }

            public async Task<RegisterEmployerResult> Handle(Command request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Creating employee.");
                var dateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
                var employerId = Guid.NewGuid();
                var result = new RegisterEmployerResult(false, "Unknown Error");
                
                try
                {
                    var putItemRequest = new PutItemRequest
                    {
                        TableName = _tableName,
                        Item = new Dictionary<string, AttributeValue>
                        {
                            {nameof(Models.Employer.HashKey), new AttributeValue {S = $"employer_{employerId}"}},
                            {nameof(Models.Employer.RangeKey), new AttributeValue {S = $"employer"}},
                            {nameof(Models.Employer.FirstName), new AttributeValue {S = request.Employer.FirstName}},
                            {nameof(Models.Employer.LastName), new AttributeValue {S = request.Employer.LastName}},
                            {nameof(Models.Employer.Email), new AttributeValue {S = request.Employer.Email}},
                            {nameof(Models.Employer.Company), new AttributeValue {S = request.Employer.Company}},
                            {nameof(Models.Employer.PhoneNumber), new AttributeValue {S = request.Employer.PhoneNumber}},
                            {nameof(Models.Employer.IsConfirmed), new AttributeValue {BOOL = request.Employer.IsConfirmed}},
                            {nameof(Models.Employer.CreatedDate), new AttributeValue {S = DateTime.UtcNow.ToString(dateFormat)}},
                            {nameof(Models.Employer.ModifiedDate), new AttributeValue {S = DateTime.UtcNow.ToString(dateFormat)}},
                        },
                        ConditionExpression = $"attribute_not_exists({nameof(Models.Employer.Company)})"
                    };

                    _logger.LogTrace("PutItemRequest: {@putItemRequest}", putItemRequest);

                    var response = await _dynamo.PutItemAsync(putItemRequest, cancellationToken);
                    result.Successful = true;
                }
                catch(ConditionalCheckFailedException ex)
                {
                    string error = "Employer with the same company already exists";
                    _logger.LogError(error, ex);
                    result.Error = error;
                }

                return result;
            }
        }
    }
}
