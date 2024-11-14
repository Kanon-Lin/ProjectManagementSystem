namespace ProjectManagementSystem.Models.ViewModels
{
    public class NotificationVm
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public TaskDetailsVm Task { get; set; }
    }
}
