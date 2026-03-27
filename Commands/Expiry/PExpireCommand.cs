using MyRedis.Core;

namespace MyRedis.Commands.Expiry
{
    public class PExpireCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
                return "-ERR wrong number of arguments for 'PEXPIRE' command";

            if (!long.TryParse(args[1], out long milliseconds))
                return "-ERR value is not an integer or out of range";

            try
            {
                return _redisStore.PExpire(args[0], milliseconds);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}