using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlMigrations.Persistence;
using SqlMigrations.Persistence.Entities;

namespace SqlMigrations.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonEntityController(IDbContextFactory<TestDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet(Name = "GetPersonEntity")]
    public async Task<PersonEntity?> Get()
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        return await dbContext.People.FirstOrDefaultAsync();
    }
}
