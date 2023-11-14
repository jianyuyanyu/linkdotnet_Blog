using LinkDotNet.Blog.Domain;
using LinkDotNet.Blog.Web;
using Microsoft.Extensions.DependencyInjection;

namespace LinkDotNet.Blog.UnitTests.Web.Features.Layout.Components;

public class FooterTests : TestContext
{
    [Fact]
    public void ShouldSetCopyrightInformation()
    {
        var appConfig = new AppConfiguration
        {
            ProfileInformation = new ProfileInformation()
            {
                Name = "Steven",
            },
        };
        Services.AddScoped(_ => appConfig);

        var cut = RenderComponent<Footer>();

        cut.Find("span").TextContent.Should().Contain("Steven");
    }

    [Fact]
    public void ShouldNotSetNameIfAboutMeIsNotEnabled()
    {
        var appConfig = new AppConfiguration();
        Services.AddScoped(_ => appConfig);

        var cut = RenderComponent<Footer>();

        cut.Find("span").TextContent.Should().Contain("©");
    }
}