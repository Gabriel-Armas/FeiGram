using Microsoft.EntityFrameworkCore;
using ProfileApi.Models;

namespace ProfileApi.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
}
