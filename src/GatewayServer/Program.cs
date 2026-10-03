using GatewayBL.InputPorts;
using GatewayBL.Managers;
using GatewayBL.OutputPorts;
using GatewayHttp.Clients;
using GatewayServer.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = $"{builder.Configuration["Auth0:Audience"]}name"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient<ILibraryHttpClient, LibraryHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetSection("ServiceUrls")["LibraryServiceUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IRatingHttpClient, RatingHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetSection("ServiceUrls")["RatingServiceUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IReservationHttpClient, ReservationHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetSection("ServiceUrls")["ReservationServiceUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped<IGatewayManager, GatewayManager>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("manage/health", () => Results.Ok("Healthy"));

await app.RunAsync();
