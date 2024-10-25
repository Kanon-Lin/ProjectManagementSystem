using ProjectManagementSystem.Models.EFModels;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectListVm
    {
		public IEnumerable<Project> Projects { get; set; } = new List<Project>();
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
        public int OwnerId { get; set; }
    }
}