using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Models;
using System.IO.Abstractions;

namespace MiniaturesGallery.Tests
{
    public static class SeedTestData
    {
        public static void SeedDBTesting(ApplicationDbContext dbContext, string user1, string user2, IFileSystem fileSystem, string rootPath)
        {
            Announcement exampleAnnouncement = new Announcement
            {
                ID = 1,
                Topic = "First Announcement",
                Text = "Example Announcement from admin account.",
                PrivateNote = "Hidden note from admin account."
            };

            exampleAnnouncement.CrateDate = DateTime.Now;
            exampleAnnouncement.UserID = user1;
            dbContext.Add(exampleAnnouncement);

            Post examplePost = new Post
            {
                ID = 2,
                Topic = "First Post",
                Text = "Example Post from admin account."
            };

            examplePost.CrateDate = DateTime.Now;
            examplePost.UserID = user2;
            dbContext.Add(examplePost);
            dbContext.SaveChanges();
            int postID = examplePost.ID;

            Comment comment = new Comment
            {
                ID = 1,
                Body = "Body of first Comment.",
                PostID = postID,
                UserID = user1
            };
            comment.CrateDate = DateTime.Now;
            dbContext.Add(comment);
            dbContext.SaveChanges();
            int commentID = comment.ID;

            Comment comment2 = new Comment
            {
                ID = 2,
                Body = "Body of second Comment in response to first.",
                PostID = postID,
                UserID = user2,
                CommentID = commentID
            };
            comment2.CrateDate = DateTime.Now;
            dbContext.Add(comment2);
            dbContext.SaveChanges();

            Rate rate = new Rate
            {
                ID = 1,
                Rating = 5,
                PostID = postID,
                UserID = user1
            };
            dbContext.Add(rate);
            dbContext.SaveChanges();

            Post post = dbContext.PostsAbs.OfType<Post>()
                .Include(a => a.Rates)
                .First(x => x.ID == rate.PostID);
            post.Rating = rate.Rating / 1;
            dbContext.Update(post);
            dbContext.SaveChanges();

            string fileName1 = "default1.jpg";
            string fileName2 = "default2.jpg";

            string FolderPath = fileSystem.Path.Combine(rootPath, "Files", postID.ToString());
            if (fileSystem.Directory.Exists(FolderPath) == false)
                fileSystem.Directory.CreateDirectory(FolderPath);
            string FolderSlashFile1 = fileSystem.Path.Combine(postID.ToString(), fileName1);
            string FilePath1 = fileSystem.Path.Combine(rootPath, "Files", FolderSlashFile1);
            string FolderSlashFile2 = fileSystem.Path.Combine(postID.ToString(), fileName2);
            string FilePath2 = fileSystem.Path.Combine(rootPath, "Files", FolderSlashFile2);

            fileSystem.File.Copy(fileSystem.Path.Combine(rootPath, fileName1), FilePath1);
            fileSystem.File.Copy(fileSystem.Path.Combine(rootPath, fileName2), FilePath2);

            Attachment attachment1 = new Attachment
            {
                ID = 1,
                PostID = postID,
                FileName = fileName1,
                FullFileName = FolderSlashFile1,
                UserID = user2
            };
            Attachment attachment2 = new Attachment
            {
                ID = 2,
                PostID = postID,
                FileName = fileName2,
                FullFileName = FolderSlashFile2,
                UserID = user2
            };

            dbContext.Add(attachment1);
            dbContext.Add(attachment2);
            dbContext.SaveChanges();
        }

    }
}
