using MyRedis.Core;

namespace MyRedis.Commands
{
    public class SetCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "Error: SET command requires exactly 2 arguments.";
            }

            string key = args[0];
            string value = args[1];
            return _redisStore.Set(key, value);
        }
    }
}
