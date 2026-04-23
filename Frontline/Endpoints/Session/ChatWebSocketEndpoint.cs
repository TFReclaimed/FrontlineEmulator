using System.Net.WebSockets;
using FastEndpoints;
using Frontline.Xmpp;
using Frontline.Xmpp.Transport;

namespace Frontline.Endpoints.Session;

public class ChatWebSocketEndpoint : EndpointWithoutRequest
{
    private readonly IXmppServer _xmppServer;

    public ChatWebSocketEndpoint(IXmppServer xmppServer)
    {
        _xmppServer = xmppServer;
    }

    public override void Configure()
    {
        Get("/chat");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("xmpp");
        var transport = new WebSocketXmppTransport(webSocket, HttpContext.Connection.RemoteIpAddress!.ToString());

        _xmppServer.InitializeClient(transport, ct);

        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            await Task.Delay(100, ct);
        }
    }
}