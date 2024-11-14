using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace ProjectManagementSystem.Services
{
    public class TaskReminderService : ITaskReminderService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<TaskReminderService> _logger;

        public TaskReminderService(AppDbContext context,
            IEmailService emailService, ILogger<TaskReminderService> logger)
        {
               _context = context;
            _emailService = emailService;
            _logger = logger;
        }
        public async Task CheckAndSendReminders()
        {
            var tasks = _context.Tasks
                .Include(t => t.AssignedTo)
                .Where(t => t.Status != "已完成" &&
                t.DueDate.Date <= DateTime.Now.AddDays(3))
                .ToList();
            foreach (var task in tasks)
            {
                await SendTaskReminder(task.TaskId);
            }
    }

        public async Task SendTaskReminder(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task?.AssignedTo?.Email == null) return;

            var daysUntilDue = (task.DueDate.Date - DateTime.Now.Date).Days;
            var isOverdue = daysUntilDue < 0;

            var notification = new Notification
            {
                MemberId = task.AssignedToId.Value,
                TaskId = task.TaskId,
                Message = isOverdue
                    ? $"任務「{task.Title}」已逾期 {Math.Abs(daysUntilDue)} 天"
                    : $"任務「{task.Title}」將於 {daysUntilDue} 天後到期",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                task.AssignedTo.Email,
                isOverdue ? "任務逾期提醒" : "任務到期提醒",
                notification.Message
                );
        }

        public async Task<List<TaskDetailsVm>> GetUpcomingTasks()
        {
            var threeDaysFromNow = DateTime.Now.AddDays(3);
            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Where(t =>
                    t.Status != "已完成" &&
                    t.DueDate <= threeDaysFromNow)
                .Select(t => new TaskDetailsVm
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    AssignedToName = t.AssignedTo.Name
                })
                .ToListAsync();
        }
    }
}
