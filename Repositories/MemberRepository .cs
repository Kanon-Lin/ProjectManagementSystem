using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly AppDbContext _context;
        public MemberRepository(AppDbContext context) {
            _context = context;
        }

        public void CreateMember(TeamMember member)
        {
            try {
                if (member == null) throw new ArgumentNullException(nameof(member));
                member.CreatedAt = DateTime.Now;
                _context.TeamMembers.Add(member);
                _context.SaveChanges();
            }
            catch (Exception ex) {
                throw new Exception("新增成員失敗", ex);
            }
            
        }

        public void DeleteMember(int memberId)
        {
            var member = _context.TeamMembers.Find(memberId);
            if (member == null)
                throw new KeyNotFoundException($"找不到ID為 {memberId} 的成員");

            _context.TeamMembers.Remove(member);
            _context.SaveChanges();
        }

        public IEnumerable<TeamMember> GetAllMembers()
        {
           return _context.TeamMembers
                .AsNoTracking()
                .OrderBy(x => x.MemberId)
                .ToList();
        }
        public async Task<IEnumerable<TeamMember>> GetAllMembersAsync()
        {
            return await _context.TeamMembers
                .AsNoTracking()
                .OrderBy(x => x.MemberId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeamMember>> GetMembersAsync(int page, int pageSize)
        {
            return await _context.TeamMembers
                .AsNoTracking()
                .OrderBy(x => x.MemberId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public TeamMember GetMemberId(int memberId)
        {
            return _context.TeamMembers.Find(memberId);
        }

        public void UpdateMember(TeamMember member)
        {
            if (member == null)throw new ArgumentNullException(nameof(member));

            var existingMember = _context.TeamMembers.Find(member.MemberId);
            if (existingMember == null) throw new KeyNotFoundException($"找不到{member.MemberId}的成員");

            existingMember.Name = member.Name;
            existingMember.Position = member.Position;
            existingMember.Email = member.Email;
            existingMember.Phone = member.Phone;

            _context.SaveChanges();
        }
    }

}