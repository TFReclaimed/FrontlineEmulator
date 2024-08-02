using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data;

public class AppDb : DbContext
{
    public DbSet<PlayerEntity> Players { get; set; }
    
    public DbSet<GuildEntity> Guilds { get; set; }
    
    public DbSet<GuildMemberEntity> GuildMembers { get; set; }
    
    public DbSet<ItemEntity> Items { get; set; }
    
    public DbSet<DropshipEntity> Dropships { get; set; }
    
    public DbSet<FinishedMissionEntity> FinishedMissions { get; set; }
    
    public DbSet<ActiveMissionEntity> ActiveMissions { get; set; }
    
    public AppDb(DbContextOptions<AppDb> options) : base(options)
    {
    }
}