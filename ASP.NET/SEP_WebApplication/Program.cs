using Microsoft.OpenApi.Models;

namespace SEP_WebApplication
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
            builder.Services.AddHostedService(sp => new BLL.CoreServisecs.CoinPriceUpdater(sp.GetRequiredService<ILogger<BLL.CoreServisecs.CoinPriceUpdater>>(), "LocaleConnection"));
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Name = "X-Api-Key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Введите API-ключ в поле Value"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
            });

            builder.Services.AddHttpClient("Api", c =>
            {
                c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
                c.DefaultRequestHeaders.Add("X-Api-Key", builder.Configuration["Api:Key"]!);
            });

            builder.Services.AddHttpClient("ApiAdmin", c =>
            {
                c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
                c.DefaultRequestHeaders.Add("X-Api-Key", builder.Configuration["Api:AdminKey"]!);
            });

            builder.Services.AddCors(o => o.AddPolicy("Client", p =>
                p.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod()));

            BLL.Configuration.ConnectionStringProvider.Initialize(builder.Configuration);
            BLL.Logger.Log.Path = System.IO.Path.Combine(builder.Environment.ContentRootPath, "data", "log.txt");
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("Client");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
