
using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.Extensions.Configuration;

namespace CatalogService.Api
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

            builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection("CatalogSettings"));
            builder.Services.ConfigureDbContext(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //Uygulama ayaða kalkarken migration iþlemlerini tamamlamasý için çaðýrýlan metod.
            app.MigrateDbContext<CatalogContext>((context, services) =>
            {
                var env = services.GetService<IWebHostEnvironment>();
                var logger = services.GetService<ILogger<CatalogContextSeed>>();

                new CatalogContextSeed()
                    .SeedAsync(context, env, logger)
                    .Wait();
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
