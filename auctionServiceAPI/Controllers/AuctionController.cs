using Microsoft.AspNetCore.Mvc;

namespace auctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{

    private readonly ILogger<AuctionController> _logger;

    public AuctionController(ILogger<AuctionController> logger)
    {
        _logger = logger;
    }

}
