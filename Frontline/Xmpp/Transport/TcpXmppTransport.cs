using System.Net.Sockets;

namespace Frontline.Xmpp.Transport;

public sealed class TcpXmppTransport : IXmppTransport
{
    private readonly TcpClient _tcpClient;

    private readonly NetworkStream _stream;

    public TcpXmppTransport(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _stream = _tcpClient.GetStream();
    }

    public void Dispose()
    {
        _stream.Dispose();
        _tcpClient.Dispose();
    }

    public string GetRemoteEndpoint()
    {
        return _tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
    }

    public async Task SendAsync(byte[] buffer, CancellationToken ct)
    {
        await _stream.WriteAsync(buffer, ct);
    }

    public async Task<int> ReceiveAsync(byte[] buffer, CancellationToken ct)
    {
        return await _stream.ReadAsync(buffer, ct);
    }

    public void Close()
    {
        _tcpClient.Close();
    }
}