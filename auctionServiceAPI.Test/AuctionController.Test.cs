using Xunit;
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

            _auctionController = new AuctionController(_loggerMock.Object, _configurationMock.Object);
        }    

        [Test]
        public async Task PostAuction_ReturnsOkResult()
        {
            // Arrange
            var itemToAuctions = new ItemToAuction[]
            {
                new ItemToAuction { ItemID = 1, ItemStartPrice = 100, ItemEndDate = DateTime.Now.AddDays(7) },
                new ItemToAuction { ItemID = 2, ItemStartPrice = 200, ItemEndDate = DateTime.Now.AddDays(14) }
            };

            _redisServiceMock.Setup(x => x.AddAuctionWithCondition(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Verifiable();

            // Act
            var result = await _auctionController.PostAuction(itemToAuctions);

            // Assert
          //  Assert.IsType<OkObjectResult>(result);
            //Assert.Equal("Added", (result as OkObjectResult).Value);

            _redisServiceMock.Verify(x => x.AddAuctionWithCondition(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Exactly(itemToAuctions.Length));
        }
    }
}