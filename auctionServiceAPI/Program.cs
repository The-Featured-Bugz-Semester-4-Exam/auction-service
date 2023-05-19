using auctionServiceAPI.Models;
using NLog;
using NLog.Web;


    var logger =
    NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
    logger.Debug("init main");
try
{
 //   var timerTick = new TimerTick();
  //  timerTick.Start();


    var builder = WebApplication.CreateBuilder(args);
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

