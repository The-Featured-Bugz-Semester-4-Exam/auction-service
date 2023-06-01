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
[Route("[controller]")]
public class AuctionController : ControllerBase
{
    private readonly ILogger<AuctionController> _logger;

    // RabbitMQ connection variables
    private ConnectionFactory factory = new ConnectionFactory();
    private IConnection connection;
    private IModel channel;

    private RedisService redisService;

    public AuctionController(ILogger<AuctionController> logger, IConfiguration configuration)
    {
        _logger = logger;

        // Initializing RedisService with configuration for Redis cache
        _logger.LogInformation("redisService cache connection: " + configuration["redisConnection"]);
        redisService = new RedisService(configuration);

        // Logging the IP address of the server
        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"auction-service responding from {_ipaddr}");
        try
        {
            // Connecting to RabbitMQ
            _logger.LogInformation("INFO: Connect to rabbitMQ configuration: " + configuration["rabbitmqUrl"] + ":" + configuration["rabbitmqPort"]);
            factory = new ConnectionFactory()
            {
                HostName = configuration["rabbitmqUrl"] ?? "localhost",
                Port = Convert.ToInt16(configuration["rabbitmqPort"] ?? "5672"),
                UserName = configuration["rabbitmqUsername"] ?? "guest",
                Password = configuration["rabbitmqUserpassword"] ?? "guest"

            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: "auction",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "FAILED: Connect to rabbitMQ configuration: " + configuration["rabbitmqUrl"] + ":" + configuration["rabbitmqPort"]);
            throw;
        }
    }

    // Endpoint to get the version information of the API
    [HttpGet("version")]
    public IEnumerable<string> GetVersion()
    {
        var properties = new List<string>();
        var assembly = typeof(Program).Assembly;
        foreach (var attribute in assembly.GetCustomAttributesData())
        {
            properties.Add($"{attribute.AttributeType.Name} - {attribute.ToString()} \n");
        }
        return properties;
    }

    // Endpoint to post auctions
    [HttpPost("postAuctions")]
    public async Task<IActionResult> PostAuction([FromBody] ItemToAuction[] itemToAuctions)
    {
        string itemsCreated = "The following items are created: ";
        string itemsAlreadyExist = "The following items already exist: ";

        for (int i = 0; i < itemToAuctions.Length; i++)
        {
            // Checking if the auction already exists in Redis cache
            if (redisService.CheckAuctionExist(itemToAuctions[i].ItemID) == false)
            {
                itemsCreated += itemToAuctions[i].ItemID.ToString() + " ";
            }
            if (redisService.CheckAuctionExist(itemToAuctions[i].ItemID) == true)
            {
                itemsAlreadyExist += itemToAuctions[i].ItemID.ToString() + " ";
            }

            // Adding the auction to Redis cache
            redisService.AddAuctionWithCondition(itemToAuctions[i].ItemID, itemToAuctions[i].ItemStartPrice, itemToAuctions[i].ItemEndDate);
        }
        _logger.LogInformation("Auction creation summary: {ItemsCreated} {ItemsAlreadyExist}", itemsCreated, itemsAlreadyExist);

        return Ok($"{itemsCreated} \n{itemsAlreadyExist}");
    }

    // Endpoint to get the auction price by ID
    [HttpGet("getAuctionPrice/{id}")]
    public async Task<IActionResult> GetAuctionPrice(int id)
    {
        // Retrieving the auction price from Redis cache
        var checkAuctionPrice = redisService.GetAuctionPrice(id);

        if (checkAuctionPrice == -1)
        {
            _logger.LogError("Auction not found for AuctionID: {AuctionID}", id);
            return BadRequest("Id does not exist");
        }

        _logger.LogInformation("Auction price retrieved for AuctionID: {AuctionID}. Price: {AuctionPrice}", id, checkAuctionPrice);
        return Ok(checkAuctionPrice);
    }

    // Endpoint to post an auction bid
    [Authorize]
    [HttpPost("postAuctionBid")]
    public async Task<IActionResult> PostAuctionBid([FromBody] Bid bid)
    {
        try
        {
            var checkAuctionPrice = redisService.GetAuctionPrice(bid.AuctionID);

            if (checkAuctionPrice == -1)
            {
                // Logging a warning if the auction does not exist
                string feedback = $"User with ID {bid.BidUserID} placed a bid on a non-existing auction (AuctionID: {bid.AuctionID}).";
                _logger.LogWarning(feedback);
                return BadRequest(feedback);
            }

            if (checkAuctionPrice >= bid.BidPrice)
            {
                // Logging a warning if the bid price is not higher than the current auction price
                string feedback = $"User with ID {bid.BidUserID} placed a bid of {bid.BidPrice}, which is below or equal to the current auction price of {checkAuctionPrice}.";
                _logger.LogWarning(feedback);
                return BadRequest(feedback);
            }

            // Updating the auction price in Redis cache
            redisService.SetAuctionPrice(bid.AuctionID, bid.BidPrice);

            string message = JsonConvert.SerializeObject(bid);
            var body = Encoding.UTF8.GetBytes(message);

            // Publishing the bid to RabbitMQ for further processing
            channel.BasicPublish(exchange: string.Empty,
                        routingKey: "auction",
                        basicProperties: null,
                        body: body);

            _logger.LogInformation($"{message}");
            return Ok($"{message}");
        }
        catch (Exception ex)
        {
            string feedback = "An error occurred while handling the Auction Bid.";
            _logger.LogError(ex, feedback);
            return BadRequest(feedback);
        }
    }
}

