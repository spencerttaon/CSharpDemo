using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
public class Startup {
    public void ConfigureServices(IServiceCollection services) {
        services.AddControllers();
        services.AddScoped<HistoryService, HistoryService>();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        app.UseMiddleware<LoggingMiddleware>();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}