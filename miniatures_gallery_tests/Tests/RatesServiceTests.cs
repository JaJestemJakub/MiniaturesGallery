using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using Xunit;

namespace MiniaturesGallery.Tests.Tests
{
    [Collection("TestDatabaseTestsCollection")]
    public class RatesServiceTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        public RatesServiceTests(TestDatabaseFixture fixture)
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
                var ratesService = new RatesService(context, null);
                var rate = ratesService.Get(1);

                Assert.NotNull(rate);
                Assert.Equal(5, rate.Rating);
            }
        }

        [Fact]
        public void Create_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                Rate rateIn = new Rate
                {
                    Rating = 4,
                    PostID = 2,
                    UserID = Fixture.TestingUserID_2
                };
                var ratesService = new RatesService(context, null);
                int id = ratesService.Create(rateIn);

                var rateOut = context.Rates.FirstOrDefault(x => x.ID == id);
                Assert.NotNull(rateOut);

                var postService = new PostsService(context, null);
                Post post = (Post)(postService.Get(rateOut.PostID, Fixture.TestingUserID_2));
                Assert.Equal(4.5, post.Rating);
            }
        }

        [Fact]
        public void Update_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var ratesService = new RatesService(context, null);
                var rate = context.Rates.First();

                rate.Rating = 3;

                ratesService.Update(rate);

                var rateOut = context.Rates.FirstOrDefault(x => x.Rating == 3);
                Assert.NotNull(rateOut);

                var postService = new PostsService(context, null);
                Post post = (Post)(postService.Get(rateOut.PostID, Fixture.TestingUserID_1));
                Assert.Equal(3, post.Rating);
            }
        }

        [Fact]
        public void Delete_Test()
        {
            using (var context = Fixture.CreateContext())
            {
                var loggerMock = new LoggerMock<AttachmentsService>();
                var logger = loggerMock.GetServiceLogger();
                var ratesService = new RatesService(context, logger);
                var rate = context.Rates.First();

                ratesService.Delete(rate.ID);

                var rateOut = context.Rates.FirstOrDefault(x => x.ID == rate.ID);
                var logs = loggerMock.GetLogs;

                Assert.Null(rateOut);
                Assert.Contains("|INFO|", logs.First());
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(999, false)]
        public void Exists_Test(int id, bool expected)
        {
            using (var context = Fixture.CreateContext())
            {
                var ratesService = new RatesService(context, null);
                Assert.Equal(expected, ratesService.Exists(id));
            }
        }
    }
}
