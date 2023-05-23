using MongoDB.Bson.Serialization;
namespace auctionServiceAPI.Models;
public class Item

{
    public int ItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public int ItemStartprice { get; set; }
    public int ItemSellerID { get; set; }
    public DateTime ItemStartDate { get; set; }
    public DateTime ItemEndDate { get; set; }
}