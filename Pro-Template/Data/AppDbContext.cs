using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pro_Template.Models;

namespace Pro_Template.Data
{
    public class AppDbContext: IdentityDbContext<CustomUser, CustomRole, Guid> 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
