using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectCreateVm
    {
        [Required(ErrorMessage = "Project Namae is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        public int OwnerId { get; set; }
        public List<SelectListItem> ProjectManagers { get; set; }  // 專案經理選單
    }
}
