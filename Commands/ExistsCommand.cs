using MyRedis.Core;

namespace MyRedis.Commands
{
    public class ExistsCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if(args.Length == 0)
            {
                return "-ERR Wrong number of parameters for 'Exists' Command";
            }

            return _redisStore.Exists(args[0])? "1" : "0";
        }
    }
}
