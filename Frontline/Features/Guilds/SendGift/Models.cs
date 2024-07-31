namespace Frontline.Features.Guilds.SendGift;

public class SendGiftRequest
{
    public int ReceiverId { get; set; }
}

public class SendGiftResponse
{
    public bool Fulfillment { get; set; }
}