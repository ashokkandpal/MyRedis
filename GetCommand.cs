using MyRedis.Core;

namespace MyRedis.Commands
{
    public class GetCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;
        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR wrong number of arguments for 'GET' command"; ;
            }

            string key = args[0];
            string? value = _redisStore.Get(key);
            return value ?? "(nil)";
        }
    }
}
