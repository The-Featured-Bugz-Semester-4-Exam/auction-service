using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
namespace auctionServiceAPI.Models;
public class Auction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public int AuctionID { get; set; }
    public Item Item { get; set; }
    public int AuctionWinnerID {get; set; } 
    public Auction(int auctionID,Item item, int auctionWinnerID)
    {
        AuctionID = auctionID;
        this.Item = item;
        AuctionWinnerID = auctionWinnerID;
    }
}