using MyRedis.Core;

namespace MyRedis.Commands.Hash
{
    public class HDelCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'HDEL' command";
            }

            string key = args[0];
            string field = args[1];

            try
            {
                bool result = _redisStore.HDel(key, field);
                return result ? "1" : "0";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}