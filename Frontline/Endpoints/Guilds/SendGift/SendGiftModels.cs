namespace Frontline.Endpoints.Guilds.SendGift;

public class SendGiftRequest
{
    public int ReceiverId { get; set; }
}

public class SendGiftResponse
{
    public bool Fulfillment { get; set; }
}