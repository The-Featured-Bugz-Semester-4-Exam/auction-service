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
        
        redisService = new RedisService(configuration);

        _logger = logger;

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"Taxabooking responding from {_ipaddr}");

        var factory = new ConnectionFactory { HostName = configuration["workerUrl"] ?? "localhost" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();


    }
    //[Authorize]
    [HttpPost("PostAuctions")]
    public async Task<IActionResult> PostAuction([FromBody] ItemToAuction[]  itemToAuctions){
        _logger.LogInformation("PostAuction");

        for (int i = 0; i < itemToAuctions.Length; i++)
        {
            //Auction
            redisService.AddAuctionWithCondition(itemToAuctions[i].ItemID,itemToAuctions[i].ItemStartPrice,itemToAuctions[i].ItemEndDate);
        }
        return Ok("Added");
    }


    [HttpGet("GetAuctionPrice/{id}")]
    public async Task<IActionResult> GetAuctionPrice(int id){

        var checkAuctionPrice = redisService.GetAuctionPrice(id);

        if (checkAuctionPrice == -1)
        {
            return BadRequest("Ugyldigt id");

            
        }
        return Ok(checkAuctionPrice);
    }


    //[Authorize]
    [HttpPost("PostAuctionBid")]
    public async Task<IActionResult> PostAuctionBid([FromBody] Bid bid)
    {
        try
        {
            //Kan diskuteres om den skal hente en brugerID eller bare lade en selv bestemme brugerID når man sender.
            //Kan nok lave noget i postman at når noget bliver udført bliver en variable sat i postman.





        
            //1. Kalder på redis cache
            //2. Checker om den er null
            //3. Checker på pris og id/key
            //4. Return Badrequest eller bool om posten går igennem eller ej
            //5. Sender til rabbitMQ
            //6. Worker modtager det og ligger i en database.
           



            //Hente en Item/Items
            //1. User kalder på catalog
            //2. Catalog kalder på worker og får de aktive auktioners item
            //4. Catalog giver de auktioner til user
            //5. User viser auktionerne på hjemmesiden.
            var checkAuctionPrice = redisService.GetAuctionPrice(bid.AuctionID);
            //Ser på om auction findes i cache
            if (checkAuctionPrice == -1)
            {
                return BadRequest("Ugyldigt id");
            }
            if (checkAuctionPrice >= bid.BidPrice)
            {
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
            _logger.LogInformation($"Bid udført: {message}");    // Logger en information om, at WorkshopRequest er tilføjet med WorkshopRequest JSON-strengen.



            return Ok($"Fisk");    // Returnerer HTTP status 200 OK med WorkshopRequest JSON-strengen i responskroppen.
        }
        catch (Exception ex)    // Håndterer exceptions og logger dem med tidspunktet.
        {
            _logger.LogInformation(ex.Message);         
            return BadRequest(ex.Message);
        }
    }
}
