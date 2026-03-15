using MyRedis.Core;

namespace MyRedis.Commands.List
{
    public class LRangeCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 3)
            {
                return "-ERR wrong number of arguments for 'LRANGE' command";
            }

            string key = args[0];

            if (!int.TryParse(args[1], out int start) || !int.TryParse(args[2], out int stop))
            {
                return "-ERR value is not an integer or out of range";
            }

            try
            {
                List<string> result = _redisStore.LRange(key, start, stop);

                if (result.Count == 0)
                    return "(empty)";

                return string.Join("\n", result.Select((v, i) => $"{i + 1}) {v}"));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}