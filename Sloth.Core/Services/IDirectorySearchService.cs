using System.Threading.Tasks;

namespace Sloth.Core.Services
{
    public interface IDirectorySearchService
    {
        Task<DirectoryUser> GetByKerberos(string kerb);
        Task<DirectoryUser> GetByEmail(string email);
    }

    public class DirectoryUser
    {
        public string Email { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Kerberos { get; set; }
        public string FullName { get; set; }
    }
}
