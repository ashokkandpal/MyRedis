using MyRedis.Core;

namespace MyRedis.Commands.Hash
{
    public class HGetCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'HGET' command";
            }

            string key = args[0];
            string field = args[1];

            try
            {
                string? value = _redisStore.HGet(key, field);
                return value ?? "(nil)";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}