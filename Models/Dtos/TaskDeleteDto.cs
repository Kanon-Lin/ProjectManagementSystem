namespace ProjectManagementSystem.Models.Dtos
{
    public class TaskDeleteDto
    {
        public int TaskId { get; set; }
        public bool Success { get; set; }
        public string Title { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
