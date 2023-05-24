using StackExchange.Redis;
using auctionServiceAPI.Models;
using auctionServiceAPI.Controllers;
namespace auctionServiceAPI.Services;

public class RedisService
{
    private string connectionString = string.Empty;
    private ConnectionMultiplexer redisConnect;

    IDatabase redisDb;
    public RedisService(IConfiguration configuration)
    {
        string connection = configuration["redisConnection"] ?? string.Empty;

        redisConnect = ConnectionMultiplexer.Connect(connection);

        if (redisConnect.IsConnected)
        {
            redisDb = redisConnect.GetDatabase();
            // Redis-forbindelsen er etableret
        }
        else
        {
            throw new InvalidOperationException("Kunne ikke oprette forbindelse til Redis.");
        }
    }
    public int GetAuctionPrice(int id)
    {

        int bidPrice = (int)redisDb.StringGet(id.ToString());

        return bidPrice > 0 ? bidPrice : -1;
    }
    public bool CheckAuctionExist(int id)
    {
        return (int?)redisDb.StringGet(id.ToString()) != null;
    }
    public bool SetAuctionPrice(int id, int bidPrice)
    {

        var checkAuctionExist = CheckAuctionExist(id);
        if (checkAuctionExist)
        {
            redisDb.StringSet(id.ToString(), bidPrice);
        }
        return checkAuctionExist;
    }

    public bool AddAuctionWithCondition(int id, int bidPrice, DateTime expireDate)
    {
        bool checkAuctionExist = CheckAuctionExist(id);
        if (checkAuctionExist == false)
        {
            var expiryTimeSpan = expireDate.Subtract(DateTime.UtcNow);
            redisDb.StringSet(id.ToString(), bidPrice, expiryTimeSpan);
        }
        //Hvis auction allerede findes returnere den falsk, fordi den må ikke overskrive en auction.
        return !checkAuctionExist;
    }
}