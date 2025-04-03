using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectManagerVm
    {
        public int ManagerId { get; set; }

        [Required(ErrorMessage = "姓名是必填欄位")]
        [StringLength(50, ErrorMessage = "姓名不能超過50個字元")]
        public string Name { get; set; }

        public int ActiveProjectsCount { get; set; }
    }
}
