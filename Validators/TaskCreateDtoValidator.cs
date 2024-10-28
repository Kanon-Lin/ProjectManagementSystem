using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.Dtos;
using ProjectManagementSystem.Models.EFModels;

namespace ProjectManagementSystem.Validators
{
    public class TaskCreateDtoValidator : AbstractValidator<TaskCreateDto>
    {
        private readonly AppDbContext _context;

        public TaskCreateDtoValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("標題為必填")
                .MaximumLength(100).WithMessage("標題不能超過100個字元")
                .MustAsync(async (title, cancellation) =>
                {
                    return !await context.Tasks.AnyAsync(t => t.Title == title);
                }).WithMessage("標題已存在");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("狀態為必填")
                .Must(status => new[] { "Not Started", "In Progress", "Completed" }
                    .Contains(status))
                .WithMessage("無效的狀態值");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("到期日為必填")
                .Must(date => date.Date >= DateTime.Today)
                .WithMessage("到期日不能早於今天");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("優先級為必填")
                .Must(priority => new[] { "High", "Medium", "Low" }
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