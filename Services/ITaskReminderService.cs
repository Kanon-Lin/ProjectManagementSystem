using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Services
{
    public interface ITaskReminderService
    {
        Task CheckAndSendReminders();
        Task SendTaskReminder(int taskId);
        Task<List<TaskDetailsVm>> GetUpcomingTasks();

    }
}
