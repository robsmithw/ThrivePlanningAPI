using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThrivePlanningAPI
{
    public class SeedDataLoader
    {
        readonly IAmazonDynamoDB _dynamoClient;
        readonly IConfiguration _config;
        readonly IDynamoDBContext _dbContext;
        readonly IWebHostEnvironment _hostingEnvironment;

        public SeedDataLoader(
            IAmazonDynamoDB dynamoClient,
            IDynamoDBContext dynamoContext,
            IConfiguration config,
            IWebHostEnvironment environment)
        {
            _dynamoClient = dynamoClient;
            _config = config;
            _dbContext = dynamoContext;
            _hostingEnvironment = environment;
        }

        public async Task SeedAsync()
        {
            var tableName = _config["DynamoDB:TableName"];
            var response = await _dynamoClient.ListTablesAsync(tableName);
            if (!response.TableNames.Contains(tableName))
            {
                await TableCreator.CreateTableIfNotExists(tableName, _dynamoClient);
                // add/update data in table
                //await TableCreator.LoadData<Models.Employer>("", (DynamoDBContext)_dbContext);

                Table employerTable = Table.LoadTable(_dynamoClient, tableName, DynamoDBEntryConversion.V2);
            }
        }
    }
}
