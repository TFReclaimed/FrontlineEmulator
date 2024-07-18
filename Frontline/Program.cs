using FastEndpoints;
using FastEndpoints.Swagger;
using Frontline.Auth;
using Frontline.Data;
using Frontline.Data.Repositories;
using Frontline.Game;
using Frontline.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using NSwag;

RulesetParser.Initialize();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(config.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = config.GetConnectionString("connection");

builder.Services.AddDbContext<AppDb>(o =>
{
    o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGuildRepository, GuildRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddHttpLogging(_ => { });

builder.Services.AddFastEndpoints();

builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(SessionAuth.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SessionAuth>(SessionAuth.SchemeName, null);

builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = false;
    o.DocumentSettings = s =>
    {
        s.AddAuth(SessionAuth.SchemeName, new OpenApiSecurityScheme
        {
            Name = SessionAuth.SessionIdHeaderName,
            In = OpenApiSecurityApiKeyLocation.Header,
            Type = OpenApiSecuritySchemeType.ApiKey
        });
    };
});

var app = builder.Build();

app.UseHttpLogging();

app.UseAuthorization();

app.UseFastEndpoints();

app.UseSwaggerGen();

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
});

app.Run();