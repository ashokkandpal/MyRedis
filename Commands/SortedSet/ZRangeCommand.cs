using System.Text;
using MyRedis.Core;

namespace MyRedis.Commands.SortedSet
{
    public class ZRangeCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 3)
            {
                return "-ERR wrong number of arguments for 'ZRANGE' command";
            }

            string key = args[0];

            if (!int.TryParse(args[1], out int start) || !int.TryParse(args[2], out int stop))
            {
                return "-ERR value is not an integer or out of range";
            }

            try
            {
                List<string> result = _redisStore.ZRange(key, start, stop);

                if (result.Count == 0)
                    return "(empty)";

                StringBuilder sb = new();
                for (int i = 0; i < result.Count; i++)
                {
                    sb.AppendLine($"{i + 1}) {result[i]}");
                }
                return sb.ToString().TrimEnd();
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}