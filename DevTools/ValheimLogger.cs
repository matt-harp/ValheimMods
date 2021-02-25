namespace DevTools
{
    public class ValheimLogger : ILogger
    {
        public void Log(object message)
        {
            Console.instance.Print(message.ToString());
        }

        public void Warn(object message)
        {
            Console.instance.Print($"[WARNING] {message}");
        }

        public void Error(object message)
        {
            Console.instance.Print($"[ERROR] {message}");
        }
    }
}