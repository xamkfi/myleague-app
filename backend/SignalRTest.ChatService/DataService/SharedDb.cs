using System.Collections.Concurrent;
using SignalRTest.ChatService.Models;

namespace SignalRTest.ChatService.DataService
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connections = new();

        public ConcurrentDictionary<string, UserConnection> connections => _connections;
    }
}
