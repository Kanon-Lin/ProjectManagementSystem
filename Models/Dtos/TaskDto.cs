namespace ProjectManagementSystem.Models.Dtos
{
    public class TaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string AssignedToName { get; set; }
        public int AssignedToId { get; set; }
        public int ProjectId { get; set; }

    }
}
