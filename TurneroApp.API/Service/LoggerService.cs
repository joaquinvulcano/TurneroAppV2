
namespace TurneroApp.API.Service
{
    using Microsoft.Extensions.Configuration;
    using Serilog;

    public class LoggerService
    {
        private readonly ILogger logger;

        public LoggerService(IConfiguration configuration)
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(configuration["FileLoggerSettings:LogFilePath"],
                              outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        public void LogInformation(string message)
        {
            logger.Information(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
        }

    }
}
