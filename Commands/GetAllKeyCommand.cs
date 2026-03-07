using System.Text;
using MyRedis.Core;

namespace MyRedis.Commands
{
    public class GetAllKeyCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR Wrong Number of parameters for 'KEYS' command";
            }

            List<string> allKeyList = _redisStore.GetAllKeys();

            if (allKeyList.Count == 0)
                return "(empty)";

            StringBuilder allKeys = new();
            for (int i = 0; i < allKeyList.Count; i++)
            {
                allKeys.Append($"{i + 1}) {allKeyList[i]}");
            }

            return allKeys.ToString().TrimEnd();
        }
    }
}
