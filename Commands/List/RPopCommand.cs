using MyRedis.Core;

namespace MyRedis.Commands.List
{
    public class RPopCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR wrong number of arguments for 'RPOP' command";
            }

            string key = args[0];

            try
            {
                string? value = _redisStore.RPop(key);
                return value ?? "(nil)";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}