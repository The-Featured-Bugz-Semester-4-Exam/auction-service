using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RabbitMQ.Client;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text;
namespace auctionServiceAPI.Controllers;


[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{

    private readonly ILogger<AuctionController> _logger;

    private string mongodbConnection = string.Empty;

    private readonly IMongoDatabase _database;

    private ConnectionFactory factory = new ConnectionFactory();
    private IConnection connection;
    private IModel channel;


    public AuctionController(ILogger<AuctionController> logger,IConfiguration configuration)
    {
       /* //MongodbConnection
        mongodbConnection = configuration["mongodbConnection"] ?? string.Empty;
        //Hej med dig*/
        var mongoSettings = configuration.GetSection("MongoDBSettings");
        //var connectionString = $"mongodb://{mongoSettings["Server"]}:{mongoSettings["Port"]}";
        var connectionString = $"mongodb://localhost:27017/";
        logger.LogInformation(connectionString + "  ");
        var client = new MongoClient(connectionString);
        //_database = client.GetDatabase(mongoSettings["Database"]);
        _database = client.GetDatabase("Auction");
        _logger = logger;
    }
    [HttpPost("CreatAuction")]
    public void PostAuction(Item item){
        
    var collection = _database.GetCollection<Auction>("AuctionCollection");
    var highestAuction = collection.Find(_ => true)
        .SortByDescending(a => a.AuctionID)
        .FirstOrDefault();

    int nextAuctionId = 1;
    if (highestAuction != null)
    {
        nextAuctionId = highestAuction.AuctionID + 1;
    // Opret den nye auktion med det næste auktions-ID
    Auction auction = new Auction(nextAuctionId, item, -1);

    // Indsæt auktionen i MongoDB
    _database.GetCollection<Auction>("AuctionCollection").InsertOne(auction);
    }

    }








   [HttpGet("GetAuctionsActive")]
    public List<Auction> GetAllAuctionActive(){
        var collection = _database.GetCollection<Auction>("AuctionCollection");

    // Udfør forespørgsel for at hente aktive auktioner baseret på dine kriterier
    var activeAuctions = collection.Find(_ => true).ToList();
   
    return activeAuctions;
    }
    [HttpGet("GetAuctionActive/{auctionId}")]
    public Auction GetAuctionActive(int auctionId){
        var collection = _database.GetCollection<Auction>("AuctionCollection");
        
        Auction auction = collection.Find(x => x.AuctionID == auctionId).FirstOrDefault();
        return auction;
    }
    [HttpPost("PostAuctionBid/{auctionBid}")]
    public IActionResult PostAuctionBid(AuctionBid auctionBid){
            try
        {
            _logger.LogInformation("WorkshopRequest oprettet" + StatusCodes.Status200OK,    // Logger en information om, at WorkshopRequest er oprettet, med HTTP status 200 OK og tidspunktet.
            DateTime.UtcNow.ToLongTimeString());

            // Exchange(topic_logs) bestemmer, hvilken type meddelelse der sendes til hvilken kø "Repair" eller "Service".
            channel.ExchangeDeclare(exchange: "topic_logs", ExchangeType.Topic);
            string message = JsonConvert.SerializeObject(auctionBid);    // Konverterer WorkshopRequest til en JSON-streng.
            var body = Encoding.UTF8.GetBytes(message);    // Konverterer JSON-strengen til en byte-array.
        
            // ServiceType datatype String skal være "Repair" eller "Service" !!!
            channel.BasicPublish(exchange: "topic_logs",
                                 // Sender beskeden til køen, der passer til ServiceType, som kan være "Repair" eller "Service".
                                 routingKey: "TestWay",
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"WorkshopRequest added - {message}");    // Logger en information om, at WorkshopRequest er tilføjet med WorkshopRequest JSON-strengen.

            return Ok(message);    // Returnerer HTTP status 200 OK med WorkshopRequest JSON-strengen i responskroppen.
        }
        catch (Exception)    // Håndterer exceptions og logger dem med tidspunktet.
        {
            _logger.LogInformation("Fejl, WorkshopRequest ikke oprettet",
            DateTime.UtcNow.ToLongTimeString());

            return null;
        }
    }
}
