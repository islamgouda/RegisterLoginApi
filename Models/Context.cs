using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RegisterLoginApi.Models
{
    public class Context:IdentityDbContext<ApplicationUser>
    {
        public Context(){ }

        public Context(DbContextOptions options):base(options)
        {

        }
        
    }
}
