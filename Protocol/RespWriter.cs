using System.Text;

namespace MyRedis.Protocol
{
    public class RespWriter
    {
        public static string WriteSimpleString(string value)
        {
            return $"+{value}\r\n";
        }

        public static string WriteError(string errorMessage)
        {
            return $"-ERR {errorMessage}\r\n";
        }

        public static string WriteInteger(long value)
        {
            return $":{value}\r\n";
        }

        public static string WriteBulkString(string? value)
        {
            if (value == null)
            {
                return "$-1\r\n";
            }

            return $"${value.Length}\r\n{value}\r\n";
        }

        public static string WriteArray(List<string?>? values)
        {
            if (values == null)
            {
                return "*-1\r\n";
            }

            StringBuilder sb = new();
            sb.Append($"*{values.Count}\r\n");

            foreach (string? value in values)
            {
                sb.Append(WriteBulkString(value));
            }

            return sb.ToString();
        }

        /// <summary>
        /// A generic method to decide which RESP type to use based on the input string.
        /// </summary>
        public static string WriteResponse(object? value)
        {
            if (value == null)
                return WriteBulkString(null);

            if (value is List<string?> list)
                return WriteArray(list);

            if (value is int i)
                return WriteInteger(i);

            if (value is long l)
                return WriteInteger(l);

            if (value is string s)
            {
                if (s == "OK")
                    return WriteSimpleString("OK");

                if (s == "(nil)")
                    return WriteBulkString(null);

                if (s.StartsWith("-ERR"))
                    return $"{s}\r\n";

                if (long.TryParse(s, out long number))
                    return WriteInteger(number);

                return WriteBulkString(s);
            }

            return WriteError("Unknown internal data type");
        }
    }
}