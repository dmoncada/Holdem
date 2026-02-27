using System.Collections.Concurrent;

namespace Holdem.Server
{
    public class PokerSessionManager
    {
        private readonly ConcurrentDictionary<string, PokerSession> _sessions = new();

        public PokerSession GetOrCreate(string sessionId)
        {
            return _sessions.GetOrAdd(sessionId, id => new PokerSession(id));
        }
    }
}
