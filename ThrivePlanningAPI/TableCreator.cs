using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThrivePlanningAPI
{
    public static class TableCreator
    {
        public static async Task CreateTableIfNotExists(string tableName, IAmazonDynamoDB dbClient)
        {
            Console.WriteLine("Getting list of tables");
            var listTablesResponse = await dbClient.ListTablesAsync();
            List<string> currentTables = listTablesResponse.TableNames;
            Console.WriteLine("Number of tables: " + currentTables.Count);
            if (!currentTables.Contains(tableName))
            {
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "HashKey",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "RangeKey",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = nameof(Models.Employer.HashKey),
                            KeyType = KeyType.HASH
                        },
                        new KeySchemaElement
                        {
                            AttributeName = nameof(Models.Employer.RangeKey),
                            KeyType = KeyType.RANGE
                        },
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 10,
                        WriteCapacityUnits = 5
                    },
                };

                var response = await dbClient.CreateTableAsync(request);

                Console.WriteLine("Table created with request ID: " + response.ResponseMetadata.RequestId);
            }
        }

        public static async Task LoadData<T>(string fileName, DynamoDBContext context)
        {
            var items = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(fileName));
            var config = new DynamoDBOperationConfig() { Conversion = DynamoDBEntryConversion.V2, SkipVersionCheck = true };
            foreach (var split in Split(items))
            {
                var batch = context.CreateBatchWrite<T>(config);
                batch.AddPutItems(split);
                await batch.ExecuteAsync();
            }
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> source, int count = 5)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / count)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }

}
