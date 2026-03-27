using MyRedis.Core;

namespace MyRedis.Commands.Expiry
{
    public class TtlCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
                return "-ERR wrong number of arguments for 'TTL' command";

            try
            {
                return _redisStore.TTL(args[0]);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}