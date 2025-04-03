//using ProjectManagementSystem.Services;

//namespace ProjectManagementSystem.BackgroundServices
//{
//    public class ReminderHostedService : BackgroundService
//    {
//        private readonly IServiceProvider _service;
//        private readonly ILogger<ReminderHostedService> _logger;

//        public ReminderHostedService(IServiceProvider service, ILogger<ReminderHostedService> logger)
//        {
//            _service = service;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    var now = DateTime.Now;
//                    var nextRun = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0);

//                    if (now > nextRun)
//                    {
//                        nextRun = nextRun.AddDays(1);
//                    }

//                    var delay = nextRun - now;
//                    _logger.LogInformation($"下次提醒檢查排程時間: {nextRun:yyyy-MM-dd HH:mm:ss}");
//                    _logger.LogInformation($"等待時間:{delay.Hours}小時 {delay.Minutes}分鐘");

//                    using (var scope = _service.CreateScope())
//                    {

//                        var reminderService = scope.ServiceProvider.GetRequiredService<ITaskReminderService>();

//                        //執行題型檢查和發信
//                        await reminderService.CheckAndSendReminders();
//                        _logger.LogInformation("提醒檢查完成");
//                    }

//                    //等24小時候執行下一次
//                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "執行提醒檢查時發生錯誤");

//                    //等待1小時後重試
//                    _logger.LogInformation("1小時候重試");
//                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
//                }

//                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
//            }
//        }

//    }
//}
