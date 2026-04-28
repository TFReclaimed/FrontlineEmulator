using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontline.Auth;
using Frontline.Battle.GameEvents;
using Frontline.Options;
using Frontline.Xmpp.Transport;
using Microsoft.Extensions.Options;
using XmppDotNet;
using XmppDotNet.Xml;
using XmppDotNet.Xml.Parser;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Bind;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.Sasl;
using XmppDotNet.Xmpp.Session;
using XmppDotNet.Xmpp.Stream;
using XmppDotNet.Xmpp.Vcard;
using XmppStream = XmppDotNet.Xmpp.Server.Stream;

namespace Frontline.Xmpp;

public class XmppClient
{
    private readonly IXmppTransport _transport;

    private readonly System.Timers.Timer _timeoutTimer;

    private readonly CancellationToken _stoppingToken;
    
    private readonly ILogger<XmppClient> _logger;
    
    private readonly ITokenValidator _tokenValidator;
    
    private readonly IOptions<ChatOptions> _chatOptions;

    private readonly StreamParser _streamParser;

    private static readonly JsonSerializerOptions InteractionEventJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    private SessionState _sessionState;

    private int _disposed;

    public int UserId { get; private set; }
    
    public string Username { get; set; } = string.Empty;

    public string Avatar { get; set; } = "avatar001";

    public DateTime? ChatBanEnd { get; set; }

    public Jid? Jid { get; private set; }
    
    public event Action<XmppClient>? OnDisconnected;
    
    public event Action<XmppClient>? OnRequestProfileUpdate;
    
    public event Action<XmppClient, string>? OnEnteredRoom;
    
    public event Action<XmppClient, string>? OnExitedRoom;
    
    public event Action<XmppClient, string, string>? OnMucMessageSent;
    
    public event Action<XmppClient, int, string, string>? OnPrivateMessageSent;

    public XmppClient(IXmppTransport transport, CancellationToken stoppingToken, ILoggerFactory loggerFactory,
        ITokenValidator tokenValidator, IOptions<ChatOptions> chatOptions)
    {
        _transport = transport;
        _timeoutTimer = new System.Timers.Timer(5000);
        _stoppingToken = stoppingToken;
        _logger = loggerFactory.CreateLogger<XmppClient>();
        _tokenValidator = tokenValidator;
        _chatOptions = chatOptions;
        _streamParser = new StreamParser();
        
        _timeoutTimer.Elapsed += (_, _) => Disconnect("Authentication timeout.");
        _timeoutTimer.AutoReset = false;

        _streamParser.OnStreamStart += OnStreamStart;
        _streamParser.OnStreamElement += OnStreamElement;
        _streamParser.OnStreamEnd += OnStreamEnd;
        _streamParser.OnStreamError += OnStreamError;

        _sessionState = SessionState.Connected;
    }

    static XmppClient()
    {
        InteractionEventJsonOptions.AddSerializerContextsFromFrontline();
    }

    public void StartReceiverTask()
    {
        _timeoutTimer.Start();
        _ = Task.Run(ReceiveAsync, _stoppingToken);
    }

    private async Task ReceiveAsync()
    {
        var buffer = new byte[4096];

        try
        {
            while (!_stoppingToken.IsCancellationRequested)
            {
                var bytesRead = await _transport.ReceiveAsync(buffer, _stoppingToken);
                if (bytesRead == 0)
                {
                    Disconnect();
                    break;
                }

                _streamParser.Write(buffer, 0, bytesRead);
            }
        }
        catch (Exception e)
        {
            if (_stoppingToken.IsCancellationRequested)
            {
                return;
            }
            
            Disconnect($"Error receiving XMPP data. {e.Message}");
        }
    }

    public async Task SendAsync(XmppXElement element)
    {
        try
        {
            var xml = element.ToString();
            var buffer = Encoding.UTF8.GetBytes(xml);
            await _transport.SendAsync(buffer, _stoppingToken);

            LogXml(element, false);
        }
        catch (Exception e)
        {
            Disconnect($"Error sending XMPP element. {e.Message}");
        }
    }

    public void Disconnect(string? reason = null)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        if (UserId != 0)
        {
            if (reason != null)
            {
                _logger.LogInformation("{Client} Disconnected: {Reason}", this, reason);
            }
            else
            {
                _logger.LogInformation("{Client} Disconnected.", this);
            }
        }

