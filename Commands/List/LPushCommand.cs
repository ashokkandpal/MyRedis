using MyRedis.Core;

namespace MyRedis.Commands.List
{
    public class LPushCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'LPUSH' command";
            }

            string key = args[0];
            string value = args[1];

            try
            {
                int length = _redisStore.LPush(key, value);
                return length.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}