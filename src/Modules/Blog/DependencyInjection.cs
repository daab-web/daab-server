using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Daab.Modules.Blog;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the blog module. The host must also register an <c>IContentVerifier</c> and an
        /// <c>ITextTranslator</c> (they bridge to the translation module, which this module does not
        /// reference).
        /// </summary>
        public IServiceCollection AddBlogModule(IConfiguration config)
        {
            var connectionString =
                config.GetConnectionString("blog-module")
                ?? throw new ArgumentException("Blog module connection string cannot be null");

            services.AddDbContext<BlogDbContext>(optionsBuilder =>
                optionsBuilder.UseSqlite(connectionString)
            );

            services.AddScoped<BlogLocales>();

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
            );

            return services;
        }
    }

    extension(WebApplication app)
    {
        public IHost UseBlogModule()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

            context.Database.Migrate();

            return app;
        }
    }
}
