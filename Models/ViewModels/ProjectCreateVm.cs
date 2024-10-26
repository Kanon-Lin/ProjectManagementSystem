using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectCreateVm
    {
        [Required(ErrorMessage = "Project Name is required.")]
        [Display(Name = "Project Name")]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Start Date is required.")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Project Manager is required.")]
        [Display(Name = "Project Manager")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Project Manager")]
        public int OwnerId { get; set; }

        // 移除這個屬性，改用 ViewBag 來處理下拉選單
        // public List<SelectListItem> ProjectManagers { get; set; }
    }
}