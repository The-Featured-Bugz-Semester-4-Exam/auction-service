using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
namespace auctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{

    private readonly ILogger<AuctionController> _logger;

    private string mongodbConnection = string.Empty;

    private readonly IMongoDatabase _database;

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
    [HttpPost]
    public void PostAuction(){
        
        Item itemm = new Item
        {
            ItemID = 1,
            ItemName = "Item 1",
            ItemDescription = "Beskrivelse af Item 1",
            ItemStartprice = 100,
            ItemSellerID = 123,
            ItemStartDate = DateTime.Parse("2022-01-01T00:00:00Z"),
            ItemEndDate = DateTime.Parse("2022-01-10T00:00:00Z")
        };
        Auction auction = new Auction(1,itemm,-1);


        _database.GetCollection<Auction>("AuctionCollection").InsertOne(auction);
    }

  /* [HttpGet]
    public List<Auction> GetAuctionsActive(){
        var collection = _database.GetCollection<Auction>("YourAuctionCollection");

    // Udfør forespørgsel for at hente aktive auktioner baseret på dine kriterier
    var activeAuctions = collection.Find(a => a.).ToList();
   
    return activeAuctions;
    }*/
    [HttpGet]
    public Auction GetAuction(){
        var collection = _database.GetCollection<Auction>("AuctionCollection");


        var filter = Builders<Auction>.Filter.Eq(a => a.AuctionID,1);
        var projection = Builders<Auction>.Projection
        .Include(a => a.AuctionID)
        .Include(a => a.Item)
        .Include(a => a.AuctionWinnerID);

        Auction auction = collection.Find(filter).Project<Auction>(projection).FirstOrDefault();

   
        return auction;
    }
 

}
