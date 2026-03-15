using MyRedis.Core;

namespace MyRedis.Commands.Set
{
    public class SIsMemberCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'SISMEMBER' command";
            }

            string key = args[0];
            string value = args[1];

            try
            {
                bool result = _redisStore.SIsMember(key, value);
                return result ? "1" : "0";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}