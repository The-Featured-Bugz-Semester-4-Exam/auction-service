using Microsoft.AspNetCore.Mvc;

namespace auctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{

    private readonly ILogger<AuctionController> _logger;

    private string mongodbConnection = string.Empty;

    public AuctionController(ILogger<AuctionController> logger,IConfiguration configuration)
    {
        mongodbConnection = configuration["mogodbConnection"] ?? string.Empty;
        //Hej med dig
        _logger = logger;
    }
    public void PostAuction(Item item){
        

      
    }

}
