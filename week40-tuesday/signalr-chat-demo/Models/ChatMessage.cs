namespace signalr_chat.Models
{
  public class ChatMessage
  {
    public int Id { get; set; } // Auto-increment ID
    public Guid? ConversationId { get; set; }
    public required string Username { get; set; } // Username of the sender
    public required string Message { get; set; } // Chat message
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Timestamp when the message was sent
  }
}