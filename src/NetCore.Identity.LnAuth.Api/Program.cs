using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetCore.Identity.LnAuth.Api.Configuration;
using NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using NetCore.Identity.LnAuth.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(AppDbContext))));

// Add AuthSettings globally as Singleton
var authSettings = builder.Configuration.GetSection(AuthSettings.SectionName)
    .Get<AuthSettings>() ?? default!;
builder.Services.AddSingleton(authSettings);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LnLoginHandler>());
builder.Services.AddControllers();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 2;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHealthChecks();

// Add Asp.Net Identity
builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<AppRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(AuthSettings.RefreshTokenProviderName);
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromSeconds(authSettings.RefreshTokenExpireSeconds);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = authSettings.ValidateIssuer,
        ValidateAudience = authSettings.ValidateAudience,
        ValidateLifetime = authSettings.ValidateLifetime,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = true,
        ValidIssuer = authSettings.Issuer,
        ValidAudience = authSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.SecretKey)),
        ClockSkew = TimeSpan.FromSeconds(0)
    };
});

// Cors settings
builder.Services.AddCors(options =>
{
    options.AddPolicy("webAppRequests", builder =>
    {
        builder.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true) // allow any origin
            .AllowCredentials();
    });
});
// Auth on Swagger
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo() { Title = "App Api", Version = "v1" });
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    config.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// SPA
builder.Services.AddSpaStaticFiles(configuration => configuration.RootPath = "spa/dist");

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseRouting();
app.UseCors("webAppRequests");
app.MapHub<LightningAuthHub>("/hubs/lightning-auth");
app.MapHealthChecks("/status");
app.UseStaticFiles();
app.UseSpaStaticFiles();

await DatabaseInitializer.RunMigrationsAsync(app.Services);

app.UseSpa(spa => { spa.Options.SourcePath = "spa/"; });

app.Run();

public partial class Program {}
