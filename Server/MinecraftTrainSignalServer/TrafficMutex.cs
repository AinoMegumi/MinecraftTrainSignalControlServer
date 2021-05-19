using StackExchange.Redis;
using System;
using System.Configuration;

namespace MinecraftTrainSignalServer
{
    public class TrafficMutex
    {
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisConfig"]));
        private static ConnectionMultiplexer Connection { get => lazyConnection.Value; }
        private readonly IDatabase db;
        private readonly string RouteID;
        public TrafficMutex(string RouteID)
        {
            this.RouteID = RouteID;
            db = Connection.GetDatabase();
            if (db.HashExists("mutex", RouteID) && db.HashGet("mutex", RouteID) == 1)
            {
                while (db.HashGet("mutex", RouteID) == 1) ;
            }
            db.HashSet("mutex", RouteID, 1);
        }
        ~TrafficMutex()
        {
            db.HashSet("mutex", RouteID, 0);
        }
    }
}
