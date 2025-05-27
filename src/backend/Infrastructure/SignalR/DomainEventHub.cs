using Microsoft.AspNetCore.SignalR;

namespace MyLeague.Infrastructure.SignalR
{
    /// <summary>
    /// SignalR hub for domain events
    /// </summary>
    public class DomainEventHub : Hub
    {
        /// <summary>
        /// Initializes a new instance of the DomainEventHub class
        /// </summary>
        public DomainEventHub() 
        {
        }
        
        /// <summary>
        /// Connection ID for the current connection
        /// </summary>
        /// <returns>The connection ID</returns>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
} 