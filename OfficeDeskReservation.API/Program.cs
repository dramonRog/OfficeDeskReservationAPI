using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Mappings;
using OfficeDeskReservation.API.Services.Implementations;
using OfficeDeskReservation.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDeskService, DeskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AppMappingProfile>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
