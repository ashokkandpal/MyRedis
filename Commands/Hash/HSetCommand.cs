using MyRedis.Core;

namespace MyRedis.Commands.Hash
{
    public class HSetCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 3)
            {
                return "-ERR wrong number of arguments for 'HSET' command";
            }

            string key = args[0];
            string field = args[1];
            string value = args[2];

            try
            {
                int result = _redisStore.HSet(key, field, value);
                return result.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}