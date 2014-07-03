using log4net;
using Microsoft.Practices.Prism.Logging;

namespace CookSharp.Prism
{
    public class LoggingFacade : ILoggerFacade
    {
        private static readonly ILog Logger = LogManager.GetLogger("Prism");

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    Logger.Debug(message);
                    break;
                case Category.Exception:
                    Logger.Error(message);
                    break;
                case Category.Info:
                    Logger.Info(message);
                    break;
                case Category.Warn:
                    Logger.Warn(message);
                    break;
            }
        }
    }
}