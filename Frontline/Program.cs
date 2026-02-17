using FastEndpoints;
using FastEndpoints.Swagger;
using Frontline.Auth;
using Frontline.Data;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Frontline.Game;
using Frontline.Missions;
using Frontline.Options;
using Frontline.Services;
using Frontline.Xmpp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using NSwag;

RulesetParser.Initialize();
MissionsParser.Initialize();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

config.AddJsonFile("Products.json", false, true);
config.AddJsonFile("StarterItems.json", false, true);

builder.Services
    .AddConfiguredOptions<JwtOptions>(config)
    .AddConfiguredOptions<ChatOptions>(config)
    .AddConfiguredOptions<UrlOptions>(config)
    .AddConfiguredOptions<ProductOptions>(config)
    .AddConfiguredOptions<StarterItemOptions>(config);

builder.Services.AddSingleton<ITokenValidator, TokenValidator>();

var connectionString = config.GetConnectionString("connection");

builder.Services.AddDbContext<AppDb>(o =>
{
    o.UseNpgsql(connectionString);
});

builder.Services.AddHttpClient<IToyService>();

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGuildRepository, GuildRepository>();
builder.Services.AddScoped<IGuildMemberRepository, GuildMemberRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IActiveMissionRepository, ActiveMissionRepository>();
builder.Services.AddScoped<IFinishedMissionRepository, FinishedMissionRepository>();

builder.Services.AddSingleton<IToyService, ToyService>();
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.AddHostedService<XmppServer>();

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

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
}

app.UseHttpLogging();

app.UseAuthorization();

app.UseFastEndpoints();

app.UseSwaggerGen();

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
});

app.Run();