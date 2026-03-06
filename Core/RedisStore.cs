using System.Collections.Concurrent;

namespace MyRedis.Core
{
    public class RedisStore
    {
        private readonly ConcurrentDictionary<string, string> redisDatabase = new();
        public RedisStore()
        {
        }

        public string Set(string key, string value)
        {
            redisDatabase[key] = value;
            return "OK";
        }

        public string? Get(string key)
        {
            redisDatabase.TryGetValue(key, out string? value);
            return value;
        }

        public bool Delete(string key)
        {
            return redisDatabase.TryRemove(key, out _);
        }

        public bool Exists(string key)
        {
            return redisDatabase.ContainsKey(key);
        }

        public List<string> GetAllKeys()
        {
            return [.. redisDatabase.Keys];
        }

        public string FlushAll()
        {
            redisDatabase.Clear();
            return "OK";
        }

        public int DbSize()
        {
            return redisDatabase.Count;
        }

        public string? Incr(string key)
        {
            string? value = Get(key);

            if (value == null)
            {
                Set(key, "1");
                return "1";
            }

            if (long.TryParse(value, out long number))
            {
                number++;
                string newValue = number.ToString();
                Set(key, newValue);
                return newValue;
            }

            return null;
        }
    };
}