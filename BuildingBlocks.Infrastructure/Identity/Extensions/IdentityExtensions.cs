using System.Text;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Identity.Options;
using BuildingBlocks.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Infrastructure.Identity.Extensions;

/// <summary>
/// Registers authentication, authorization, and current-user services.
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// Adds JWT-based identity services backed by module-published roles.
    /// </summary>
    public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(IdentityOptions.SectionName);
        var identityOptions = section.Get<IdentityOptions>() ?? new IdentityOptions();

        services.AddHttpContextAccessor();
        services.AddOptions<IdentityOptions>()
            .Bind(section)
            .ValidateDataAnnotations();

        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.TryAddScoped<ICurrentTenant, CurrentTenant>();
        services.TryAddSingleton<RolePermissionService>();
        services.TryAddTransient<IClaimsTransformation, PermissionClaimsTransformation>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Preserve JWT claim names as-is so CurrentUser can read claims directly ('sub', 'roles').
                options.MapInboundClaims = false;

                options.TokenValidationParameters.NameClaimType = CustomClaimTypes.Sub;
                options.TokenValidationParameters.RoleClaimType = CustomClaimTypes.Roles;
                options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;

                options.Authority = identityOptions.Authority;

                options.TokenValidationParameters.ValidIssuer = identityOptions.Issuer;
                options.TokenValidationParameters.ValidateIssuer = !string.IsNullOrWhiteSpace(identityOptions.Issuer);

                // Clerk tokens do not include an 'aud' claim, so audience validation stays disabled unless explicitly configured.
                options.TokenValidationParameters.ValidAudience = identityOptions.Audience;
                options.TokenValidationParameters.ValidateAudience = !string.IsNullOrWhiteSpace(identityOptions.Audience);

                if (!string.IsNullOrWhiteSpace(identityOptions.SigningKey))
                {
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityOptions.SigningKey));
                }
            });

        services.AddAuthorization();

        return services;
    }
}
