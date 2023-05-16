using Microsoft.AspNetCore.Mvc;

namespace auctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{

    private readonly ILogger<AuctionController> _logger;

    public AuctionController(ILogger<AuctionController> logger,IConfiguration configuration)
    {
        //Hej med dig
        _logger = logger;
    }
   
    

}
