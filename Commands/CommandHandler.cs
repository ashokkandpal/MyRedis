using MyRedis.Core;

namespace MyRedis.Commands
{
    public class CommandHandler
    {
        private readonly RedisStore _store;
        private readonly Dictionary<string, ICommand> _commands;

        public CommandHandler(RedisStore store)
        {
            _store = store;
            _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "SET", new SetCommand(store) },
                { "GET", new GetCommand(store) },
                { "DEL", new DelCommand(store) } 
            };
        }

        public string Handle(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "-ERR empty command";

            string[] inputParts = input.Split(' ');

            if (_commands.TryGetValue(inputParts[0], out ICommand? command))
            {
                string[] args = inputParts.Skip(1)
                    .Select(a => a.Trim('"'))
                    .ToArray();

                return command.Execute(args);
            }
               
            return $"-ERR unknown command '{inputParts[0]}'";
        }
    }
}
