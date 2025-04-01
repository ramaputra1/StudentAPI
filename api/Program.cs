using Microsoft.EntityFrameworkCore;
// using api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi(); // nda pake ini lagi pakai swager (!!!!!)
builder.Services.AddEndpointsApiExplorer(); // diganti ini (!!!!!) // SWAGGER -1
builder.Services.AddSwaggerGen(); // diganti ini (!!!!!) // SWAGGER -1


// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
//     new MySqlServerVersion(new Version(8, 0, 21))));


builder.Services.AddControllers();





var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers().RequireHost("*"); // agar routing tidak sensitif huruf
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi(); // nda pake ini
    app.UseSwagger();         // SWAGGER -2
    app.UseSwaggerUI();       // SWAGGER -2
}

app.UseHttpsRedirection();

// app.MapGet("/students", async (ApplicationDBContext db) => 
//     await db.Student.ToListAsync()
// );


app.Run();

