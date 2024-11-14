using ProjectManagementSystem.Services;

namespace ProjectManagementSystem.BackgroundServices
{
    public class ReminderHostedService : BackgroundService
    {
        private readonly IServiceProvider _service;
        private readonly ILogger<ReminderHostedService> _logger;

        public ReminderHostedService(IServiceProvider service, ILogger<ReminderHostedService> logger)
        {
            _service = service;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _service.CreateScope();
                    var reminderService = scope.ServiceProvider.GetRequiredService<ITaskReminderService>();
                    await reminderService.CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking tasks");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

    }
}
