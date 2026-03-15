using MyRedis.Commands.Hash;
using MyRedis.Commands.List;
using MyRedis.Commands.Set;
using MyRedis.Commands.SortedSet;
using MyRedis.Core;

namespace MyRedis.Commands
{
    public class CommandHandler
    {
        private readonly RedisStore _store;
        private readonly Dictionary<string, ICommand> _commands;

        public CommandHandler(RedisStore store)
        {
            _store = store;
            _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "SET", new SetCommand(store) },
                { "GET", new GetCommand(store) },
                { "DEL", new DelCommand(store) },
                { "EXISTS", new ExistsCommand(store) },
                { "KEYS", new GetAllKeyCommand(store) },
                { "FLUSHALL", new FlushAllCommand(store) },
                { "DBSIZE", new DbSizeCommand(store) },
                { "INCR", new IncrCommand(store) },
                // List
                { "LPUSH", new LPushCommand(store) },
                { "RPUSH", new RPushCommand(store) },
                { "LPOP", new LPopCommand(store) },
                { "RPOP", new RPopCommand(store) },
                { "LRANGE", new LRangeCommand(store) },
                { "LLEN", new LLenCommand(store) },
                // Hash
                { "HSET", new HSetCommand(store) },
                { "HGET", new HGetCommand(store) },
                { "HGETALL", new HGetAllCommand(store) },
                { "HDEL", new HDelCommand(store) },
                // Set
                { "SADD", new SAddCommand(store) },
                { "SMEMBERS", new SMembersCommand(store) },
                { "SREM", new SRemCommand(store) },
                { "SISMEMBER", new SIsMemberCommand(store) },
                // Sorted Set
                { "ZADD", new ZAddCommand(store) },
                { "ZRANGE", new ZRangeCommand(store) },
                { "ZSCORE", new ZScoreCommand(store) },
                { "ZCARD", new ZCardCommand(store) }
            };
        }

        public string Handle(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "-ERR empty command";

            string[] inputParts = input.Split(' ');

            if (_commands.TryGetValue(inputParts[0], out ICommand? command))
            {
                string[] args = [.. inputParts.Skip(1).Select(a => a.Trim('"'))];

                return command.Execute(args);
            }

            return $"-ERR unknown command '{inputParts[0]}'";
        }
    }
}
