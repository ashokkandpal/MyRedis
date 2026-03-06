using MyRedis.Commands;
using MyRedis.Core;

Console.WriteLine("This is my own redis, Phase 1 - for learning purposes only. It is not intended for production use.");


RedisStore store = new RedisStore();
CommandHandler handler = new CommandHandler(store);

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();

    if (string.IsNullOrEmpty(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    string response = handler.Handle(input);
    Console.WriteLine(response);
}