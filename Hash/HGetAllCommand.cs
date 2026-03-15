using System.Text;
using MyRedis.Core;

namespace MyRedis.Commands.Hash
{
    public class HGetAllCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR wrong number of arguments for 'HGETALL' command";
            }

            string key = args[0];

            try
            {
                Dictionary<string, string> result = _redisStore.HGetAll(key);

                if (result.Count == 0)
                    return "(empty)";

                StringBuilder sb = new();
                int index = 1;
                foreach (KeyValuePair<string, string> kvp in result)
                {
                    sb.AppendLine($"{index++}) {kvp.Key}");
                    sb.AppendLine($"{index++}) {kvp.Value}");
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