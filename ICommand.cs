namespace MyRedis.Commands
{
    public interface ICommand
    {
        public string Execute(string[] args);
    }
}
