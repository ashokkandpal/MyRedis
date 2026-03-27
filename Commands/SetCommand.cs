using MyRedis.Core;

namespace MyRedis.Commands
{
    public class SetCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length < 2)
                return "-ERR wrong number of arguments for 'SET' command";

            string key = args[0];
            string value = args[1];
            long? expiryMs = null;

            if (args.Length == 4)
            {
                string flag = args[2].ToUpper();
                if (flag == "EX")
                {
                    if (long.TryParse(args[3], out long seconds))
                        expiryMs = seconds * 1000;
                    else
                        return "-ERR value is not an integer or out of range";
                }
                else if (flag == "PX")
                {
                    if (long.TryParse(args[3], out long ms))
                        expiryMs = ms;
                    else
                        return "-ERR value is not an integer or out of range";
                }
            }

            return _redisStore.Set(key, value, expiryMs);
        }
    }
}
