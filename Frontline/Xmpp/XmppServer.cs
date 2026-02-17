using System.Net;
using System.Net.Sockets;
using Frontline.Auth;
using Frontline.Data.Repositories;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Xmpp;

public class XmppServer : BackgroundService
{
    private readonly ILogger<XmppServer> _logger;
    
    private readonly IOptions<ChatOptions> _chatOptions;
    
    private readonly ILoggerFactory _loggerFactory;

    private readonly ITokenValidator _tokenValidator;
    
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly TcpListener _tcpListener;

    private readonly List<XmppClient> _xmppClients;
    
    private readonly Dictionary<string, ChatRoom> _chatRooms;

    public XmppServer(ILogger<XmppServer> logger, IOptions<ChatOptions> chatOptions, ILoggerFactory loggerFactory,
        ITokenValidator tokenValidator, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _chatOptions = chatOptions;
        _loggerFactory = loggerFactory;
        _tokenValidator = tokenValidator;
        _serviceScopeFactory = serviceScopeFactory;
        _tcpListener = new TcpListener(IPAddress.Any, _chatOptions.Value.Port);
        _xmppClients = [];
        _chatRooms = new Dictionary<string, ChatRoom>();
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _tcpListener.Start();
        
        _logger.LogInformation("XMPP server started on port {Port}.", _chatOptions.Value.Port);
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken);
                _logger.LogDebug("Accepted XMPP client {Client}.", tcpClient.Client.RemoteEndPoint);
                
                var xmppClient = new XmppClient(tcpClient, stoppingToken, _loggerFactory, _tokenValidator, _chatOptions);
                xmppClient.OnDisconnected += OnClientDisconnected;
                xmppClient.OnRequestProfileUpdate += OnClientRequestProfileUpdate;
                xmppClient.OnEnteredRoom += OnClientEnteredRoom;
                xmppClient.OnExitedRoom += OnClientExitedRoom;
                xmppClient.OnMucMessageSent += OnClientMucMessageSent;
                xmppClient.OnPrivateMessageSent += OnClientPrivateMessageSent;
                xmppClient.StartReceiverTask();
                
                _xmppClients.Add(xmppClient);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                
                _logger.LogError(e, "Error accepting TCP client.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _tcpListener.Stop();
    }

    private void OnClientDisconnected(XmppClient client)
    {
        _ = ProcessOnClientDisconnected(client);
    }

    private async Task ProcessOnClientDisconnected(XmppClient client)
    {
        if (!_xmppClients.Remove(client))
        {
            return;
        }
        
        foreach (var chatRoom in _chatRooms.Values)
        {
            await chatRoom.RemoveClient(client);
        }
    }
    
    private void OnClientRequestProfileUpdate(XmppClient client)
    {
        _ = ProcessOnClientRequestProfileUpdate(client);
    }
    
    private async Task ProcessOnClientRequestProfileUpdate(XmppClient client)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        
        var player = await playerRepository.GetByIdAsync(client.UserId);
        if (player is null)
        {
            _logger.LogWarning("{Client} wants to update their profile, but the player does not exist.", client);
            return;
        }
        
        client.Username = player.Name;
        client.Avatar = player.AvatarId;
    }

    private void OnClientEnteredRoom(XmppClient client, string room)
    {
        _ = ProcessOnClientEnteredRoom(client, room);
    }

    private async Task ProcessOnClientEnteredRoom(XmppClient client, string room)
    {
        if (room.StartsWith("guild"))
        {
            var guildId = Guid.Parse(room.AsSpan(5));
            
            using var scope = _serviceScopeFactory.CreateScope();
            var guildRepository = scope.ServiceProvider.GetRequiredService<IGuildRepository>();
            
            var guild = await guildRepository.GetPlayerGuildAsync(client.UserId);
            if (guild is null || guild.Id != guildId)
            {
                _logger.LogWarning("{Client} tried to enter guild room {Room}, but they are not in the guild.",
                    client, room);
                return;
            }
        }
        
        if (!_chatRooms.TryGetValue(room, out var chatRoom))
        {
            chatRoom = new ChatRoom(_chatOptions, room);
            _chatRooms.Add(room, chatRoom);
        }
        
        await chatRoom.AddClient(client);
    }
    
    private void OnClientExitedRoom(XmppClient client, string room)
    {
        _ = ProcessOnClientExitedRoom(client, room);
    }
    
    private async Task ProcessOnClientExitedRoom(XmppClient client, string room)
    {
        if (_chatRooms.TryGetValue(room, out var chatRoom))
        {
            await chatRoom.RemoveClient(client);
        }
    }
    
    private void OnClientMucMessageSent(XmppClient client, string room, string message)
    {
        _ = ProcessOnClientMucMessageSent(client, room, message);
    }
    
    private async Task ProcessOnClientMucMessageSent(XmppClient client, string room, string message)
    {
        if (_chatRooms.TryGetValue(room, out var chatRoom))
        {
            await chatRoom.BroadcastMessage(client, message);
        }
    }
    
    private void OnClientPrivateMessageSent(XmppClient client, int id, string subject, string body)
    {
        var recipient = _xmppClients.FirstOrDefault(x => x.UserId == id);
        recipient?.SendPrivateMessage(client.Jid!, subject, body);
    }
}