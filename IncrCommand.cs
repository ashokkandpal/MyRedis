using MyRedis.Core;

namespace MyRedis.Commands
{
    public class IncrCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR Wrong number of parameters for 'INCR' command";
            }

            string key = args[0];
            string? result = _redisStore.Incr(key);
            return result ?? "-ERR value is not an integer or out of range";
        }
    }
}
