using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniaturesGallery.Data;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace MiniaturesGallery.Tests
{
    public class TestDatabaseFixture
    {
        private const string ConnectionString = "Data Source=DB/TestMiniaturesGallerySQLite.db";
        public string TestingUserID_1 { get; set; }
        public string TestingUserID_2 { get; set; }
        public MockFileSystem FileSystem { get; set; }

        public TestDatabaseFixture()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if(TestingUserID_1.IsNullOrEmpty()) //DB is renew with every test, but user are not
            {
                var store = new UserStore<IdentityUser>(context);
                var testingUser1 = new IdentityUser("testingUser1");
                var testingUser2 = new IdentityUser("testingUser2");
                store.CreateAsync(testingUser1);
                store.CreateAsync(testingUser2);

                TestingUserID_1 = testingUser1.Id;
                TestingUserID_2 = testingUser2.Id;
            }

            FileSystem = new MockFileSystem();
            FileSystem.AddDirectory("Files");
            var file1 = new MockFileData("");
            var file2 = new MockFileData("");
            var file3 = new MockFileData("");
            FileSystem.AddFile("default1.jpg", file1);
            FileSystem.AddFile("default2.jpg", file2);
            FileSystem.AddFile("testImage.jpg", file3);
            SeedTestData.SeedDBTesting(context, TestingUserID_1, TestingUserID_2, FileSystem, "");
        }

        public void Cleanup()
        {
            using var context = CreateContext();

            context.PostsAbs.RemoveRange(context.PostsAbs);
            context.SaveChanges();

            FileSystem = new MockFileSystem();
            FileSystem.AddDirectory("Files");
            var file1 = new MockFileData("");
            var file2 = new MockFileData("");
            var file3 = new MockFileData("");
            FileSystem.AddFile("default1.jpg", file1);
            FileSystem.AddFile("default2.jpg", file2);
            FileSystem.AddFile("testImage.jpg", file3);
            SeedTestData.SeedDBTesting(context, TestingUserID_1, TestingUserID_2, FileSystem, "");
        }

        public ApplicationDbContext CreateContext()
            => new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(ConnectionString)
                    .Options);
    }

    [CollectionDefinition("TestDatabaseTestsCollection")]
    public class TestDatabaseTestsCollection : ICollectionFixture<TestDatabaseFixture>
    {
    }
}
