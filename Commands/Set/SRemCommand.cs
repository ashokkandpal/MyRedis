using MyRedis.Core;

namespace MyRedis.Commands.Set
{
    public class SRemCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'SREM' command";
            }

            string key = args[0];
            string value = args[1];

            try
            {
                int result = _redisStore.SRem(key, value);
                return result.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}