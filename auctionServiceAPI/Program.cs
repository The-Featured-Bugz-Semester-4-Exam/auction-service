using auctionServiceAPI.Models;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Setting up NLog logger
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    // Creating a new WebApplication instance
    var builder = WebApplication.CreateBuilder(args);

    // Configuring JWT authentication
    string myValidAudience = Environment.GetEnvironmentVariable("ValidAudience") ?? "http://localhost";
    string mySecret = "mySecretIsASecret";
    string myIssuer = "myIssuerIsAnIssue";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = myIssuer,
                ValidAudience = myValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret))
            };
        });

    // Clearing existing logging providers and configuring NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    // Enabling authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (System.Exception ex)
{
    // Logging the exception and re-throwing it
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    // Shutting down the NLog logger
    NLog.LogManager.Shutdown();
}
