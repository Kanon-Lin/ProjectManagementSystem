using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.Dtos;


namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectDetailsVm
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        // 任務列表(使用別名並using)
        public List<TaskDto> Tasks { get; set; } = new List<TaskDto>();

        // 用於編輯時的下拉選單
        public List<SelectListItem> ProjectManagers { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } 

        public TaskCreateVm TaskCreateVm { get; set; }



    }
}
