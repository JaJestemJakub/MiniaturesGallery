using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using System.IO.Abstractions;

namespace MiniaturesGallery.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IFileSystem fileSystem, string adminUserPw)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Password is set with the following:
                // dotnet user-secrets set SeedAdminUserPW <pw>
                // The admin user can do anything

                var adminID = await EnsureUser(serviceProvider, adminUserPw, "admin@admin.com");
                await EnsureRole(serviceProvider, adminID, HelpClasses.Constants.ContactAdministratorsRole);
                SeedDB(serviceProvider, fileSystem, adminID);
            }
        }

        private static void SeedDB(IServiceProvider serviceProvider, IFileSystem fileSystem, string uid)
        {
            var postsService = serviceProvider.GetService<IPostService>();
            var commentsService = serviceProvider.GetService<ICommentsService>();
            var ratesService = serviceProvider.GetService<IRatesService>();
            var attachmentsService = serviceProvider.GetService<IAttachmentsService>();
            var hostingEnvironment = serviceProvider.GetService<IWebHostEnvironment>();

            if (postsService.Any() == false)
            {
                Announcement exampleAnnouncement = new Announcement
                {
                    Topic = "First Announcement",
                    Text = "Example Announcement from admin account.",
                    PrivateNote = "Hidden note from admin account."
                };

                int announcementID = postsService.Create(exampleAnnouncement, uid);

                Post examplePost = new Post
                {
                    Topic = "First Post",
                    Text = "Example Post from admin account."
                };

                int postID = postsService.Create(examplePost, uid);

                Comment comment = new Comment
                {
                    Body = "Body of first Comment.",
                    PostID = postID,
                    UserID = uid
                };
                int commentID = commentsService.Create(comment);

                Comment comment2 = new Comment
                {
                    Body = "Body of second Comment in response to first.",
                    PostID = postID,
                    UserID = uid,
                    CommentID = commentID
                };
                commentsService.Create(comment2);

                Rate rate = new Rate
                {
                    Rating = 5,
                    PostID = postID,
                    UserID = uid
                };
                ratesService.Create(rate);

                string fileName1 = "default1.jpg";
                string fileName2 = "default2.jpg";

                string FolderPath = fileSystem.Path.Combine(hostingEnvironment.WebRootPath, "Files", postID.ToString());
                if (fileSystem.Directory.Exists(FolderPath) == false)
                    fileSystem.Directory.CreateDirectory(FolderPath);
                string FolderSlashFile1 = fileSystem.Path.Combine(postID.ToString(), fileName1);
                string FilePath1 = fileSystem.Path.Combine(hostingEnvironment.WebRootPath, "Files", FolderSlashFile1);
                string FolderSlashFile2 = fileSystem.Path.Combine(postID.ToString(), fileName2);
                string FilePath2 = fileSystem.Path.Combine(hostingEnvironment.WebRootPath, "Files", FolderSlashFile2);

                fileSystem.File.Copy(fileSystem.Path.Combine(hostingEnvironment.WebRootPath, fileName1), FilePath1);
                fileSystem.File.Copy(fileSystem.Path.Combine(hostingEnvironment.WebRootPath, fileName2), FilePath2);

                Attachment attachment1 = new Attachment
                {
                    PostID = postID,
                    FileName = fileName1,
                    FullFileName = FolderSlashFile1,
                    UserID = uid
                };
                Attachment attachment2 = new Attachment
                {
                    PostID = postID,
                    FileName = fileName2,
                    FullFileName = FolderSlashFile2,
                    UserID = uid
                };

                attachmentsService.Create(attachment1);
                attachmentsService.Create(attachment2);
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
