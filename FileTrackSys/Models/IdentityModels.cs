using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FileTracking.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
       // public File File { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<File> Files { get; set; }
        public DbSet<Districts> Districts { get; set; }
        public DbSet<FileType> FileTypes { get; set; }
        public DbSet<FileStatus> FileStatuses { get; set; }
        public DbSet<IdentificationOption> IdentificationOptions { get; set; }
        public DbSet<ManageFileNumber> ManageFileNumbers { get; set; }
        public DbSet<FileVolumes> FileVolumes { get; set; }
        public DbSet<Branches> Branches { get; set; }
        public DbSet<AdUser> AdUsers { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<ReturnState> ReturnStates { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ExternalRequestsBinder> ExternalRequestsBinder { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }


        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}