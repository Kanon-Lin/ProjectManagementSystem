namespace ProjectManagementSystem.Models.Dtos
{
    public class ProjectDetailsDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OwnerName { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }
}
