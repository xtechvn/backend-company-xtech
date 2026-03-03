public class TicketMessageDto
{
    public long Id { get; set; }
    public Guid TicketId { get; set; }
    public string SenderType { get; set; } = default!; // "Agent" | "Customer"
    public string SenderId { get; set; } = default!;
    public string? Content { get; set; }
    public string? ContentHtml { get; set; }
    public string CreatedAt { get; set; } = default!; // string format để render
}