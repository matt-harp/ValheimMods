namespace DevTools
{
    public interface ILogger
    {
        void Log(object message);
        
        void Warn(object message);
        
        void Error(object message);
    }
}