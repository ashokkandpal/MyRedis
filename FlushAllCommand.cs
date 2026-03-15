using MyRedis.Core;

namespace MyRedis.Commands
{
    public class FlushAllCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if(args.Length > 1)
            {
                return "-ERR Wrong number of parameters for 'FLUSHALL' command";
            }

            return _redisStore.FlushAll();
        }
    }
}
