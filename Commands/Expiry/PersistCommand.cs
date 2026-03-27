using MyRedis.Core;

namespace MyRedis.Commands.Expiry
{
    public class PersistCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
                return "-ERR wrong number of arguments for 'PERSIST' command";

            try
            {
                return _redisStore.Persist(args[0]);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}