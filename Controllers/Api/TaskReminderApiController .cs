using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services;
using System.Text.Json;

namespace ProjectManagementSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskReminderApiController : ControllerBase
    {
        private readonly ITaskReminderService _reminderService;
        private readonly IEmailService _emailService;
        private readonly ILogger<TaskReminderApiController> _logger;

        public TaskReminderApiController(
            ITaskReminderService reminderService,
        IEmailService emailService,
        ILogger<TaskReminderApiController> logger)
        {
            _reminderService = reminderService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] string email)
        {
            try
            {
                _logger.LogInformation($"嘗試發送測試郵件到: {email}");

                var smtpSettings = HttpContext.RequestServices
                    .GetRequiredService<IOptions<SmtpSettings>>()
                    .Value;

                _logger.LogInformation($"SMTP設定: {JsonSerializer.Serialize(smtpSettings)}");

                await _emailService.SendEmailAsync(
                    email,
                    "測試郵件",
                    "這是一封測試郵件，用於確認郵件服務正常運作。"
                );
                return Ok(new { message = "測試郵件發送成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試郵件發送失敗，詳細錯誤: {Error}", ex.ToString());
                return StatusCode(500, new
                {
                    error = "郵件發送失敗",
                    message = ex.Message,
                    details = ex.ToString()
                });
            }
        }

        [HttpPost("check-reminders")]
        public async Task<IActionResult> CheckReminders()
        {
            try
            {
                await _reminderService.CheckAndSendReminders();
                return Ok(new { message = "提醒檢查完成" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提醒檢查失敗");
                return StatusCode(500, new { error = "提醒檢查失敗", message = ex.Message });
            }
        }

        [HttpGet("tasks/upcoming")]
        public async Task<IActionResult> GetUpcomingTasks()
        {
            try
            {
                var tasks = await _reminderService.GetUpcomingTasks();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取即將到期任務失敗");
                return StatusCode(500, new { error = "獲取任務失敗", message = ex.Message });
            }
        }

        [HttpPost("tasks/{taskId}/remind")]
        public async Task<IActionResult> SendTaskReminder(int taskId)
        {
            try
            {
                await _reminderService.SendTaskReminder(taskId);
                return Ok(new { message = "任務提醒已發送" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"發送任務 {taskId} 提醒失敗");
                return StatusCode(500, new { error = "提醒發送失敗", message = ex.Message });
            }
        }
    }
}
