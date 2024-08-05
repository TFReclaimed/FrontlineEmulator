using Frontline.Options;
using Microsoft.Extensions.Options;
using XmppDotNet;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.Delay;
using XmppDotNet.Xmpp.Muc;
using Item = XmppDotNet.Xmpp.Muc.User.Item;
using X = XmppDotNet.Xmpp.Muc.User.X;

namespace Frontline.Xmpp;

public class ChatRoom
{
    private readonly IOptions<ChatOptions> _chatOptions;
    
    private readonly string _name;
    
    private readonly List<XmppClient> _clients;
    
    private readonly List<Message> _messages;
    
    public ChatRoom(IOptions<ChatOptions> chatOptions, string name)
    {
        _chatOptions = chatOptions;
        _name = name;
        _clients = new List<XmppClient>();
        _messages = new List<Message>();
    }
    
    public async Task AddClient(XmppClient client)
    {
        if (_clients.Contains(client))
        {
            return;
        }
        
        foreach (var xmppClient in _clients)
        {
            await client.SendAsync(MakePresence(xmppClient.Jid!));
        }

        _clients.Add(client);
        
        await Broadcast(MakePresence(client.Jid!));
        
        foreach (var message in _messages)
        {
            await client.SendAsync(message);
        }

        await SendWelcomeMessage(client);
    }
    
    public async Task RemoveClient(XmppClient client)
    {
        _clients.Remove(client);
        
        var presenceElement = new Presence
        {
            From = new Jid(_name, Globals.XmppMucAddress, client.Jid!.Local),
            Type = PresenceType.Unavailable
        };
        
        await Broadcast(presenceElement);
    }
    
    public async Task BroadcastMessage(XmppClient sender, string message)
    {
        var messageElement = new Message
        {
            From = new Jid(_name, Globals.XmppMucAddress, sender.Jid!.Local),
            Body = message,
            Type = MessageType.GroupChat,
            Delay = new Delay(DateTime.UtcNow)
        };

        messageElement.SetAttribute("nck", sender.Username);
        messageElement.SetAttribute("img", sender.Avatar);
        
        _messages.Add(messageElement);
        if (_messages.Count > 40)
        {
            _messages.RemoveAt(0);
        }
        
        await Broadcast(messageElement);
    }

    public async Task Broadcast(XmppXElement element)
    {
        foreach (var client in _clients)
        {
            await client.SendAsync(element);
        }
    }

    private Presence MakePresence(Jid jid)
    {
        return new Presence
        {
            From = new Jid(_name, Globals.XmppMucAddress, jid.Local),
            MucUser = new X
            {
                Item = new Item
                {
                    Affiliation = Affiliation.Member,
                    Role = Role.Participant
                }
            }
        };
    }

    private async Task SendWelcomeMessage(XmppClient client)
    {
        var systemJid = new Jid(_name, Globals.XmppMucAddress, "-1");
        
        await Broadcast(MakePresence(systemJid));
        
        if (string.IsNullOrWhiteSpace(_chatOptions.Value.WelcomeMessage))
        {
            return;
        }
        
        var welcomeMessage = new Message
        {
            From = systemJid,
            Body = _chatOptions.Value.WelcomeMessage,
            Type = MessageType.GroupChat
        };
        
        welcomeMessage.SetAttribute("nck", "<color=red>SYSTEM</color>");
        welcomeMessage.SetAttribute("img", "avatar006");
        
        await client.SendAsync(welcomeMessage);
    }
}