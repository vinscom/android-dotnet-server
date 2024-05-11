
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;

namespace android_dotnet_server
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
            // Output the configured providers

            foreach (var provider in builder.Configuration.Sources.ToList())
            {
                switch (provider)
                {
                    case MemoryConfigurationSource _:
                        Console.WriteLine("MemoryConfigurationSource");
                        break;
                    case EnvironmentVariablesConfigurationSource envVarSource:
                        Console.WriteLine("EnvironmentVariablesConfigurationSource: " + envVarSource.Prefix);
                        break;
                    case JsonConfigurationSource c:
                        Console.WriteLine("JsonConfigurationSource: " + c.FileProvider?.GetFileInfo(c.Path)?.PhysicalPath);
                        break;
                    case ChainedConfigurationSource _:
                        Console.WriteLine("ChainedConfigurationSource");
                        break;
                    default:
                        Console.WriteLine(provider.ToString());
                        break;
                }
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
