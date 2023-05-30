using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using auctionServiceAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using auctionServiceAPI.Services;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;
namespace auctionServiceAPI.Controllers;


[ApiController]
[Route("api")]
public class AuctionController : ControllerBase
{

    private readonly ILogger<AuctionController> _logger;

    private ConnectionFactory factory = new ConnectionFactory();
    private IConnection connection;
    private IModel channel;

    private RedisService redisService;

    public AuctionController(ILogger<AuctionController> logger, IConfiguration configuration)
    {
        _logger = logger;

        _logger.LogInformation("redisService cache connection: " + configuration["redisConnection"]);
       redisService = new RedisService(configuration);


        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"AuctionService responding from {_ipaddr}");

        _logger.LogInformation("Laver connection til rabbitmq med: " + configuration["rabbitmqUrl"] + ":" + configuration["rabbitmqPort"]);
        var factory = new ConnectionFactory { HostName = configuration["rabbitmqUrl"] ?? "localhost", Port = Convert.ToInt16(configuration["rabbitmqPort"]) };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();


    }
    /*
[HttpGet("loadbalancer")]
public IActionResult LoadBalancer()
{
    // Implementer din logik til belastningsfordeling her
    // Dette kan omfatte oprettelse af en load balancer-session, valg af en instans af Auction Service, osv.

    // Returner information om den valgte Auction Service-instans
    var properties = new Dictionary<string, string>();
    properties.Add("service", "Auction Service");

    // Tilføj andre relevante oplysninger om instansen, f.eks. IP-adresse, version osv.
    var hostName = System.Net.Dns.GetHostName();
    var ips = System.Net.Dns.GetHostAddresses(hostName);
    var ipa = ips.First().MapToIPv4().ToString();
    properties.Add("ip-address", ipa);
    
    return Ok(properties);
}
*/


    [HttpPost("postAuctions")]
public async Task<IActionResult> PostAuction([FromBody] ItemToAuction[] itemToAuctions)
{
    string itemsCreated = "The following items is created: ";
    string itemsAlreadyExist = "The following items does already exist: ";

    for (int i = 0; i < itemToAuctions.Length; i++)
    {
        if (redisService.CheckAuctionExist(itemToAuctions[i].ItemID) == false)
        {
            itemsCreated += itemToAuctions[i].ItemID.ToString() + " ";
        }
        if(redisService.CheckAuctionExist(itemToAuctions[i].ItemID) == true)
        {
            itemsAlreadyExist += itemToAuctions[i].ItemID.ToString() + " "; 
        }
        
        redisService.AddAuctionWithCondition(itemToAuctions[i].ItemID, itemToAuctions[i].ItemStartPrice, itemToAuctions[i].ItemEndDate);
    }
    _logger.LogInformation("Auction creation summary: {ItemsCreated} {ItemsAlreadyExist}", itemsCreated, itemsAlreadyExist);

    //_logger.LogInformation($"The following items {} is created and the following items does already exist {itemsAlreadyExist}).");
    return Ok($"{itemsCreated} \n{itemsAlreadyExist}" );
}

    [HttpGet("GetAuctionPrice/{id}")]
        public async Task<IActionResult> GetAuctionPrice(int id)
        {
            var checkAuctionPrice = redisService.GetAuctionPrice(id);
            //Ser på om auction findes i cache
            if (checkAuctionPrice == -1)
            {
                _logger.LogError("Auction not found for AuctionID: {AuctionID}", id);
                return BadRequest("Ugyldigt id");
            }

            _logger.LogInformation("Auction price retrieved for AuctionID: {AuctionID}. Price: {AuctionPrice}", id, checkAuctionPrice);
            return Ok(checkAuctionPrice);
        }

    [Authorize]
    [HttpPost("PostAuctionBid")]
    public async Task<IActionResult> PostAuctionBid([FromBody] Bid bid)
    {
        try
        {  
        var checkAuctionPrice = redisService.GetAuctionPrice(bid.AuctionID);
        // Ser på om auktionen findes i cache
        if (checkAuctionPrice == -1)
        {
            _logger.LogWarning($"User with ID {bid.BidUserID} placed a bid on a non-existing auction (AuctionID: {bid.AuctionID}).");
            return BadRequest("Ugyldigt id");
        }
        
        if (checkAuctionPrice >= bid.BidPrice)
        {
            _logger.LogWarning($"User with ID {bid.BidUserID} placed a bid of {bid.BidPrice}, which is below or equal to the current auction price of {checkAuctionPrice}.");
            return BadRequest("For lavt bud");
        }

            redisService.SetAuctionPrice(bid.AuctionID,bid.BidPrice);

            string message = JsonConvert.SerializeObject(bid);    // Konverterer WorkshopRequest til en JSON-streng.           
            var body = Encoding.UTF8.GetBytes(message);    // Konverterer JSON-strengen til en byte-array.

            // ServiceType datatype String skal være "Repair" eller "Service" !!!
            channel.ExchangeDeclare(exchange: "topic_logs", ExchangeType.Topic);
            channel.BasicPublish(exchange: "topic_logs",
                                 // Sender beskeden til køen, der passer til ServiceType, som kan være "Repair" eller "Service".
                                 routingKey: "auction",
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"{message}");    // Logger en information om, at WorkshopRequest er tilføjet med WorkshopRequest JSON-strengen.


            return Ok($"{message}");    // Returnerer HTTP status 200 OK med WorkshopRequest JSON-strengen i responskroppen.
        }
        catch (Exception ex)    // Håndterer exceptions og logger dem med tidspunktet.
        {
            _logger.LogError(ex, "Fejl under håndtering af AuctionBid");
            return BadRequest("Der opstod en fejl under håndtering af AuctionBid.");
        }
    }
}
