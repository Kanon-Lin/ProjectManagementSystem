using ProjectManagementSystem.Models.EFModels;

namespace ProjectManagementSystem.Models.ViewModels
{
    public class FileVm
    {
        //列表
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedByName { get; set; }
        public DateTime? UploadedAt { get; set; }

        //上傳
        public int TaskId { get; set; }
        public IFormFile UploadFile { get; set; }

        //顯示檔案列表
        public List<FileVm> Files { get; set; }
    }
}
