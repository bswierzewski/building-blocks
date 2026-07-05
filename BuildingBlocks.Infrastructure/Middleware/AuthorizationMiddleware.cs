using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Interfaces;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Wolverine.Attributes;
using Wolverine.Configuration;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// A Wolverine <see cref="SyncFrame"/> that emits authentication and permission checks directly
/// into the generated handler source code at startup. Because the checks are written as plain C#
/// at code-generation time, there is zero runtime reflection overhead.
/// </summary>
internal sealed class AuthorizationFrame : SyncFrame
{
    // Required permissions that the current user must hold. Empty means "authenticated only".
    private readonly string[] _permissions;
    // Resolved at code-generation time via FindVariables; holds the ICurrentUser variable reference.
    private Variable _currentUser = null!;

    internal AuthorizationFrame(string[] permissions)
    {
        _permissions = permissions;
    }

    /// <summary>
    /// Tells Wolverine which variables this frame needs. The <see cref="ICurrentUser"/> variable
    /// will be resolved from the IoC container and injected into the generated method.
    /// </summary>
    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _currentUser = chain.FindVariable(typeof(ICurrentUser));
        yield return _currentUser;
    }

    /// <summary>
    /// Writes the authentication and optional permission guards into the generated handler method,
    /// then delegates to the next frame in the pipeline.
    /// </summary>
    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        // Throw 401 Unauthorized if the current user is not authenticated.
        writer.WriteLine($"if (!{_currentUser.Usage}.{nameof(ICurrentUser.IsAuthenticated)})");
        writer.WriteLine($"    throw new {typeof(UnauthorizedAccessException).FullNameInCode()}();");

        // Throw 403 Forbidden when specific permissions are required but the user does not hold them.
        if (_permissions.Length > 0)
        {
            var checks = _permissions.Select(p => $"!{_currentUser.Usage}.{nameof(ICurrentUser.HasPermission)}(\"{p}\")");
            var condition = string.Join(" || ", checks);

            writer.WriteLine($"if ({condition})");
            writer.WriteLine($"    throw new {typeof(ForbiddenAccessException).FullNameInCode()}();");
        }

        // Continue with the next frame (the actual handler invocation).
        Next?.GenerateCode(method, writer);
    }
}

/// <summary>
/// Marks a Wolverine handler class or method as requiring an authenticated user.
/// Permissions are baked into generated handler code at startup — zero runtime reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class AuthorizeAttribute : ModifyChainAttribute
{
    /// <summary>
    /// Optional list of permission strings the current user must satisfy.
    /// When empty the check is authentication-only (any authenticated user is allowed).
    /// </summary>
    public string[] Permissions { get; set; } = [];

    /// <summary>
    /// Inserts an <see cref="AuthorizationFrame"/> at the front of the handler pipeline,
    /// ensuring auth checks run before any business logic.
    /// </summary>
    public override void Modify(IChain chain, GenerationRules rules, IServiceContainer container)
    {
        chain.Middleware.Insert(0, new AuthorizationFrame(Permissions));
    }
}
