namespace ProjectManagementSystem.Models.ViewModels
{
    public class TaskDetailsVm
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int AssignedToId { get; set; }
        public string AssignedToName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
    }
}
