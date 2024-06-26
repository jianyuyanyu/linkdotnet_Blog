using System.Threading;
using System.Threading.Tasks;
using LinkDotNet.Blog.Domain;
using LinkDotNet.Blog.Infrastructure.Persistence.Sql;
using LinkDotNet.Blog.TestUtilities;
using LinkDotNet.Blog.Web;
using LinkDotNet.Blog.Web.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCronJob;

namespace LinkDotNet.Blog.IntegrationTests.Web.Features;

public class SimilarBlogPostJobTests : SqlDatabaseTestBase<BlogPost>
{
    private readonly Repository<SimilarBlogPost> similarBlogPostRepository;
    
    public SimilarBlogPostJobTests()
    {
        similarBlogPostRepository = 
            new Repository<SimilarBlogPost>(DbContextFactory, Substitute.For<ILogger<Repository<SimilarBlogPost>>>());
    }
    
    [Fact]
    public async Task ShouldCalculateSimilarBlogPosts()
    {
        var blogPost1 = new BlogPostBuilder().WithTitle("Title 1").Build();
        var blogPost2 = new BlogPostBuilder().WithTitle("Title 2").Build();
        var blogPost3 = new BlogPostBuilder().WithTitle("Title 3").Build();
        await Repository.StoreAsync(blogPost1);
        await Repository.StoreAsync(blogPost2);
        await Repository.StoreAsync(blogPost3);
        var config = Options.Create(new ApplicationConfiguration { ShowSimilarPosts = true });
        
        var job = new SimilarBlogPostJob(Repository, similarBlogPostRepository, config);
        await job.RunAsync(new JobExecutionContext(typeof(SimilarBlogPostJob), true), CancellationToken.None);
        
        var similarBlogPosts = await similarBlogPostRepository.GetAllAsync();
        similarBlogPosts.Should().HaveCount(3);
    }
    
    [Fact]
    public async Task ShouldNotCalculateWhenDisabledInApplicationConfiguration()
    {
        var blogPost1 = new BlogPostBuilder().WithTitle("Title 1").Build();
        var blogPost2 = new BlogPostBuilder().WithTitle("Title 2").Build();
        var blogPost3 = new BlogPostBuilder().WithTitle("Title 3").Build();
        await Repository.StoreAsync(blogPost1);
        await Repository.StoreAsync(blogPost2);
        await Repository.StoreAsync(blogPost3);
        var config = Options.Create(new ApplicationConfiguration { ShowSimilarPosts = false });
        
        var job = new SimilarBlogPostJob(Repository, similarBlogPostRepository, config);
        await job.RunAsync(new JobExecutionContext(typeof(SimilarBlogPostJob), true), CancellationToken.None);
        
        var similarBlogPosts = await similarBlogPostRepository.GetAllAsync();
        similarBlogPosts.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ShouldNotCalculateWhenNotTriggeredAsInstantJob()
    {
        var blogPost1 = new BlogPostBuilder().WithTitle("Title 1").Build();
        var blogPost2 = new BlogPostBuilder().WithTitle("Title 2").Build();
        var blogPost3 = new BlogPostBuilder().WithTitle("Title 3").Build();
        await Repository.StoreAsync(blogPost1);
        await Repository.StoreAsync(blogPost2);
        await Repository.StoreAsync(blogPost3);
        var config = Options.Create(new ApplicationConfiguration { ShowSimilarPosts = true });
        
        var job = new SimilarBlogPostJob(Repository, similarBlogPostRepository, config);
        await job.RunAsync(new JobExecutionContext(typeof(SimilarBlogPostJob), null), CancellationToken.None);
        
        var similarBlogPosts = await similarBlogPostRepository.GetAllAsync();
        similarBlogPosts.Should().BeEmpty();
    }
}
