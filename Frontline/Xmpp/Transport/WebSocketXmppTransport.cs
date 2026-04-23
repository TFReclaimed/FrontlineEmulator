using System.Net.WebSockets;

namespace Frontline.Xmpp.Transport;

public sealed class WebSocketXmppTransport : IXmppTransport
{
    private readonly WebSocket _webSocket;

    private readonly string _remoteEndPoint;

    public WebSocketXmppTransport(WebSocket webSocket, string remoteEndPoint)
    {
        _webSocket = webSocket;
        _remoteEndPoint = remoteEndPoint;
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }

    public string GetRemoteEndpoint()
    {
        return _remoteEndPoint;
    }

    public async Task SendAsync(byte[] buffer, CancellationToken ct)
    {
        await _webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, ct);
    }

    public async Task<int> ReceiveAsync(byte[] buffer, CancellationToken ct)
    {
        var result = await _webSocket.ReceiveAsync(buffer, ct);
        return result.Count;
    }

    public void Close()
    {
        _webSocket.Abort();
    }
}