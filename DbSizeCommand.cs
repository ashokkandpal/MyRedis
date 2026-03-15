using MyRedis.Core;

namespace MyRedis.Commands
{
    public class DbSizeCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if(args.Length > 0)
            {
                return "-ERR Wrong number of parameters for 'DBSIZE' command";
            }

            return _redisStore.DbSize().ToString();
        }
    }
}
