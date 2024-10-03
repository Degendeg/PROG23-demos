namespace signalr_chat.Hubs
{
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.SignalR;
  using signalr_chat.Data;
  using signalr_chat.Models;
  using System.Threading.Tasks;

  [Authorize]
  public class ChatHub : Hub
  {
    private readonly ChatContext _context;

    public ChatHub(ChatContext context)
    {
      _context = context;
    }

    public override async Task OnConnectedAsync()
    {
      if (Context.User?.Identity != null && Context.User.Identity.IsAuthenticated)
      {
        var messages = _context.ChatMessages
            .OrderBy(m => m.Timestamp)
            .Take(50) // Limit to last 50 messages
            .ToList();

        foreach (var message in messages)
        {
          await Clients.Caller.SendAsync("ReceiveMessage", message.Username, message.Message);
        }
      }
      else
      {
        await Clients.Caller.SendAsync("ReceiveMessage", "System", "You are not authorized.");
      }
      await base.OnConnectedAsync();
    }

    public async Task SendMessage(string user, string message)
    {
      // Save message to database
      var chatMessage = new ChatMessage
      {
        Username = user,
        Message = message
      };

      _context.ChatMessages.Add(chatMessage);
      await _context.SaveChangesAsync(); // Save to database

      // Broadcast the message to all connected clients
      await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task StartPrivateChat(string targetUser)
    {
      var currentUser = Context.User?.Identity?.Name;

      // Check if a conversation exists between these users
      var conversation = _context.Conversations
          .FirstOrDefault(c => (c.Participant1 == currentUser && c.Participant2 == targetUser) ||
                               (c.Participant1 == targetUser && c.Participant2 == currentUser));

      if (conversation == null && currentUser != null)
      {
        // Create a new conversation if none exists
        conversation = new Conversation
        {
          Participant1 = currentUser,
          Participant2 = targetUser
        };
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
      }

      // Fetch previous messages from this conversation
      var messages = _context.ChatMessages
          .Where(m => m.ConversationId == conversation.Id)
          .OrderBy(m => m.Timestamp)
          .Select(m => new
          {
            Username = m.Username,
            Message = m.Message,
            Timestamp = m.Timestamp
          })
          .ToList();

      // Send conversation and message history to the caller
      await Clients.Caller.SendAsync("OpenPrivateChat", conversation.Id, targetUser, messages);
      // Notify the target user to open the same chat
      await Clients.User(targetUser).SendAsync("OpenPrivateChat", conversation.Id, currentUser, messages);
    }

    public async Task SendPrivateMessage(Guid conversationId, string message)
    {
      var currentUser = Context.User?.Identity?.Name;
      var users = _context.Users.ToList();
      var conversation = _context.Conversations.Find(conversationId);

      if (conversation != null && currentUser != null)
      {
        // Save the message in the convo
        var chatMessage = new ChatMessage
        {
          Username = currentUser,
          ConversationId = conversationId,
          Message = message,
          Timestamp = DateTime.UtcNow
        };

        conversation.Messages.Add(chatMessage);
        await _context.SaveChangesAsync();

        // Get the list of all users
        var allUsers = _context.Users.Select(u => u.Username).ToList();

        // Create a list of users that are outside the conversation
        var usersOutsideConvo = allUsers
            .Where(u => u != conversation.Participant1 && u != conversation.Participant2)
            .ToList();

        // Send the message to all users except those outside the conversation
        await Clients.AllExcept(usersOutsideConvo).SendAsync("ReceivePrivateMessage", conversation.Id, currentUser, message);
      }
    }
  }
}