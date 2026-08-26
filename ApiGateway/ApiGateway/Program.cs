using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
    ?? throw new InvalidOperationException(
        "Keycloak:Authority is not configured.");

var keycloakAudience = builder.Configuration["Keycloak:Audience"]
    ?? throw new InvalidOperationException(
        "Keycloak:Audience is not configured.");

var keycloakIssuer = builder.Configuration["Keycloak:Issuer"]
    ?? throw new InvalidOperationException(
        "Keycloak:Issuer is not configured.");

var requireHttpsMetadata =
    builder.Configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;

        options.RequireHttpsMetadata = requireHttpsMetadata;

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = keycloakIssuer,
            ValidAudience = keycloakAudience,

            RoleClaimType = "roles",

            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    $"JWT authentication failed: {context.Exception}");

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("===== JWT CLAIMS =====");

                foreach (var claim in context.Principal?.Claims ?? [])
                {
                    Console.WriteLine(
                        $"Type: {claim.Type} | Value: {claim.Value}");
                }

                Console.WriteLine("===== ROLES =====");

                Console.WriteLine(
                    $"is_user: {context.Principal?.IsInRole("user")}");

                Console.WriteLine(
                    $"is_admin: {context.Principal?.IsInRole("admin")}");

                Console.WriteLine(
                    $"is_moderator: {context.Principal?.IsInRole("moderator")}");

                Console.WriteLine("======================");

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("admin");
    });

    options.AddPolicy("Moderator", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("moderator");
    });

    // Admin OR moderator
    options.AddPolicy("AdminOrModerator", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("admin", "moderator");
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(transformContext =>
        {
            var user = transformContext.HttpContext.User;

            Console.WriteLine("===== YARP USER CLAIMS =====");

            foreach (var claim in user.Claims)
            {
                Console.WriteLine(
                    $"Type: {claim.Type} | Value: {claim.Value}");
            }

            Console.WriteLine("============================");

            // Удаляем headers, которые мог прислать клиент
            transformContext.ProxyRequest.Headers.Remove("X-User-Id");
            transformContext.ProxyRequest.Headers.Remove("X-User-Name");
            transformContext.ProxyRequest.Headers.Remove("X-User-Roles");

            if (user.Identity?.IsAuthenticated != true)
            {
                return ValueTask.CompletedTask;
            }

            var userId = user.FindFirst("sub")?.Value;
            var username = user.FindFirst("preferred_username")?.Value;

            var roles = user
                .FindAll("roles")
                .Select(x => x.Value);

            if (!string.IsNullOrEmpty(userId))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                    "X-User-Id",
                    userId);
            }

            if (!string.IsNullOrEmpty(username))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                    "X-User-Name",
                    username);
            }

            transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                "X-User-Roles",
                string.Join(",", roles));

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("OK"));

app.MapReverseProxy();

app.Run();
