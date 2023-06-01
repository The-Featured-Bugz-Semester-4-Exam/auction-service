
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using auctionServiceAPI.Controllers;
using auctionServiceAPI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Linq;
using auctionServiceAPI.Models;
namespace auctionServiceAPI.Tests
{
    public class AuctionControllerTests
    {
        private readonly AuctionController _auctionController;
        private readonly Mock<ILogger<AuctionController>> _loggerMock;
        private readonly Mock<RedisService> _redisServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public AuctionControllerTests()
        {
            _loggerMock = new Mock<ILogger<AuctionController>>();
            _redisServiceMock = new Mock<RedisService>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.SetupGet(x => x["server"]).Returns("localhost");
            _configurationMock.SetupGet(x => x["port"]).Returns("27017");
            _configurationMock.SetupGet(x => x["auctionActiveCol"]).Returns("auctionActiveCol");
            _configurationMock.SetupGet(x => x["auctionDoneCol"]).Returns("auctionDoneCol");
            _configurationMock.SetupGet(x => x["database"]).Returns("Auction");

            _auctionController = new AuctionController(_loggerMock.Object, _configurationMock.Object);
        }

        [Test]
        public async Task PostAuction_ReturnsOkResult()
        {
            // Arrange
            var itemToAuctions = new ItemToAuction[]
            {
                new ItemToAuction { ItemID = 1, ItemStartPrice = 100, ItemEndDate = DateTime.Now.AddDays(7) },
                new ItemToAuction { ItemID = 3, ItemStartPrice = 200, ItemEndDate = DateTime.Now.AddDays(14) }
            };

            //Test if auction is found. Should fail 
            var resultGetAuctionPriceFail = await _auctionController.GetAuctionPrice(100);
            Assert.IsInstanceOf<BadRequestObjectResult>(resultGetAuctionPriceFail);


            //Test Post item should succes
            var resultPostAuction = await _auctionController.PostAuction(itemToAuctions);
            Assert.AreEqual(200, (resultPostAuction as OkObjectResult).StatusCode);

            //Test The item should now be there.
            var resultGetAuctionPriceSucces = await _auctionController.GetAuctionPrice(3);
            Assert.AreEqual(200, (resultGetAuctionPriceSucces as OkObjectResult).StatusCode);
        }
    }
}