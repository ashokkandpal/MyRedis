namespace MyRedis.Protocol
{
    public class RespParser(StreamReader reader)
    {
        private readonly StreamReader _reader = reader;

        public List<string>? Parse()
        {
            string? firstLine = _reader.ReadLine();

            if (string.IsNullOrEmpty(firstLine)) 
                return null;

            if (firstLine.StartsWith("*"))
            {
                int elementCount = int.Parse(firstLine.Substring(1));

                if (elementCount == -1) 
                    return null;

                List<string> result = new(elementCount);

                for (int i = 0; i < elementCount; i++)
                {
                    string? lengthLine = _reader.ReadLine(); 
                    if (lengthLine == null || !lengthLine.StartsWith("$")) return null;

                    int length = int.Parse(lengthLine.Substring(1));

                    if (length == -1)
                    {
                        result.Add(null!); 
                        continue;
                    }

                    char[] buffer = new char[length];
                    _reader.ReadBlock(buffer, 0, length);
                    result.Add(new string(buffer));

                    _reader.ReadLine();
                }

                return result;
            }

            return [.. firstLine.Split(' ').Select(s => s.Trim())];
        }
    }
}
