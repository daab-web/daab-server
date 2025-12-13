using Daab.Modules.Scientists.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Persistence;

public class ScientistsContext : DbContext
{
    internal DbSet<Scientist> Scientists { get; set; }

    public ScientistsContext(DbContextOptions<ScientistsContext> options)
        : base(options) { }
}
