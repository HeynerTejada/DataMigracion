using EntityFramework.Exceptions.MySQL.Pomelo;
using Microsoft.EntityFrameworkCore;
using WepApi.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var serverVersion = new MySqlServerVersion(new Version(5, 7, 33));
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseMySql(builder.Configuration.GetConnectionString("ApplicationConnection"), serverVersion
    , b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
    .EnableSensitiveDataLogging(true)
    .UseExceptionProcessor());

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