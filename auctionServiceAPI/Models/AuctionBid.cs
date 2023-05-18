namespace auctionServiceAPI.Models;

public class AuctionBid
{
    public int AuctionBidID {get; set;}
    public string AuctionBidUserID { get; set; } = string.Empty;

    public int Bid { get; set; }    
    public AuctionBid(int auctionBidID,string auctionBidUserID,int bid)
    {
        this.AuctionBidID = auctionBidID;
        this.AuctionBidUserID = auctionBidUserID;
        this.Bid = bid;
    }
}