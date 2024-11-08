using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class MemberVm
    {
        public int MemberId { get; set; }

        [Required(ErrorMessage ="請輸入姓名")]
        [MaxLength(20,ErrorMessage ="姓名不超過20字")]
        [Display(Name ="姓名")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請輸入職位")]
        [MaxLength(20, ErrorMessage = "職位不超過20字")]
        [Display(Name = "職位")]
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
