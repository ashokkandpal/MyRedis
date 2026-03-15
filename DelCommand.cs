using MyRedis.Core;

namespace MyRedis.Commands
{
    public class DelCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;
        public string Execute(string[] args)
        {
            if (args.Length == 0)
            {
                return "-ERR wrong number of arguments for 'DEL' command";
            }

            int deletedCount = 0;

            foreach (string key in args)
            {
                if (_redisStore.Delete(key))
                {
                    deletedCount++;
                }
            }

            return deletedCount.ToString();
        }
    }
}