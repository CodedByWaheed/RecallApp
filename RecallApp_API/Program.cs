using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecallApp_API.Middleware;
using RecallApp_BL.Services;
using RecallApp_BL.Services.Interfaces;
using RecallApp_BL.Services.Security;
using RecallApp_DL.Data;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Repositories;
using RecallApp_DL.Repositories.Interfaces;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DbUnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DbUnitOfWork>());
builder.Services.AddScoped<IDbTransactionContext>(sp => sp.GetRequiredService<DbUnitOfWork>());
builder.Services.AddScoped<IDbConnectionFactory>(sp =>
{
    IConfiguration Configuraion = sp.GetRequiredService<IConfiguration>();
    string connectionString = Configuraion.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("RecallDb connection string is not configured.");
    return new DbConnectionFactory(connectionString);
});
builder.Services.AddScoped<IEncryptionService>(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    string base64Key = configuration["Encryption:Key"]
        ?? throw new InvalidOperationException("Encryption key is not configured.");
    return new EncryptionService(base64Key);
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISavedItemRepository, SavedItemRepository>();
builder.Services.AddScoped<IEncryptedContentRepository, EncryptedContentRepository>();
builder.Services.AddScoped<ISavedItemService, SavedItemService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("RecallPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7135"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();

string jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? 
    throw new InvalidOperationException("JWT secret key is not configured.");

string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? 
    throw new InvalidOperationException("JWT issuer is not configured.");

string jwtAudience = builder.Configuration["Jwt:Audience"] ?? 
    throw new InvalidOperationException("JWT audience is not configured.");

byte[] keyBytes = Convert.FromBase64String(jwtSecretKey);

if (keyBytes.Length != 32)
{
    throw new InvalidOperationException("JWT secret key must be exactly 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService,CurrentUserService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ValidUser", policy =>
    {
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
        {
            string? userIdValue = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
            .Value?? context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?
            .Value;

            return Guid.TryParse(userIdValue, out _);
        });
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General limit: 100 requests per minute.
    //options.AddPolicy("GeneralPolicy", httpContext =>
    //{
    //    string partitionKey =httpContext.User.Identity?.IsAuthenticated == true
    //            ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
    //                ?? httpContext.User.FindFirstValue("sub")
    //                ?? httpContext.Connection.RemoteIpAddress?.ToString()
    //                ?? "unknown"
    //            : httpContext.Connection.RemoteIpAddress?.ToString()
    //                ?? "unknown";

    //    return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
    //        _ => new FixedWindowRateLimiterOptions
    //        {
    //            PermitLimit = 100,
    //            Window = TimeSpan.FromMinutes(1),
    //            QueueLimit = 0,
    //            AutoReplenishment = true
    //        });
    //});

    options.GlobalLimiter =PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            string partitionKey =
                httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? httpContext.User.FindFirstValue("sub")
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown"
                    : httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });


    // Login and registration: 5 requests per minute per IP.
    options.AddPolicy("AuthPolicy", httpContext =>
    {
        string partitionKey =httpContext.Connection.RemoteIpAddress?.ToString()?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // Refresh token: 10 requests per minute per IP.
    options.AddPolicy("RefreshPolicy", httpContext =>
    {
        string partitionKey =httpContext.Connection.RemoteIpAddress?.ToString()?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});


//builder.Services.AddAuthorization();

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<IAuthService>(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();

    return new AuthService(
        userRepository: sp.GetRequiredService<IUserRepository>(),
        refreshTokenRepository: sp.GetRequiredService<IRefreshTokenRepository>(),
        passwordService: sp.GetRequiredService<PasswordService>(),
        refreshTokenService: sp.GetRequiredService<RefreshTokenService>(),
        jwtSecretKey: jwtSecretKey,
        jwtIssuer: jwtIssuer,
        jwtAudience: jwtAudience
    );
});
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // ===============================
    // 1) Define the JWT Bearer security scheme
    // ===============================
    //
    // This tells Swagger that our API uses JWT Bearer authentication
    // through the HTTP Authorization header.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // The name of the HTTP header where the token will be sent.
        Name = "Authorization",


        // Indicates this is an HTTP authentication scheme.
        Type = SecuritySchemeType.Http,


        // Specifies the authentication scheme name.
        // Must be exactly "Bearer" for JWT Bearer tokens.
        Scheme = "Bearer",


        // Optional metadata to describe the token format.
        BearerFormat = "JWT",


        // Specifies that the token is sent in the request header.
        In = ParameterLocation.Header,


        // Text shown in Swagger UI to guide the user.
        Description = "Enter: Bearer {your JWT token}"
    });


    // ===============================
    // 2) Require the Bearer scheme for secured endpoints
    // ===============================
    //
    // This tells Swagger that endpoints protected by [Authorize]
    // require the Bearer token defined above.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                // Reference the previously defined "Bearer" security scheme.
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },


            // No scopes are required for JWT Bearer authentication.
            // This array is empty because JWT does not use OAuth scopes here.
            new string[] {}
        }
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("RecallPolicy");

app.UseAuthentication();

app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
