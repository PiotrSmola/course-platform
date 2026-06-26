using System.Reflection;
using MediatR;

namespace CoursePlatform.Application.Common.Behaviours;

public class TrimmingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TrimStrings(request);
        return await next();
    }

    private static void TrimStrings(object obj)
    {
        if (obj == null) return;

        var type = obj.GetType();
        if (type.IsClass && type != typeof(string))
        {
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType == typeof(string) && prop.CanWrite && prop.CanRead)
                {
                    var value = (string?)prop.GetValue(obj);
                    if (value != null)
                    {
                        prop.SetValue(obj, value.Trim());
                    }
                }
            }
        }
    }
}
