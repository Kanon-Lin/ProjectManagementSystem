using ProjectManagementSystem.Models.EFModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace ProjectManagementSystem.Repositories
{
    public interface IMemberRepository
    {
        IEnumerable<TeamMember> GetAllMembers();
        TeamMember GetMemberId(int memberId);
        void CreateMember(TeamMember member);
        void UpdateMember(TeamMember member);
        void DeleteMember(int memberId);

    }
}