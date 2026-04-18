using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sms.api.Interfaces;

namespace sms.api.Services
{
    public class PasswordHashingService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PasswordHashingService> _logger;

        public PasswordHashingService(IServiceProvider serviceProvider, ILogger<PasswordHashingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Password Hashing Service...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

                var hashedCount = await userManager.HashPlainTextPasswordsAsync();
                
                if (hashedCount > 0)
                {
                    _logger.LogWarning($"Found and hashed {hashedCount} plain text passwords for security.");
                }
                else
                {
                    _logger.LogInformation("All passwords are already properly hashed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while hashing plain text passwords");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Password Hashing Service stopped.");
            return Task.CompletedTask;
        }
    }
}
