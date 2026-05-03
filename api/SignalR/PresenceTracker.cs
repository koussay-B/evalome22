using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<int, List<string>> OnlineUsers = new();

        // [
        //     userID :[connection1,connecttion],
        //     userId2 :[connection1],
        //     1 :[con11,jkbj],
        //     2:[swqdq]
        // ]

        public Task<bool> UserConnected(int userId, string connectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.TryGetValue(userId, out var connections))
                {
                    connections.Add(connectionId);
                }
                else
                {
                    OnlineUsers[userId] = new List<string> { connectionId };
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(int userId, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.TryGetValue(userId, out var connections))
                {
                    return Task.FromResult(isOffline);
                }

                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    OnlineUsers.Remove(userId);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<int[]> GetOnlineUsers()
        {
            int[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.Keys.Order().ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<int> GetOnlineCount()
        {
            int count;
            lock (OnlineUsers)
            {
                count = OnlineUsers.Count;
            }
            return Task.FromResult(count);
        }

        public Task<List<string>> GetConnectionsForUser(int userId)
        {
            lock (OnlineUsers)
            {
                if (OnlineUsers.TryGetValue(userId, out var connections))
                {
                    // Return a copy to avoid external modification
                    return Task.FromResult(new List<string>(connections));
                }
            }

            return Task.FromResult(new List<string>());
        }
    }
}