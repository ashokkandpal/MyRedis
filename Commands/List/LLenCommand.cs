using MyRedis.Core;

namespace MyRedis.Commands.List
{
    public class LLenCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR wrong number of arguments for 'LLEN' command";
            }

            string key = args[0];

            try
            {
                int length = _redisStore.LLen(key);
                return length.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}