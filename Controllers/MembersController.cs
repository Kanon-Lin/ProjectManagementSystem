using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Models.ViewModels;
using ProjectManagementSystem.Repositories;

namespace ProjectManagementSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly IMemberRepository _memberRepository;

        public MembersController(IMemberRepository memberRepository) 
        {
            _memberRepository = memberRepository;
        }
        // GET: Members
        public IActionResult Index()
        {
            var members = _memberRepository.GetAllMembers()
                .Select(m => new MemberVm
                {
                    MemberId = m.MemberId,
                    Name = m.Name,
                    Position = m.Position,
                    Email = m.Email,
                    Phone = m.Phone,
                    CreatedAt = m.CreatedAt
                });

            return View(members);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        public IActionResult Create(MemberVm vm)
        {
            if (ModelState.IsValid)
            {
                var member = new TeamMember
                {
                    Name = vm.Name,
                    Position = vm.Position,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    CreatedAt = DateTime.Now
                };
                _memberRepository.CreateMember(member);
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Members/Edit/5
        public IActionResult Edit(int id)
        {
            var member = _memberRepository.GetMemberId(id);
            if (member == null)
            {
                return NotFound();
            }
            var vm = new MemberVm
            {
                MemberId = member.MemberId,
                Name = member.Name,
                Position = member.Position,
                Email = member.Email,
                Phone = member.Phone,
                CreatedAt = member.CreatedAt
            };
            return View(vm);
        }

        // POST: Members/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MemberVm model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var member = new TeamMember
                    {
                        MemberId = model.MemberId,
                        Name = model.Name,
                        Position = model.Position,
                        Email = model.Email,
                        Phone = model.Phone
                    };

                    _memberRepository.UpdateMember(member);

                    // 成功後導回列表頁
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "更新失敗: " + ex.Message);
                }
            }

            // 如果更新失敗，返回到 Modal
            return PartialView("_EditMemberModal", model);
        }

        // GET: Members/Delete/5
        [HttpPost,ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _memberRepository.DeleteMember(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
        
    }
}
