using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Interfaces;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Wolverine.Configuration;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// A Wolverine <see cref="SyncFrame"/> that emits an approval-status check directly into the
/// generated handler source code at startup. The <c>status</c> claim in the JWT is compared
/// against <c>approved</c> (set by an admin via Clerk public metadata). Non-approved users
/// receive a 403 Forbidden response.
/// </summary>
internal sealed class ApprovedUserFrame : SyncFrame
{
    private Variable _currentUser = null!;

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _currentUser = chain.FindVariable(typeof(ICurrentUser));
        yield return _currentUser;
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        // Only enforce approval for authenticated users (HTTP requests with a JWT).
        // Background services and scheduled jobs have no HTTP context, so IsAuthenticated
        // is false and the check is skipped — they are not subject to user-approval gating.
        writer.WriteLine(
            $"if ({_currentUser.Usage}.{nameof(ICurrentUser.IsAuthenticated)} && {_currentUser.Usage}.{nameof(ICurrentUser.Status)} != {typeof(UserStatus).FullNameInCode()}.{nameof(UserStatus.Approved)})");
        writer.WriteLine(
            $"    throw new {typeof(ForbiddenAccessException).FullNameInCode()}();");

        Next?.GenerateCode(method, writer);
    }
}

/// <summary>
/// A Wolverine policy that prepends <see cref="ApprovedUserFrame"/> to every handler chain,
/// ensuring that only admin-approved users can invoke any message handler or HTTP endpoint.
/// Register it via <c>opts.Policies.Add&lt;RequireApprovedUserPolicy&gt;()</c> or the
/// <c>opts.RequireApprovedUsers()</c> extension in projects that need this guard.
/// </summary>
public sealed class RequireApprovedUserPolicy : IChainPolicy
{
    public void Apply(IReadOnlyList<IChain> chains, GenerationRules rules, IServiceContainer container)
    {
        foreach (var chain in chains)
            chain.Middleware.Insert(0, new ApprovedUserFrame());
    }
}
