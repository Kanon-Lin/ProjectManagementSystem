using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class TaskListFilterVm
    {
        public List<TaskDetailsVm> Tasks { get; set; }
        public string DeadlineStatus { get; set; }  // "all", "upcoming", "overdue"
        public string SortBy { get; set; }          // "dueDate", "priority"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // 選項列表
        public List<SelectListItem> DeadlineStatusOptions => new()
    {
        new SelectListItem("所有任務", "all"),
        new SelectListItem("即將到期(3天內)", "upcoming"),
        new SelectListItem("已逾期", "overdue")
    };

        public List<SelectListItem> SortByOptions => new()
    {
        new SelectListItem("依到期日", "dueDate"),
        new SelectListItem("依優先度", "priority")
    };
    }
}
