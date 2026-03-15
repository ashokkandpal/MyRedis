using System.Text;
using MyRedis.Core;

namespace MyRedis.Commands.Set
{
    public class SMembersCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 1)
            {
                return "-ERR wrong number of arguments for 'SMEMBERS' command";
            }

            string key = args[0];

            try
            {
                HashSet<string> result = _redisStore.SMembers(key);

                if (result.Count == 0)
                    return "(empty)";

                StringBuilder sb = new();
                int index = 1;
                foreach (string member in result)
                {
                    sb.AppendLine($"{index++}) {member}");
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