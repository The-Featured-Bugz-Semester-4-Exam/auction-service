using auctionServiceAPI.Models;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

    var logger =
    NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
    logger.Debug("init main");
try
{
 //   var timerTick = new TimerTick();
  //  timerTick.Start();


    var builder = WebApplication.CreateBuilder(args);
    // Konfigurer JWT-validering

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

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();




    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


    var app = builder.Build();


        app.UseSwagger();
        app.UseSwaggerUI();

    

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
    
}
catch (System.Exception ex )
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
    
}
finally 
{
    NLog.LogManager.Shutdown();
}

