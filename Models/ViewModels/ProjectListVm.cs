using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementSystem.Models.EFModels;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class ProjectListVm
    {
		public IEnumerable<Project> Projects { get; set; } = new List<Project>();
        
        // 分頁用
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
        public int OwnerId { get; set; }

        //搜尋用
        public string SearchTerm { get; set; }
        //篩選
        public DateTime? StartDateFilter { get; set; }
        public DateTime? EndDateFilter { get; set; }
        public string StatusFilter {  get; set; }
        public int? ManagerFilter { get; set; }

        //排序
        public string SortOrder { get; set; }

        //下拉選單
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem> ();
        public List<SelectListItem> ProjectManagers { get; set; } = new List<SelectListItem> ();

    }
}