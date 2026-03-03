using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ultilities
{
    public class TicketHub : Hub
    {
        public Task JoinTicket(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId)) return Task.CompletedTask;
            return Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public Task LeaveTicket(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId)) return Task.CompletedTask;
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }
    }
}

