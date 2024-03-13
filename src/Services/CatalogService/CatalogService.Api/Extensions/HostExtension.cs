﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace CatalogService.Api.Extensions
{
    public static class HostExtension
    {
        public static IHost MigrateDbContext<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder) 
            where TContext : DbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<TContext>>();

                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    var retry = Policy.Handle<SqlException>()
                             .WaitAndRetry(new TimeSpan[]
                             {
                                 TimeSpan.FromSeconds(3),
                                 TimeSpan.FromSeconds(5),
                                 TimeSpan.FromSeconds(8),
                             });

                    retry.Execute(() => InvokeSeeder(seeder, context, services));

                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                }
            }

            return host;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            //Veritabanının ve ilgili tabloların oluşturulup oluşturulmadığını kontol edyor. oluşturulmadıysa gidip oluşturur.
            context.Database.EnsureCreated();
            //çalıştıılmamış migration var ise çalıştır.
            context.Database.Migrate();
            seeder(context, services); // seeder matodunu çalıştır ve seed datalar db ye eklensin.
        }
    }
}
