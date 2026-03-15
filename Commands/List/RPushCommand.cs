using MyRedis.Core;

namespace MyRedis.Commands.List
{
    public class RPushCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'RPUSH' command";
            }

            string key = args[0];
            string value = args[1];

            try
            {
                int length = _redisStore.RPush(key, value);
                return length.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}