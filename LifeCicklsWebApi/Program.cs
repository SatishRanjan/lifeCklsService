using LifeCicklsWebApi.ErrorHandling;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace LifeCicklsWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers(config =>
            {
                config.Filters.Add(typeof(LifeCklsExceptionFilter));
            });

            
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = EnvironmentHelper.GetEnvironmentValueOrDefault("COSMOSDB_MONGO_CONNECTION", "mongodb://localhost:55000/");
                return new MongoClient(connectionString);
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Enable CORS
            app.UseCors(options =>
            {
                options.AllowAnyOrigin();  // You can also specify specific origins instead of AllowAnyOrigin
                options.AllowAnyMethod();
                options.AllowAnyHeader();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.UseCors("AllowAll");

            app.Run();
            //app.Run("http://localhost:8001");
        }
    }
}
