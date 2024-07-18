using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data;

public class AppDb : DbContext
{
    public DbSet<PlayerEntity> Players { get; set; }
    
    public DbSet<GuildEntity> Guilds { get; set; }
    
    public DbSet<GuildMemberEntity> GuildMembers { get; set; }
    
    public DbSet<ItemEntity> Items { get; set; }
    
    public AppDb(DbContextOptions<AppDb> options) : base(options)
    {
    }
}