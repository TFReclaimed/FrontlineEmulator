using Frontline.Data.Entities;
using Frontline.Data.Repositories;
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

    private readonly Jid _systemJid;

    private readonly List<XmppClient> _clients;

    private readonly Dictionary<XmppClient, DateTime> _lastMessageTimes;

    private readonly List<Message> _messages;

    private readonly Lock _lock = new();

    private ChatRoom(IOptions<ChatOptions> chatOptions, string name, List<Message> history)
    {
        _chatOptions = chatOptions;
        _name = name;
        _systemJid = new Jid(_name, Globals.XmppMucAddress, "-1");
        _clients = [];
        _lastMessageTimes = [];
        _messages = history;
    }

    public static async Task<ChatRoom> CreateAsync(IOptions<ChatOptions> chatOptions, string name,
        IChatMessageRepository chatMessageRepository)
    {
        var messageEntities = await chatMessageRepository.GetRecentAsync(name, Globals.MaxMessages);
        var history = messageEntities.Select(entity => BuildMessageElement(entity, name)).ToList();

        return new ChatRoom(chatOptions, name, history);
    }

    public async Task AddClient(XmppClient client)
    {
        XmppClient[] targets;
        lock (_lock)
        {
            if (_clients.Contains(client))
            {
                return;
            }

            targets = _clients.ToArray();

            _clients.Add(client);
        }

        var tasks = targets.Select(c => client.SendAsync(MakePresence(c.Jid!, c.Username)));
        await Task.WhenAll(tasks);

        await Broadcast(MakePresence(client.Jid!, client.Username));

        Message[] messages;
        lock (_lock)
        {
            messages = _messages.ToArray();
        }

        foreach (var message in messages)
        {
            await client.SendAsync(message);
        }

        await SendSystemPresence(client);
        await SendWelcomeMessage(client);
    }

    public async Task RemoveClient(XmppClient client)
    {
        lock (_lock)
        {
            _clients.Remove(client);
        }

        if (client.Jid is null)
        {
            return;
        }

        var presenceElement = new Presence
        {
            From = new Jid(_name, Globals.XmppMucAddress, client.Jid.Local),
            Type = PresenceType.Unavailable
        };

        await Broadcast(presenceElement);
    }

    public async Task BroadcastMessage(XmppClient sender, string message, IChatMessageRepository chatMessageRepository)
    {
        if (sender.Jid is null)
        {
            return;
        }

        if (sender.ChatBanEnd.HasValue && sender.ChatBanEnd.Value > DateTime.UtcNow)
        {
            var remainingSeconds = (int) (sender.ChatBanEnd.Value - DateTime.UtcNow).TotalSeconds;
            await SendSystemMessage(sender, $"You are banned from chatting for another {remainingSeconds} seconds.");
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            await SendSystemMessage(sender, "Cannot send an empty message.");
            return;
        }

        var cooldown = _chatOptions.Value.MessageCooldownMilliseconds;
        if (cooldown > 0)
        {
            bool isOnCooldown;
            lock (_lock)
            {
                if (_lastMessageTimes.TryGetValue(sender, out var lastMessageTime)
                    && (DateTime.UtcNow - lastMessageTime).TotalMilliseconds < cooldown)
                {
                    isOnCooldown = true;
                }
                else
                {
                    _lastMessageTimes[sender] = DateTime.UtcNow;
                    isOnCooldown = false;
                }
            }

            if (isOnCooldown)
            {
                await SendSystemMessage(sender, "Please wait before sending another message.");
                return;
            }
        }

        var messageEntity = new ChatMessageEntity
        {
            Room = _name,
            SenderId = sender.UserId,
            Body = message,
            SentAt = DateTime.UtcNow
        };

        await chatMessageRepository.AddAsync(messageEntity);

        var messageElement = BuildMessageElement(messageEntity, _name, sender.Username, sender.Avatar);

        lock (_lock)
        {
            _messages.Add(messageElement);

            if (_messages.Count > Globals.MaxMessages)
            {
                _messages.RemoveAt(0);
            }
        }

        await Broadcast(messageElement);
    }

    public async Task Broadcast(XmppXElement element)
    {
        XmppClient[] targets;
        lock (_lock)
        {
            targets = _clients.ToArray();
        }

        var tasks = targets.Select(c => c.SendAsync(element));
        await Task.WhenAll(tasks);
    }

    private static Message BuildMessageElement(ChatMessageEntity entity, string roomName, string? senderName = null,
        string? senderAvatarId = null)
    {
        var messageElement = new Message
        {
            From = new Jid(roomName, Globals.XmppMucAddress, entity.SenderId.ToString()),
            Body = entity.Body,
            Type = MessageType.GroupChat,
            Delay = new Delay(entity.SentAt)
        };

        messageElement.SetAttribute("nck", senderName ?? entity.Player?.Name ?? "Unknown User");
        messageElement.SetAttribute("img", senderAvatarId ?? entity.Player?.AvatarId ?? "avatar001");

        return messageElement;
    }

    private Presence MakePresence(Jid jid, string nickname)
    {
        return new Presence
        {
            From = new Jid(_name, Globals.XmppMucAddress, jid.Local),
            Nick = nickname,
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

    private async Task SendSystemPresence(XmppClient client)
    {
        await client.SendAsync(MakePresence(_systemJid, "<color=red>SYSTEM</color>"));
    }

    private async Task SendWelcomeMessage(XmppClient client)
    {
        if (!string.IsNullOrWhiteSpace(_chatOptions.Value.WelcomeMessage))
        {
            await SendSystemMessage(client, _chatOptions.Value.WelcomeMessage);
        }

        if (_name.StartsWith("guild"))
        {
            await SendSystemMessage(client, "Welcome to the guild chat!");
        }
        else
        {
            await SendSystemMessage(client, "Welcome to the global chat!");
        }
    }

    private async Task SendSystemMessage(XmppClient client, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var messageElement = new Message
        {
            From = _systemJid,
            Body = message,
            Type = MessageType.GroupChat,
            Delay = new Delay(DateTime.UtcNow)
        };

        messageElement.SetAttribute("nck", "<color=red>SYSTEM</color>");
        messageElement.SetAttribute("img", "avatar006");

        await client.SendAsync(messageElement);
    }
}