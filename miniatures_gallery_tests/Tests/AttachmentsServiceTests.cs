using Microsoft.AspNetCore.Http;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using Moq;
using Xunit;

namespace MiniaturesGallery.Tests.Tests
{
    [Collection("TestDatabaseTestsCollection")]
    public class AttachmentsServiceTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        public AttachmentsServiceTests(TestDatabaseFixture fixture)
        {
            Fixture = fixture;
        }
        public TestDatabaseFixture Fixture { get; }
        public void Dispose() => Fixture.Cleanup();

        [Fact]
        public void Get_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var attachmentsService = new AttachmentsService(context, "", null, Fixture.FileSystem);
                var attachemnt = context.Attachments.First();

                Assert.Equal("default1.jpg", attachemnt.FileName);
                Assert.Equal("2\\default1.jpg", attachemnt.FullFileName);
                Assert.Equal(Fixture.TestingUserID_2, attachemnt.UserID);
            }
        }

        [Fact]
        public void Create_Attachemnt_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                Attachment attachmentIn = new Attachment
                {
                    PostID = 2,
                    FileName = "fileName",
                    FullFileName = "fullFileName",
                    UserID = Fixture.TestingUserID_2
                };
                var attachmentsService = new AttachmentsService(context, "", null, Fixture.FileSystem);
                int id = attachmentsService.Create(attachmentIn);

                var attachmentOut = context.Attachments.FirstOrDefault(x => x.ID == id);
                Assert.NotNull(attachmentOut);
            }
        }

        [Fact]
        public void Create_files_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                using (var stream = File.OpenRead("testImage.jpg"))
                {
                    Directory.CreateDirectory("Files/2"); //it is easier to copy real file than mock it for IFormFile
                    var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName("testImage.jpg"))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "image/jpeg"
                    };
                    var attachmentsService = new AttachmentsService(context, "", null, Fixture.FileSystem);
                    attachmentsService.Create(new List<IFormFile> { file }, 2, Fixture.TestingUserID_2);

                    Assert.Equal(3, context.Attachments.Count());
                }
            }
        }

        [Fact]
        public void Delete_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var loggerMock = new LoggerMock<AttachmentsService>();
                var logger = loggerMock.GetServiceLogger();
                var attachmentsService = new AttachmentsService(context, "", logger, Fixture.FileSystem);
                var attachemnt = context.Attachments.First();

                attachmentsService.Delete(attachemnt.ID);

                var attachmentOut = context.Attachments.FirstOrDefault(x => x.ID == attachemnt.ID);
                var logs = loggerMock.GetLogs;
                Assert.Null(attachmentOut);
                Assert.Contains("|INFO|", logs.First());
            }
        }

        [Fact]
        public void DeleteAll_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var loggerMock = new LoggerMock<AttachmentsService>();
                var logger = loggerMock.GetServiceLogger();
                var attachmentsService = new AttachmentsService(context, "", logger, Fixture.FileSystem);
                var post = context.Posts.First();

                attachmentsService.DeleteAll(post.ID);

                var postService = new PostsService(context, null);
                var postOut = (Post)postService.Get(post.ID, Fixture.TestingUserID_1);
                var logs = loggerMock.GetLogs;
                Assert.Empty(postOut.Attachments);
                Assert.Equal(2, logs.Count);
                Assert.Contains("|INFO|", logs.First());
            }
        }
    }
}
