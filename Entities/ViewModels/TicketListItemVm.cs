using Entities.Models;
using System.ComponentModel.DataAnnotations;

public class TicketListItemVm
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Subject { get; set; } = "";
    public int ServiceId { get; set; }
    public int DepartmentId { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    [MaxLength(450)]
    public string? AssignedAgentId { get; set; }

    public string? ServiceName { get; set; }
    public string? DepartmentName { get; set; }
}