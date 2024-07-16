using FileTypeChecker;
using FileTypeChecker.Extensions;
using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Models;
using System.IO.Abstractions;

namespace MiniaturesGallery.Services
{
    public interface IAttachmentsService
    {
        void Create(List<IFormFile>? Files, int postID, string UserID);
        int Create(Attachment att);
        void Delete(int id);
        void DeleteAll(int postID);
        Attachment Get(int id);
    }

    public class AttachmentsService : IAttachmentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _rootPath;
        private readonly ILogger<AttachmentsService> _logger;
        private readonly IFileSystem _fileSystem;

        public AttachmentsService(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<AttachmentsService> logger, IFileSystem fileSystem)
        {
            _rootPath = hostingEnvironment.WebRootPath;
            _context = context;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public AttachmentsService(ApplicationDbContext context, string rootPath, ILogger<AttachmentsService> logger, IFileSystem fileSystem)
        {
            _rootPath = rootPath;
            _context = context;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public void Create(List<IFormFile>? files, int postID, string UserID)
        {
            if (files != null && files.Count > 0)
            {
                foreach (IFormFile f in files)
                {
                    
                    Stream fileStream = f.OpenReadStream();
                    bool isRecognizableType = FileTypeValidator.IsTypeRecognizable(fileStream);
                    if (isRecognizableType && fileStream.IsImage())
                    {
                        string FolderPath = _fileSystem.Path.Combine(_rootPath, "Files", postID.ToString());
                        if (_fileSystem.Directory.Exists(FolderPath) == false)
                            _fileSystem.Directory.CreateDirectory(FolderPath);
                        string FolderSlashFile = _fileSystem.Path.Combine(postID.ToString(), f.FileName);
                        string FilePath = _fileSystem.Path.Combine(_rootPath, "Files", FolderSlashFile);

                        using (FileStream fs = new FileStream(FilePath, FileMode.Create))
                            f.CopyTo(fs);
                        Attachment att = new Attachment(UserID) { FileName = f.FileName, FullFileName = FolderSlashFile, PostID = postID };
                        _context.Add(att);
                    }
                    else
                    {
                        //TODO: what if file is not image
                    }
                }
                _context.SaveChanges();
            }
        }

        public int Create(Attachment att)
        {
            _context.Add(att);
            _context.SaveChanges();

            return att.ID;
        }

        public void Delete(int id)
        {
            Attachment att = _context.Attachments
                .First(x => x.ID == id);

            _logger.LogInformation($"Attachment ID: {id} FileName: {att.FileName} PostID: {att.PostID} Of: {att.UserID} DELETE invoked");

            _context.Attachments.Remove(att);

            string FilePath = _fileSystem.Path.Combine(_rootPath, "Files", att.FullFileName);
            string FolderPath = _fileSystem.Path.Combine(_rootPath, "Files", att.PostID.ToString());

            DeleteFileIfExistsThenDeleteFolderIfEmpty(FilePath, FolderPath);

            _context.SaveChanges();
        }

        public void DeleteAll(int postID)
        {
            var postabs = _context.PostsAbs
                .Include(a => (a as Post).Attachments)
                .FirstOrDefault(m => m.ID == postID);

            if(postabs is Post)
            {
                Post post = postabs as Post;
                if (post.Attachments != null && post.Attachments.Any())
                {
                    foreach (var att in post.Attachments)
                    {
                        _logger.LogInformation($"Attachment ID: {att.ID} FileName: {att.FileName} PostID: {att.PostID} Of: {att.UserID} DELETE invoked");

                        _context.Attachments.Remove(att);

                        string FilePath = _fileSystem.Path.Combine(_rootPath, "Files", att.FullFileName);
                        string FolderPath = _fileSystem.Path.Combine(_rootPath, "Files", att.PostID.ToString());

                        DeleteFileIfExistsThenDeleteFolderIfEmpty(FilePath, FolderPath);
                    }
                    _context.SaveChanges();
                }           
            }
        }

        public Attachment Get(int id)
        {
            Attachment att = _context.Attachments
                .First(x => x.ID == id);

            return att;
        }

        private void DeleteFileIfExistsThenDeleteFolderIfEmpty(string FilePath, string FolderPath)
        {
            if (_fileSystem.Directory.Exists(FolderPath))
            {
                if (_fileSystem.File.Exists(FilePath))
                    _fileSystem.File.Delete(FilePath);
                string[] files = _fileSystem.Directory.GetFiles(FolderPath);
                if (files.Length == 0)
                    _fileSystem.Directory.Delete(FolderPath);
            }
        }
    }
}
