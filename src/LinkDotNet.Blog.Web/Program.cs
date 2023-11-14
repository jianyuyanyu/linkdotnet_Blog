using Blazored.Toast;
using LinkDotNet.Blog.Web.Authentication.OpenIdConnect;
using LinkDotNet.Blog.Web.Authentication.Dummy;
using LinkDotNet.Blog.Web.Features;
using LinkDotNet.Blog.Web.Features.Layout;
using LinkDotNet.Blog.Web.RegistrationExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkDotNet.Blog.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        var appConfiguration = AppConfigurationFactory.Create(builder.Configuration);
        builder.Services.AddSingleton(_ => appConfiguration);
        builder.Services.AddBlazoredToast();
        builder.Services.RegisterServices();
        builder.Services.AddStorageProvider(builder.Configuration);
        builder.Services.AddResponseCompression();
        builder.Services.AddControllers();
        builder.Services.AddHostedService<BlogPostPublisher>();
        builder.Services.AddHostedService<TransformBlogPostRecordsService>();

        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.UseDummyAuthentication();
        }
        else
        {
            builder.Services.UseAuthentication(appConfiguration);
        }

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapControllers();

        app.UseAntiforgery();
        app.UseStatusCodePagesWithRedirects("/NotFound");

        app.Run();
    }
}
