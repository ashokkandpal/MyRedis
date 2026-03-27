using MyRedis.Core;

namespace MyRedis.Commands.Expiry
{
    public class ExpireCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
                return "-ERR wrong number of arguments for 'EXPIRE' command";

            if (!long.TryParse(args[1], out long seconds))
                return "-ERR value is not an integer or out of range";

            try
            {
                return _redisStore.Expire(args[0], seconds);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}