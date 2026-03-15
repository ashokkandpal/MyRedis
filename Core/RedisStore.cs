using System.Collections.Concurrent;

namespace MyRedis.Core
{
    public class RedisStore
    {
        private readonly ConcurrentDictionary<string, object> _redisDatabase = new();

        private const string WrongTypeError =
            "-WRONGTYPE Operation against a key holding the wrong kind of value";

        public RedisStore() { }

        public string Set(string key, string value)
        {
            _redisDatabase[key] = value;
            return "OK";
        }

        public string? Get(string key)
        {
            if (!_redisDatabase.TryGetValue(key, out object? obj))
                return null;

            if (obj is not string)
                return WrongTypeError;

            return obj as string;
        }

        public bool Delete(string key)
        {
            return _redisDatabase.TryRemove(key, out _);
        }

        public bool Exists(string key)
        {
            return _redisDatabase.ContainsKey(key);
        }

        public List<string> GetAllKeys()
        {
            return [.. _redisDatabase.Keys];
        }

        public string FlushAll()
        {
            _redisDatabase.Clear();
            return "OK";
        }

        public int DbSize()
        {
            return _redisDatabase.Count;
        }

        public string? Incr(string key)
        {
            if (Exists(key) && GetDataType(key) != DataType.String)
                return WrongTypeError;

            string? value = Get(key) ?? "0";

            if (long.TryParse(value, out long number))
            {
                number++;
                string newValue = number.ToString();
                Set(key, newValue);
                return newValue;
            }

            return null;
        }

        #region Helper Method
        public DataType GetDataType(string key)
        {
            if (!_redisDatabase.TryGetValue(key, out object? value))
                return DataType.None;

            return value switch
            {
                string => DataType.String,
                List<string> => DataType.List,
                Dictionary<string, string> => DataType.Hash,
                HashSet<string> => DataType.Set,
                SortedDictionary<double, string> => DataType.SortedSet,
                _ => DataType.None
            };
        }
        #endregion

        #region List Commands        
        public int LPush(string key, string value)
        {
            if (Exists(key) && GetDataType(key) != DataType.List)
                throw new InvalidOperationException(WrongTypeError);

            List<string>? list = _redisDatabase.GetOrAdd(key, _ => new List<string>()) as List<string>;
            lock (list!)
            {
                list.Insert(0, value);
                return list.Count;
            }
        }

        public int RPush(string key, string value)
        {
            if (Exists(key) && GetDataType(key) != DataType.List)
                throw new InvalidOperationException(WrongTypeError);

            List<string>? list = _redisDatabase.GetOrAdd(key, _ => new List<string>()) as List<string>;
            lock (list!)
            {
                list.Add(value);
                return list.Count;
            }
        }

        public string? LPop(string key)
        {
            if (!Exists(key)) return null;
            if (GetDataType(key) != DataType.List)
                throw new InvalidOperationException(WrongTypeError);

            List<string>? list = _redisDatabase[key] as List<string>;
            lock (list!)
            {
                if (list.Count == 0) return null;
                string val = list[0];
                list.RemoveAt(0);
                return val;
            }
        }

        public string? RPop(string key)
        {
            if (!Exists(key)) return null;
            if (GetDataType(key) != DataType.List)
                throw new InvalidOperationException(WrongTypeError);

            List<string>? list = _redisDatabase[key] as List<string>;
            lock (list!)
            {
                if (list.Count == 0) return null;
                string val = list[^1];
                list.RemoveAt(list.Count - 1);
                return val;
            }
        }

        public List<string> LRange(string key, int start, int stop)
        {
            if (!Exists(key)) return [];
            if (GetDataType(key) != DataType.List)
                throw new InvalidOperationException(WrongTypeError);

            List<string>? list = _redisDatabase[key] as List<string>;
            lock (list!)
            {
                int count = list.Count;
                if (start < 0) start = Math.Max(0, count + start);
                if (stop < 0) stop = count + stop;
                stop = Math.Min(stop, count - 1);
                if (start > stop) return [];
                return list.GetRange(start, stop - start + 1);
            }
        }

        public int LLen(string key)
        {
            if (!Exists(key)) return 0;
            if (GetDataType(key) != DataType.List)
                throw new InvalidOperationException(WrongTypeError);

            List<string>? list = _redisDatabase[key] as List<string>;
            lock (list!) return list.Count;
        }
        #endregion

        #region Hash Commands

        public int HSet(string key, string field, string value)
        {
            if (Exists(key) && GetDataType(key) != DataType.Hash)
                throw new InvalidOperationException(WrongTypeError);

            Dictionary<string, string>? hash = _redisDatabase.GetOrAdd(key, _ => new Dictionary<string, string>())
                       as Dictionary<string, string>;
            lock (hash!)
            {
                bool isNew = !hash.ContainsKey(field);
                hash[field] = value;
                return isNew ? 1 : 0;
            }
        }

