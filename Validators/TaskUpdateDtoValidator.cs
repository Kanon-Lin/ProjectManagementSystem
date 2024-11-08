using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.Dtos;
using ProjectManagementSystem.Models.EFModels;

namespace ProjectManagementSystem.Validators
{
    public class TaskUpdateDtoValidator : AbstractValidator<TaskUpdateDto>
    {
        private static readonly string[] AllowedStatuses = { "待開始", "已完成", "進行中", "未開始" };

        public TaskUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("標題不能為空")
                .MaximumLength(100).WithMessage("標題最多100個字元");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("狀態不能為空")
                .Must(status => AllowedStatuses.Contains(status))
                .WithMessage($"狀態必須為以下值之一: {string.Join("、", AllowedStatuses)}");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("優先級不能為空")
                .Must(priority => new[] { "高", "中", "低" }.Contains(priority))
                .WithMessage("優先級必須為：高、中、低");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("到期日不能為空");


            // AssignedToId 可以為 null，所以不需要 NotEmpty 驗證
            When(x => x.AssignedToId.HasValue, () =>
            {
                RuleFor(x => x.AssignedToId.Value)
                    .GreaterThan(0)
                    .WithMessage("若指派負責人，ID必須大於0");
            });
        }
    }
}