using MyRedis.Core;

namespace MyRedis.Commands.SortedSet
{
    public class ZAddCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 3)
            {
                return "-ERR wrong number of arguments for 'ZADD' command";
            }

            string key = args[0];

            if (!double.TryParse(args[1], out double score))
            {
                return "-ERR value is not a valid float";
            }

            string value = args[2];

            try
            {
                int result = _redisStore.ZAdd(key, score, value);
                return result.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}