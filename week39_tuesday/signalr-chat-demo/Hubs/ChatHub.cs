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
  }
}