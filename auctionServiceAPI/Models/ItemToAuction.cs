namespace auctionServiceAPI.Models;

public class ItemToAuction
{
    public int ItemID { get; set; }
    public int ItemStartPrice { get; set; }
    public DateTime ItemEndDate { get; set; }
}