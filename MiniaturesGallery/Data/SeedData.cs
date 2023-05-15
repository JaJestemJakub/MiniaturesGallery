using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MiniaturesGallery.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string adminUserPw)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Password is set with the following:
                // dotnet user-secrets set SeedAdminUserPW <pw>
                // The admin user can do anything

                var adminID = await EnsureUser(serviceProvider, adminUserPw, "admin@admin.com");
                await EnsureRole(serviceProvider, adminID, HelpClasses.Constants.ContactAdministratorsRole);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, string userPw, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = UserName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, userPw);
            }

            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            //if (userManager == null)
            //{
            //    throw new Exception("userManager is null");
            //}

            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
            {
                throw new Exception("The userPw password was probably not strong enough!");
            }

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }
    }
}
