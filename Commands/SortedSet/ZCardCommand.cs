using MyRedis.Core;

namespace MyRedis.Commands.SortedSet
{
    public class ZCardCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR wrong number of arguments for 'ZCARD' command";
            }

            string key = args[0];

            try
            {
                int count = _redisStore.ZCard(key);
                return count.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}