        public string? HGet(string key, string field)
        {
            if (!Exists(key)) return null;
            if (GetDataType(key) != DataType.Hash)
                throw new InvalidOperationException(WrongTypeError);

            Dictionary<string, string>? hash = _redisDatabase[key] as Dictionary<string, string>;
            lock (hash!)
            {
                hash.TryGetValue(field, out string? value);
                return value;
            }
        }

        public Dictionary<string, string> HGetAll(string key)
        {
            if (!Exists(key)) return [];
            if (GetDataType(key) != DataType.Hash)
                throw new InvalidOperationException(WrongTypeError);

            Dictionary<string, string>? hash = _redisDatabase[key] as Dictionary<string, string>;
            lock (hash!) return new Dictionary<string, string>(hash);
        }

        public bool HDel(string key, string field)
        {
            if (!Exists(key)) return false;
            if (GetDataType(key) != DataType.Hash)
                throw new InvalidOperationException(WrongTypeError);

            Dictionary<string, string>? hash = _redisDatabase[key] as Dictionary<string, string>;
            lock (hash!) return hash.Remove(field);
        }
        #endregion

        #region Set Commands

        public int SAdd(string key, string value)
        {
            if (Exists(key) && GetDataType(key) != DataType.Set)
                throw new InvalidOperationException(WrongTypeError);

            HashSet<string>? set = _redisDatabase.GetOrAdd(key, _ => new HashSet<string>())
                      as HashSet<string>;
            lock (set!) return set.Add(value) ? 1 : 0;
        }

        public HashSet<string> SMembers(string key)
        {
            if (!Exists(key)) return [];
            if (GetDataType(key) != DataType.Set)
                throw new InvalidOperationException(WrongTypeError);

            HashSet<string>? set = _redisDatabase[key] as HashSet<string>;
            lock (set!) return new HashSet<string>(set);
        }

        public int SRem(string key, string value)
        {
            if (!Exists(key)) return 0;
            if (GetDataType(key) != DataType.Set)
                throw new InvalidOperationException(WrongTypeError);

            HashSet<string>? set = _redisDatabase[key] as HashSet<string>;
            lock (set!) return set.Remove(value) ? 1 : 0;
        }

        public bool SIsMember(string key, string value)
        {
            if (!Exists(key)) return false;
            if (GetDataType(key) != DataType.Set)
                throw new InvalidOperationException(WrongTypeError);

            HashSet<string>? set = _redisDatabase[key] as HashSet<string>;
            lock (set!) return set.Contains(value);
        }
        #endregion

        #region Sorted Set Commands

        public int ZAdd(string key, double score, string value)
        {
            if (Exists(key) && GetDataType(key) != DataType.SortedSet)
                throw new InvalidOperationException(WrongTypeError);

            SortedDictionary<double, string>? sortedSet = _redisDatabase.GetOrAdd(key, _ => new SortedDictionary<double, string>())
                       as SortedDictionary<double, string>;
            lock (sortedSet!)
            {
                // Check if value already exists under a different score
                double? existingScore = null;
                foreach (KeyValuePair<double, string> kvp in sortedSet)
                {
                    if (kvp.Value == value)
                    {
                        existingScore = kvp.Key;
                        break;
                    }
                }

                if (existingScore.HasValue)
                {
                    sortedSet.Remove(existingScore.Value);
                    sortedSet[score] = value;
                    return 0;
                }

                sortedSet[score] = value;
                return 1;
            }
        }

        public List<string> ZRange(string key, int start, int stop)
        {
            if (!Exists(key)) return [];
            if (GetDataType(key) != DataType.SortedSet)
                throw new InvalidOperationException(WrongTypeError);

            SortedDictionary<double, string>? sortedSet = _redisDatabase[key] as SortedDictionary<double, string>;
            lock (sortedSet!)
            {
                List<string> values = [.. sortedSet.Values];
                int count = values.Count;
                if (start < 0) start = Math.Max(0, count + start);
                if (stop < 0) stop = count + stop;
                stop = Math.Min(stop, count - 1);
                if (start > stop) return [];
                return values.GetRange(start, stop - start + 1);
            }
        }

        public double? ZScore(string key, string value)
        {
            if (!Exists(key)) return null;
            if (GetDataType(key) != DataType.SortedSet)
                throw new InvalidOperationException(WrongTypeError);

            SortedDictionary<double, string>? sortedSet = _redisDatabase[key] as SortedDictionary<double, string>;
            lock (sortedSet!)
            {
                foreach (KeyValuePair<double, string> kvp in sortedSet)
                {
                    if (kvp.Value == value)
                        return kvp.Key;
                }
                return null;
            }
        }

        public int ZCard(string key)
        {
            if (!Exists(key)) return 0;
            if (GetDataType(key) != DataType.SortedSet)
                throw new InvalidOperationException(WrongTypeError);

            SortedDictionary<double, string>? sortedSet = _redisDatabase[key] as SortedDictionary<double, string>;
            lock (sortedSet!) return sortedSet.Count;
        }
        #endregion
    }
}