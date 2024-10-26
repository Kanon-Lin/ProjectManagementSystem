using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class TaskCreateVm
    {
        [Required(ErrorMessage = "Task title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Please assign the task to a team member")]
        [Display(Name = "Assigned To")]
        public int AssignedToId { get; set; }

        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; }

        // For dropdowns
        public List<SelectListItem> TeamMembers { get; set; }
        public List<SelectListItem> Statuses { get; set; }
        public List<SelectListItem> Priorities { get; set; }
    }
}
