using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventAz.WebAPI.Middleware;

public sealed class SwaggerAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        var controller = method.DeclaringType;
        var methodAttributes = method.GetCustomAttributes(true);
        var controllerAttributes = controller?.GetCustomAttributes(true) ?? [];

        if (methodAttributes.OfType<AllowAnonymousAttribute>().Any() ||
            controllerAttributes.OfType<AllowAnonymousAttribute>().Any())
            return;

        var requiresAuthorization = methodAttributes.OfType<IAuthorizeData>().Any() ||
                                    controllerAttributes.OfType<IAuthorizeData>().Any();
        if (!requiresAuthorization)
            return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            }
        ];
    }
}
