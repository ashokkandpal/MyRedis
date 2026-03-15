using MyRedis.Core;

namespace MyRedis.Commands.SortedSet
{
    public class ZScoreCommand(RedisStore redisStore) : ICommand
    {
        private readonly RedisStore _redisStore = redisStore;

        public string Execute(string[] args)
        {
            if (args.Length != 2)
            {
                return "-ERR wrong number of arguments for 'ZSCORE' command";
            }

            string key = args[0];
            string value = args[1];

            try
            {
                double? score = _redisStore.ZScore(key, value);
                return score.HasValue ? score.Value.ToString() : "(nil)";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }
    }
}