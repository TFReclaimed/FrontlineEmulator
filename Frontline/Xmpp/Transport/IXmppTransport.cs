namespace Frontline.Xmpp.Transport;

public interface IXmppTransport : IDisposable
{
    string GetRemoteEndpoint();
    Task SendAsync(byte[] buffer, CancellationToken ct);
    Task<int> ReceiveAsync(byte[] buffer, CancellationToken ct);
    void Close();
}