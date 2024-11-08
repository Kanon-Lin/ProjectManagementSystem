using FluentValidation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.Dtos;
using ProjectManagementSystem.Models.EFModels;

namespace ProjectManagementSystem.Validators
{
    public class TaskCreateDtoValidator : AbstractValidator<TaskCreateDto>
    {
        private readonly AppDbContext _context;
        private static readonly string[] AllowedStatuses = { "待開始", "已完成", "進行中", "未開始" };

        public TaskCreateDtoValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(dto => dto.Title)
                .NotEmpty().WithMessage("標題為必填")
                .MaximumLength(100).WithMessage("標題不能超過100個字元")
                .MustAsync(async (dto, title, cancellation) =>
                {
                    return !await _context.Tasks
                        .AnyAsync(t => t.Title == title && t.ProjectId == dto.ProjectId);
                }).WithMessage("此專案中已存在相同標題的任務");

            RuleFor(x => x.Status)
            .NotEmpty().WithMessage("狀態不能為空")
            .Must(status => AllowedStatuses.Contains(status))
            .WithMessage($"狀態必須為以下值之一: {string.Join("、", AllowedStatuses)}");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("到期日為必填")
                .Must(date => date.Date >= DateTime.Today)
                .WithMessage("到期日不能早於今天");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("優先級為必填")
                .Must(priority => new[] { "高", "中", "低" }
                    .Contains(priority))
                .WithMessage("無效的優先級值");

            When(x => x.AssignedToId != null, () =>
            {
                RuleFor(x => x.AssignedToId)
                    .MustAsync(async (id, cancellation) =>
                    {
                        if (id == null) return true;
                        return await context.TeamMembers
                            .AnyAsync(m => m.MemberId == id);
                    })
                    .WithMessage("指定的團隊成員不存在");
            });
        }
    }
}