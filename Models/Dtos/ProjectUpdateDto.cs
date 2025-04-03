namespace ProjectManagementSystem.Models.Dtos
{
    public class ProjectUpdateDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int OwnerId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
