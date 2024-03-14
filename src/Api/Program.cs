using Api.Interfaces;
using Api.Services;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // register dependencies
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            builder.Services.AddHttpClient<IHackerNewsServiceClient, HackerNewsService>(client =>
            {
                client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
            });
            builder.Services.AddSingleton<IHackerNewsService, CachedHackerNewsServiceDecorator>();
            builder.Services.AddTransient<IBestStoriesService, BestStoriesService>();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}