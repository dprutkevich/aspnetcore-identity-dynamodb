using Microsoft.AspNetCore.Http;

namespace Identity.DynamoDb.Tests.Extensions;
public static class IResultExtensions
{
    public static object GetValueObject(this IResult result)
    {
        return result
            .GetType()
            .GetProperty("Value")!
            .GetValue(result)!;
    }

    public static object? GetField(this IResult result, string property)
    {
        var value = result.GetValueObject();
        return value.GetType().GetProperty(property)!.GetValue(value);
    }
}

