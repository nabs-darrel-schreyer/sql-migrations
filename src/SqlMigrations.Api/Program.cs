using Nabs.Launchpad.Core.Persistence;
using Scalar.AspNetCore;
using SqlMigrations.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


var connectionString = "Server=.;Database=SqlMigrationsDatabase;Integrated Security=True;TrustServerCertificate=True;";
builder.Services.AddSqlDb<TestDbContext>(connectionString);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var serviceScope = app.Services.CreateAsyncScope();
    await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();
    //await dbContext.Database.EnsureDeletedAsync();
    //await dbContext.Database.EnsureCreatedAsync();


}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
