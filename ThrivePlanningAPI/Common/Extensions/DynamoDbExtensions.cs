using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThrivePlanningAPI.Common.Extensions
{
    public static class DynamoDbExtensions
    {
        public static TDynamoType GetDynamoValue<TDynamoType>(this Dictionary<string, AttributeValue> item, string attrName)
        {
            TDynamoType dynamoValue = default;
            if (item?.ContainsKey(attrName) == true && !item[attrName].NULL)
            {
                //todo: add more to this, create an enum with dynamo types.
                if (typeof(TDynamoType) == typeof(string))
                {
                    dynamoValue = (TDynamoType)(object)item[attrName]?.S;
                }
                else if (typeof(TDynamoType) == typeof(int))
                {
                    if (int.TryParse(item[attrName]?.N, out var value))
                    {
                        dynamoValue = (TDynamoType)(object)value;
                    }
                }
                else if (typeof(TDynamoType) == typeof(bool))
                {
                    bool value = item[attrName]?.BOOL ?? false;
                    dynamoValue = (TDynamoType)(object)value;
                }
                else if (typeof(TDynamoType) == typeof(long))
                {
                    if (long.TryParse(item[attrName]?.N, out var value))
                    {
                        dynamoValue = (TDynamoType)(object)value;
                    }
                }
                else if (typeof(TDynamoType) == typeof(Guid))
                {
                    if (Guid.TryParse(item[attrName]?.S, out Guid value))
                    {
                        dynamoValue = (TDynamoType)(object)value;
                    }
                }
                else if (typeof(TDynamoType) == typeof(AttributeValue))
                {
                    dynamoValue = (TDynamoType)(object)item[attrName];
                }

                else if (typeof(TDynamoType) == typeof(Dictionary<string, AttributeValue>))
                {
                    dynamoValue = (TDynamoType)(object)item[attrName].M;
                }

            }
            return dynamoValue;
        }

        public static Dictionary<string, AttributeValue> GetDynamoMap(this Dictionary<string, AttributeValue> item, string attrName)
        {
            return item.GetDynamoValue<Dictionary<string, AttributeValue>>(attrName);
        }

        public static bool AttributeValueIsNotNull(this Dictionary<string, AttributeValue> item, string index)
        {
            return item.ContainsKey(index) && !item[index].NULL;
        }
    }

}
