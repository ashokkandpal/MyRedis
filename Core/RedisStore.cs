using System.Collections.Concurrent;

namespace MyRedis.Core
{
    public class RedisStore
    {
        private readonly ConcurrentDictionary<string, object> _redisDatabase = new();
        private readonly ConcurrentDictionary<string, long> _expiry = new();

        private const string WrongTypeError =
            "-WRONGTYPE Operation against a key holding the wrong kind of value";

        public RedisStore() { }

        private bool IsExpired(string key)
        {
            if (_expiry.TryGetValue(key, out long expiryTime))
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > expiryTime)
                {
                    _redisDatabase.TryRemove(key, out _);
                    _expiry.TryRemove(key, out _);
                    return true;
                }
            }
            return false;
        }

        private void SetExpiry(string key, long milliseconds)
        {
            _expiry[key] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + milliseconds;
        }

        public string Set(string key, string value, long? expiryMilliseconds = null)
        {
            _redisDatabase[key] = value;
            if (expiryMilliseconds.HasValue)
            {
                SetExpiry(key, expiryMilliseconds.Value);
            }
            else
            {
                _expiry.TryRemove(key, out _);
            }
            return "OK";
        }

        public string? Get(string key)
        {
            if (IsExpired(key)) return null;

            if (!_redisDatabase.TryGetValue(key, out object? obj))
                return null;

            if (obj is not string)
                return WrongTypeError;

            return obj as string;
        }

        public bool Delete(string key)
        {
            _expiry.TryRemove(key, out _);
            return _redisDatabase.TryRemove(key, out _);
        }

        public bool Exists(string key)
        {
            if (IsExpired(key)) return false;
            return _redisDatabase.ContainsKey(key);
        }

        public List<string> GetAllKeys()
        {
            return [.. _redisDatabase.Keys.Where(k => !IsExpired(k))];
        }

        public string FlushAll()
        {
            _redisDatabase.Clear();
            _expiry.Clear();
            return "OK";
        }

        public int DbSize()
        {
            return _redisDatabase.Keys.Count(k => !IsExpired(k));
        }

        public string? Incr(string key)
        {
            if (IsExpired(key)) return null; // Or just let Exists handle it

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
            if (IsExpired(key)) return DataType.None;

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
            if (IsExpired(key)) { /* let it recreate list */ }

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
            if (IsExpired(key)) { /* let it recreate list */ }

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
            if (IsExpired(key)) return null;
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
            if (IsExpired(key)) return null;
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
            if (IsExpired(key)) return [];
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
            if (IsExpired(key)) return 0;
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
            if (IsExpired(key)) return null;
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
            if (IsExpired(key)) return [];
            if (!Exists(key)) return [];
            if (GetDataType(key) != DataType.Hash)
                throw new InvalidOperationException(WrongTypeError);

            Dictionary<string, string>? hash = _redisDatabase[key] as Dictionary<string, string>;
            lock (hash!) return new Dictionary<string, string>(hash);
        }

        public bool HDel(string key, string field)
        {
            if (IsExpired(key)) return false;
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
            if (IsExpired(key)) return [];
            if (!Exists(key)) return [];
            if (GetDataType(key) != DataType.Set)
                throw new InvalidOperationException(WrongTypeError);

            HashSet<string>? set = _redisDatabase[key] as HashSet<string>;
            lock (set!) return new HashSet<string>(set);
        }

        public int SRem(string key, string value)
        {
            if (IsExpired(key)) return 0;
            if (!Exists(key)) return 0;
            if (GetDataType(key) != DataType.Set)
                throw new InvalidOperationException(WrongTypeError);

            HashSet<string>? set = _redisDatabase[key] as HashSet<string>;
            lock (set!) return set.Remove(value) ? 1 : 0;
        }

        public bool SIsMember(string key, string value)
        {
            if (IsExpired(key)) return false;
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
            if (IsExpired(key)) return 0;
            if (!Exists(key)) return 0;
            if (GetDataType(key) != DataType.SortedSet)
                throw new InvalidOperationException(WrongTypeError);

            SortedDictionary<double, string>? sortedSet = _redisDatabase[key] as SortedDictionary<double, string>;
            lock (sortedSet!) return sortedSet.Count;
        }

        public string Expire(string key, long seconds)
        {
            if (!Exists(key) || IsExpired(key)) return "0";
            SetExpiry(key, seconds * 1000);
            return "1";
        }

        public string PExpire(string key, long milliseconds)
        {
            if (!Exists(key) || IsExpired(key)) return "0";
            SetExpiry(key, milliseconds);
            return "1";
        }

        public string TTL(string key)
        {
            if (!Exists(key) || IsExpired(key)) return "-2";
            if (!_expiry.TryGetValue(key, out long expiryTime)) return "-1";
            
            long remaining = (expiryTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000;
            return Math.Max(1, remaining).ToString();
        }

        public string PTTL(string key)
        {
            if (!Exists(key) || IsExpired(key)) return "-2";
            if (!_expiry.TryGetValue(key, out long expiryTime)) return "-1";
            
            long remaining = expiryTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Math.Max(1, remaining).ToString();
        }

        public string Persist(string key)
        {
            if (!Exists(key)) return "0";
            if (!_expiry.ContainsKey(key)) return "0";
            
            _expiry.TryRemove(key, out _);
            return "1";
        }

        public void StartExpiryBackgroundTask(CancellationToken cancellationToken = default)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var key in _expiry.Keys)
                    {
                        IsExpired(key);
                    }
                    await Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);
        }
        #endregion
    }
}