using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using MiniaturesGallery.Data;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IAttachmentsService
    {
        Task CreateAsync(List<IFormFile>? Files, int postID, string UserID);
        Task DeleteAsync(int id);
        Task DeleteAllAsync(int postID);
        Task<Attachment> GetAsync(int id);
    }

    public class AttachmentsService : IAttachmentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AttachmentsService(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task CreateAsync(List<IFormFile>? files, int postID, string UserID)
        {
            if (files != null && files.Count > 0)
            {
                foreach (IFormFile f in files)
                {
                    if (f.IsImage())
                    {
                        string FolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", postID.ToString());
                        if (Directory.Exists(FolderPath) == false)
                            Directory.CreateDirectory(FolderPath);
                        string FolderSlashFile = Path.Combine(postID.ToString(), f.FileName);
                        string FilePath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", FolderSlashFile);

                        using (FileStream fs = new FileStream(FilePath, FileMode.Create))
                            f.CopyTo(fs);
                        Attachment att = new Attachment(UserID) { FileName = f.FileName, FullFileName = FolderSlashFile, PostID = postID };
                        _context.Add(att);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            Attachment att = await _context.Attachments
                .Include(a => a.Post)
                .FirstAsync(x => x.ID == id);

            _context.Attachments.Remove(att);

            string FilePath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.FullFileName);
            string FolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.PostID.ToString());

            System.IO.File.Delete(FilePath);
            string[] files = Directory.GetFiles(FolderPath);
            if (files.Length == 0)
                Directory.Delete(FolderPath);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync(int postID)
        {
            var post = await _context.Posts
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(m => m.ID == postID);

            if (post.Attachments != null && post.Attachments.Any())
            {
                foreach (var att in post.Attachments)
                {
                    _context.Attachments.Remove(att);

                    string FilePath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.FullFileName);
                    string FolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.PostID.ToString());

                    System.IO.File.Delete(FilePath);
                    string[] files = Directory.GetFiles(FolderPath);
                    if (files.Length == 0)
                        Directory.Delete(FolderPath);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Attachment> GetAsync(int id)
        {
            Attachment att = await _context.Attachments
                .Include(a => a.Post)
                .FirstAsync(x => x.ID == id);
            
            return att;
        }
    }
}
