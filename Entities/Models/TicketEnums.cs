using System;

namespace Entities.Models
{
    public enum TicketStatus
    {
        Open = 0,
        InProgress = 1,
        Resolved = 2,
        Closed = 3
    }
   

    public enum MessageSenderType
    {
        Customer = 0,
        Agent = 1,
        System = 2
    }

    public enum NotificationType
    {
        NewTicket = 0,
        NewMessage = 1,
        StatusChanged = 2
    }
}
