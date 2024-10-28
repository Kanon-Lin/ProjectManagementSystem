using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectTask = ProjectManagementSystem.Models.EFModels.Task;  // 為 Task 類型定義別名
using System.Collections.Generic;
using ProjectManagementSystem.Models.EFModels;  


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
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        // 用於編輯時的下拉選單
        public List<SelectListItem> ProjectManagers { get; set; }
        public List<SelectListItem> StatusOptions { get; set; }

        public TaskCreateVm TaskCreateVm { get; set; }



    }
}