        _transport.Close();
        _timeoutTimer.Stop();
        _timeoutTimer.Dispose();

        OnDisconnected?.Invoke(this);
    }

    public async Task SendPrivateMessage(Jid from, string subject = "", string body = "", string username = "")
    {
        var message = new Message
        {
            From = from,
            Type = MessageType.Chat,
            Subject = subject,
            Body = body
        };

        if (!string.IsNullOrEmpty(username))
        {
            message.SetAttribute("nck", username);
        }

        await SendAsync(message);
    }

    private void OnStreamStart(XmppXElement element)
    {
        _ = ProcessOnStreamStart(element);
    }
    
    private async Task ProcessOnStreamStart(XmppXElement element)
    {
        element.IsStartTag = true;
        
        LogXml(element, true);

        var headerStream = new XmppStream
        {
            From = element.GetAttributeJid("to"),
            Version = "1.0",
            IsStartTag = true
        };
        
        await SendAsync(headerStream);

        var features = new StreamFeatures();
        
        switch (_sessionState)
        {
            case SessionState.Connected:
                _sessionState = SessionState.Authenticating;
                features.Mechanisms = new Mechanisms();
                features.Mechanisms.AddMechanism(SaslMechanism.Plain);
                break;
            
            case SessionState.Authenticated:
                _sessionState = SessionState.Binding;
                features.Bind = new Bind();
                features.Session = new Session();
                break;
            
            default:
                if (UserId != 0)
                {
                    _logger.LogWarning("{Client} Unknown session state.", this);
                }
                break;
        }

        await SendAsync(features);
    }

    private void OnStreamElement(XmppXElement element)
    {
        _ = ProcessOnStreamElement(element);
    }

    private async Task ProcessOnStreamElement(XmppXElement element)
    {
        LogXml(element, true);

        switch (element)
        {
            case XmppDotNet.Xmpp.Sasl.Auth auth:
                await HandleAuth(auth);
                break;
            
            case Iq iq:
                await HandleIq(iq);
                break;
            
            case Presence presence:
                HandlePresence(presence);
                break;
            
            case Message message:
                await HandleMessage(message);
                break;
            
            default:
                if (UserId != 0)
                {
                    _logger.LogWarning("{Client} Unhandled XMPP element.", this);
                }
                break;
        }
    }

    private void OnStreamEnd()
    {
        _ = ProcessOnStreamEnd();
    }
    
    private async Task ProcessOnStreamEnd()
    {
        var stream = new XmppStream
        {
            IsEndTag = true
        };
        
        await SendAsync(stream);
    }

    private void OnStreamError(Exception exception)
    {
        Disconnect($"Error parsing XML stream. {exception.Message}");
    }

    private async Task HandleAuth(XmppDotNet.Xmpp.Sasl.Auth auth)
    {
        if (_sessionState != SessionState.Authenticating)
        {
            Disconnect();
            return;
        }

        var data = Encoding.UTF8.GetString(auth.Bytes).Split('\x000', StringSplitOptions.RemoveEmptyEntries);
        var token = data[1];
        
        if (!_tokenValidator.IsValidToken(token, out var jwt))
        {
            _logger.LogWarning("{Client} Failed to validate token.", this);
            await SendAsync(new Failure());
            Disconnect();
            return;
        }
        
        UserId = int.Parse(jwt.Claims.First(claim => claim.Type == "UserId").Value);
        
        _timeoutTimer.Stop();
        _streamParser.Reset();

        _sessionState = SessionState.Authenticated;
        await SendAsync(new Success());
    }

    private async Task HandleIq(Iq iq)
    {
        switch (iq.Query)
        {
            case Bind bind:
                await HandleBind(iq, bind);
                break;
            
            case Session:
                await HandleSession(iq);
                break;
            
            case Vcard:
                HandleVcard();
                break;
            
            default:
                _logger.LogWarning("{Client} Unhandled IQ query.", this);
                break;
        }
    }

    private async Task HandleBind(Iq iq, Bind bind)
    {
        Jid = new Jid(UserId.ToString(), Globals.XmppServerAddress, bind.Resource);

        var response = new BindIq
        {
            Id = iq.Id,
            Type = IqType.Result,
            Bind = new Bind
            {
                Jid = Jid
            }
        };
        
        _sessionState = SessionState.Binded;
        await SendAsync(response);
    }

    private async Task HandleSession(Iq iq)
    {
        iq.Type = IqType.Result;

        await SendAsync(iq);
    }

    private void HandleVcard()
    {
        OnRequestProfileUpdate?.Invoke(this);
    }
    
    private void HandlePresence(Presence presence)
    {
        if (UserId == 0)
        {
            Disconnect();
            return;
        }

        var to = presence.To;
        if (to is null)
        {
            return;
        }
        
        var room = to.Local;
        if (string.IsNullOrEmpty(room))
        {
            return;
        }
        
        if (room != "worldwide" && !room.StartsWith("guild") && !Guid.TryParse(room.AsSpan(5), out _))
        {
            _logger.LogWarning("{Client} Invalid room: {Room}", this, room);
            return;
        }
        
        if (presence.Type == PresenceType.Unavailable)
        {
            _logger.LogInformation("{Client} Exited room: {Room}", this, room);
            OnExitedRoom?.Invoke(this, room);
            return;
        }

        _logger.LogInformation("{Client} Entering room: {Room}", this, room);
        OnEnteredRoom?.Invoke(this, room);
    }

    private async Task HandleMessage(Message message)
    {
        if (UserId == 0)
        {
            Disconnect();
            return;
        }

        if (string.IsNullOrEmpty(message.Body) && string.IsNullOrEmpty(message.Subject))
        {
            return;
        }

        // Profile loading is not requested if the client loads directly into a game
        if (string.IsNullOrEmpty(Username) && message is not { Type: MessageType.Chat, Subject: ":::INTERACTIONEVENT" })
        {
            _logger.LogWarning("{Client} Attempted to send message before profile was loaded.", this);
            return;
        }

        var body = message.Body.Trim();

        if (message.Type == MessageType.Chat)
        {
            var to = int.Parse(message.To.Local);
            var subject = message.Subject;

            if (subject == ":::CHALLENGED:::" && to == UserId)
            {
                _logger.LogWarning("{Client} Attempted to challenge self.", this);
                await SendPrivateMessage(Jid!, $":::CHALLENGE_REJECTED:::{to}");
                return;
            }

            if (subject == ":::CHALLENGED:::" && to == -1)
            {
                _logger.LogWarning("{Client} Attempted to challenge system.", this);
                var systemJid = new Jid("-1", Globals.XmppServerAddress, "-1");
                await SendPrivateMessage(systemJid, $":::CHALLENGE_REJECTED:::{to}");
                return;
            }

            // We reserialize the body as fastJSON is vulnerable to RCE
            if (subject == ":::INTERACTIONEVENT")
            {
                var interactionEvent = JsonSerializer.Deserialize<GameEventInteractionsParams>(body, InteractionEventJsonOptions);
                if (interactionEvent is null)
                {
                    _logger.LogWarning("{Client} Failed to deserialize interaction event.", this);
                    return;
                }

                body = JsonSerializer.Serialize(interactionEvent, InteractionEventJsonOptions);
            }

            if (subject != ":::INTERACTIONEVENT")
            {
                _logger.LogInformation("{Client} [PM #{To}] [{Subject}]: {Body}", this, to, subject, body);
            }

            OnPrivateMessageSent?.Invoke(this, to, subject, body);
        }
        else if (message.Type == MessageType.GroupChat)
        {
            if (body.Length > Globals.MaxMessageLength)
            {
                body = body[..Globals.MaxMessageLength];
            }

            _logger.LogInformation("{Client} [MUC #{To}]: {Message}", this, message.To.Local, body);
            OnMucMessageSent?.Invoke(this, message.To.Local, body);
        }
    }
    
    private void LogXml(XmppXElement element, bool incoming)
    {
        if (!_chatOptions.Value.EnableXmlLogging)
        {
            return;
        }
        
        var direction = incoming ? "IN" : "OUT";
        _logger.LogInformation("{Client} [{Direction}]: {Element}", this, direction, element);
    }

    public override string ToString()
    {
        var transportName = _transport switch
        {
            TcpXmppTransport => "TCP",
            WebSocketXmppTransport => "WS",
            _ => throw new ArgumentOutOfRangeException()
        };
        var user = UserId == 0 ? "Not authenticated" : UserId.ToString();
        return $"[{transportName} {_transport.GetRemoteEndpoint()}/{user}]";
    }
}