using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using Xunit;

namespace MiniaturesGallery.Tests.Tests
{
    [Collection("TestDatabaseTestsCollection")]
    public class PostServiceTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        public PostServiceTests(TestDatabaseFixture fixture)
        {
            Fixture = fixture;
        }
        public TestDatabaseFixture Fixture { get; }
        public void Dispose() => Fixture.Cleanup();

        [Fact]
        public void GetAll_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                var posts = postService.GetAll();

                var announcment = posts.OfType<Announcement>().First();

                //Announcement Part
                Assert.IsType<Announcement>(announcment);
                Assert.Equal("First Announcement", announcment.Topic);
                Assert.Equal("Example Announcement from admin account.", announcment.Text);
                Assert.Equal("Hidden note from admin account.", announcment.PrivateNote);
                Assert.Equal(Fixture.TestingUserID_1, announcment.UserID);

                var post = posts.OfType<Post>().First();
                //Post Part
                Assert.IsType<Post>(post);
                Assert.Equal("First Post", post.Topic);
                Assert.Equal("Example Post from admin account.", post.Text);
                Assert.Equal(Fixture.TestingUserID_2, post.UserID);

                //Comments part
                Assert.Equal(2, post.NoOfComments);

                //Rates part
                Assert.Equal(1, post.NoOfRates);
                Assert.Equal(5, post.Rating);

                //Attachments part
                Assert.Equal(2, post.Attachments.Count);
                Assert.Equal("default1.jpg", post.Attachments.First().FileName);
                Assert.Equal("2\\default1.jpg", post.Attachments.First().FullFileName);
                Assert.Equal(Fixture.TestingUserID_2, post.Attachments.First().UserID);
            }
        }

        [Fact]
        public void GetAll_WithUser_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                var posts = postService.GetAll(Fixture.TestingUserID_1);

                var announcment = posts.OfType<Announcement>().First();
                //Announcement Part
                Assert.IsType<Announcement>(announcment);

                var post = posts.OfType<Post>().First();
                //Post Part
                Assert.IsType<Post>(post);

                //Rates part
                Assert.Equal(Fixture.TestingUserID_1, post.Rates.First().UserID);
            }
        }

        [Fact]
        public void GetAll_WithUserSearchString_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                var posts = postService.GetAll(Fixture.TestingUserID_1, "Anno");

                Assert.Single(posts);
            }
        }

        [Fact]
        public void Get_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                var postAbs = postService.Get(2, Fixture.TestingUserID_1);

                var post = postAbs as Post;
                //Post Part
                Assert.IsType<Post>(post);
                Assert.Equal("First Post", post.Topic);

                //Comments part
                Assert.Equal("Body of first Comment.", post.Coments.First().Body);
                Assert.Equal(Fixture.TestingUserID_1, post.Coments.First().UserID);
                Assert.Equal("Body of second Comment in response to first.", post.Coments.First().Comments.First().Body);
                Assert.Equal(Fixture.TestingUserID_2, post.Coments.First().Comments.First().UserID);
            }
        }

        [Fact]
        public void GetOfUser_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                var posts = postService.GetAllOfUser(Fixture.TestingUserID_2, Fixture.TestingUserID_2);

                Assert.Single(posts);
            }
        }

        [Fact]
        public void Create_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postIn = new Post() { Topic = "Test Post", Text = "Description" };
                var postService = new PostsService(context, null);
                int id = postService.Create(postIn, Fixture.TestingUserID_1);

                var postOut = context.Posts.FirstOrDefault(x => x.ID == id);
                Assert.NotNull(postOut);
            }
        }

        [Fact]
        public void Update_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                var post = context.Posts.First();

                post.Topic = "Changed Topic";
                post.Text = "Changed Text";

                postService.Update(post);

                var postOut = context.Posts.FirstOrDefault(x => x.Topic == "Changed Topic" && x.Text == "Changed Text");
                Assert.NotNull(postOut);
            }
        }

        [Fact]
        public void Delete_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var loggerMock = new LoggerMock<AttachmentsService>();
                var logger = loggerMock.GetServiceLogger();
                var postService = new PostsService(context, logger);
                var attachemntService = new AttachmentsService(context, "", logger, Fixture.FileSystem);
                var post = context.Posts.First();

                postService.Delete(post.ID, attachemntService);

                var postOut = context.Posts.FirstOrDefault();
                var logs = loggerMock.GetLogs;
                Assert.Null(postOut);
                Assert.Contains("|INFO|", logs.First());
            }
        }

        [Fact]
        public void Any_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                Assert.True(postService.Any());
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(999, false)]
        public void Exists_Test(int id, bool expected)
        {
            using (var context = Fixture.CreateContext())
            {
                var postService = new PostsService(context, null);
                Assert.Equal(expected, postService.Exists(id));
            }
        }
    }
}
