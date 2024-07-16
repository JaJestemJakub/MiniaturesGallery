using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using Xunit;

namespace MiniaturesGallery.Tests.Tests
{
    [Collection("TestDatabaseTestsCollection")]
    public class CommentsServiceTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        public CommentsServiceTests(TestDatabaseFixture fixture)
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
                var commentService = new CommentsService(context, null);
                var comment = commentService.Get(1);

                Assert.NotNull(comment);
                Assert.Equal("Body of first Comment.", comment.Body);
            }
        }

        [Fact]
        public void Create_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                Comment commentIn = new Comment
                {
                    Body = "Body of new Comment.",
                    PostID = 2,
                    UserID = Fixture.TestingUserID_1
                };
                var commentService = new CommentsService(context, null);
                int id = commentService.Create(commentIn);

                var commentOut = context.Comments.FirstOrDefault(x => x.ID == id);
                Assert.NotNull(commentOut);
            }
        }

        [Fact]
        public void Update_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var commentService = new CommentsService(context, null);
                var comment = context.Comments.First();

                comment.Body = "Changed Body";

                commentService.Update(comment);

                var commentOut = context.Comments.FirstOrDefault(x => x.Body == "Changed Body");
                Assert.NotNull(commentOut);
            }
        }

        [Theory]
        [InlineData(1, false)] //since id1 have child element it is expeted not to be deleted
        [InlineData(2, true)]
        public void Delete_Test(int id, bool expected)
        {
            using (var context = Fixture.CreateContext())
            {
                var loggerMock = new LoggerMock<AttachmentsService>();
                var logger = loggerMock.GetServiceLogger();
                var commentService = new CommentsService(context, logger);

                commentService.Delete(id);

                var commentOut = context.Comments.FirstOrDefault(x => x.ID == id);
                var logs = loggerMock.GetLogs;
                Assert.Equal(expected, commentOut == null);
                Assert.Equal(expected, logs.Count != 0);
            }
        }

        [Theory]
        [InlineData(1,true)]
        [InlineData(999,false)]
        public void Exists_Test(int id, bool expected)
        {
            using (var context = Fixture.CreateContext())
            {
                var commentService = new CommentsService(context, null);
                Assert.Equal(expected, commentService.Exists(id));
            }
        }
    }
}
