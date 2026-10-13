using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BuildingBlocks.Infrastructure.OpenApi;

/// <summary>
/// Provides extension methods for configuring OpenAPI options to include standardized RFC 7807 Problem Details error
/// responses and schema enhancements.
/// </summary>
public static class OpenApiExtensions
{
    private const string ProblemDetailsSchemaName = nameof(HttpValidationProblemDetails);
    private const string ProblemJsonContentType = "application/problem+json";
    private const string DefaultErrorResponseDescription = "An error occurred";

    public static OpenApiOptions AddProblemDetailsResponses(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(HttpValidationProblemDetails)
                || schema is not OpenApiSchema openApiSchema)
                return Task.CompletedTask;

            openApiSchema.Description ??= "RFC 7807 validation-style problem details returned by the API.";
            openApiSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            openApiSchema.Properties.TryAdd("traceId", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "The trace identifier for request tracking."
            });

            openApiSchema.Properties.TryAdd("timestamp", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "date-time",
                Description = "Timestamp when the error occurred in UTC."
            });

            return Task.CompletedTask;
        });

        options.AddOperationTransformer(async (operation, context, cancellationToken) =>
        {
            operation.Responses ??= [];
            var problemDetailsSchema = await GetProblemDetailsSchemaAsync();

            operation.Responses.TryAdd(
                "default",
                CreateProblemDetailsResponse(DefaultErrorResponseDescription, problemDetailsSchema));

            foreach (var (statusCode, response) in operation.Responses.ToArray())
            {
                if (!IsErrorStatusCode(statusCode) || HasStructuredErrorSchema(response))
                    continue;

                operation.Responses[statusCode] = CreateProblemDetailsResponse(
                    string.IsNullOrWhiteSpace(response.Description)
                        ? DefaultErrorResponseDescription
                        : response.Description,
                    problemDetailsSchema);
            }

            async Task<IOpenApiSchema> GetProblemDetailsSchemaAsync()
            {
                if (context.Document is null)
                {
                    return await context.GetOrCreateSchemaAsync(
                        typeof(HttpValidationProblemDetails),
                        cancellationToken: cancellationToken);
                }

                context.Document.Components ??= new OpenApiComponents();
                context.Document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

                if (!context.Document.Components.Schemas.ContainsKey(ProblemDetailsSchemaName))
                {
                    context.Document.AddComponent(
                        ProblemDetailsSchemaName,
                        await context.GetOrCreateSchemaAsync(
                            typeof(HttpValidationProblemDetails),
                            cancellationToken: cancellationToken));
                }

                return new OpenApiSchemaReference(ProblemDetailsSchemaName, context.Document);
            }
        });

        return options;
    }

    /// <summary>
    /// Describes every enum as <c>oneOf</c> of <c>const</c> members instead of a plain <c>enum</c> list. The <c>title</c>
    /// is the member name and the <c>description</c> is its label from <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/> (the name when
    /// there is none), so generated clients get the labels of dropdown options without an endpoint serving them.
    /// </summary>
    public static OpenApiOptions AddEnumLabels(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddSchemaTransformer((schema, context, _) =>
        {
            var enumType = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;
            if (!enumType.IsEnum || schema is not OpenApiSchema openApiSchema || openApiSchema.Enum is not { Count: > 0 })
                return Task.CompletedTask;

            openApiSchema.OneOf = enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .OrderBy(field => field.MetadataToken)
                .Select(field => (IOpenApiSchema)new OpenApiSchema
                {
                    Const = field.Name,
                    Title = field.Name,
                    Description = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?.GetName() ?? field.Name
                })
                .ToList();
            openApiSchema.Enum = null;
            openApiSchema.Type = null;

            return Task.CompletedTask;
        });

        return options;
    }

    private static OpenApiResponse CreateProblemDetailsResponse(string description, IOpenApiSchema schema)
        => new()
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [ProblemJsonContentType] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };

    private static bool IsErrorStatusCode(string statusCode)
        => int.TryParse(statusCode, out var parsedStatusCode)
            && parsedStatusCode >= StatusCodes.Status400BadRequest;

    private static bool HasStructuredErrorSchema(IOpenApiResponse response)
        => response.Content?.Values.Any(mediaType => !IsEmptySchema(mediaType.Schema)) == true;

    private static bool IsEmptySchema(IOpenApiSchema? schema)
        => schema switch
        {
            null => true,
            OpenApiSchemaReference => false,
            OpenApiSchema openApiSchema => openApiSchema.Type is null
                && string.IsNullOrWhiteSpace(openApiSchema.Format)
                && openApiSchema.Items is null
                && openApiSchema.AdditionalProperties is null
                && (openApiSchema.Properties?.Count ?? 0) == 0
                && (openApiSchema.AllOf?.Count ?? 0) == 0
                && (openApiSchema.AnyOf?.Count ?? 0) == 0
                && (openApiSchema.OneOf?.Count ?? 0) == 0,
            _ => false
        };

    public static OpenApiOptions AddBearerSecurityScheme(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste your Clerk JWT token (without the 'Bearer ' prefix)"
            };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any();

            if (!hasAuthorize)
                return Task.CompletedTask;

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });

            return Task.CompletedTask;
        });

        return options;
    }
}
