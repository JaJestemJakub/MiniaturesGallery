using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IAttachmentsService
    {
        void Create(List<IFormFile>? Files, int postID, string UserID);
        void Create(Attachment att);
        void Delete(int id);
        void DeleteAll(int postID);
        Attachment Get(int id);
    }

    public class AttachmentsService : IAttachmentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AttachmentsService> _logger;

        public AttachmentsService(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<AttachmentsService> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public void Create(List<IFormFile>? files, int postID, string UserID)
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
                _context.SaveChanges();
            }
        }

        public void Create(Attachment att)
        {
            _context.Add(att);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            Attachment att = _context.Attachments
                .First(x => x.ID == id);

            _logger.LogInformation($"Attachment ID: {id} FileName: {att.FileName} PostID: {att.PostID} Of: {att.UserID} DELETE invoked");

            _context.Attachments.Remove(att);

            string FilePath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.FullFileName);
            string FolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.PostID.ToString());

            DeleteFileIfExistsThenDeleteFolderIfEmpty(FilePath, FolderPath);

            _context.SaveChanges();
        }

        public void DeleteAll(int postID)
        {
            var post = _context.PostsAbs.OfType<Post>()
                .Include(a => a.Attachments)
                .FirstOrDefault(m => m.ID == postID);

            if (post.Attachments != null && post.Attachments.Any())
            {
                foreach (var att in post.Attachments)
                {
                    _logger.LogInformation($"Attachment ID: {att.ID} FileName: {att.FileName} PostID: {att.PostID} Of: {att.UserID} DELETE invoked");

                    _context.Attachments.Remove(att);

                    string FilePath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.FullFileName);
                    string FolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "Files", att.PostID.ToString());

                    DeleteFileIfExistsThenDeleteFolderIfEmpty(FilePath, FolderPath);
                }
            }

            _context.SaveChanges();
        }

        public Attachment Get(int id)
        {
            Attachment att = _context.Attachments
                .First(x => x.ID == id);

            return att;
        }

        private static void DeleteFileIfExistsThenDeleteFolderIfEmpty(string FilePath, string FolderPath)
        {
            if (Directory.Exists(FolderPath))
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                string[] files = Directory.GetFiles(FolderPath);
                if (files.Length == 0)
                    Directory.Delete(FolderPath);
            }
        }
    }
}
