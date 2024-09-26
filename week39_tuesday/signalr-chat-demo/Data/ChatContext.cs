using Microsoft.EntityFrameworkCore;
using signalr_chat.Models;

namespace signalr_chat.Data
{
  public class ChatContext : DbContext
  {
    public ChatContext(DbContextOptions<ChatContext> options) : base(options) { }

    public DbSet<ChatMessage> ChatMessages { get; set; } // The table of chat messages

    public DbSet<User> Users { get; set; } // The table consisting of users
  }
}