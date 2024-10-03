namespace signalr_chat.Models
{
  public class Conversation
  {
    public Guid Id { get; set; } = Guid.NewGuid(); // Unique for each convo
    public required string Participant1 { get; set; }
    public required string Participant2 { get; set; }
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
  }
}