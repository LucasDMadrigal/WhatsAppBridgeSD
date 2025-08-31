using Microsoft.EntityFrameworkCore;
using WhatsAppBridgeSD.src.Core.Interfaces;
using WhatsAppBridgeSD.src.Infrastructure.Data;
using WhatsAppBridgeSD.src.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// lectura de configuración
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// DB
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cs));

// DI
builder.Services.AddScoped<IMessageService, MessageService>();

// SignalR (opcional)
builder.Services.AddSignalR();


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapHub<ChatHub>("/chatHub"); // si usás SignalR

app.Run();
