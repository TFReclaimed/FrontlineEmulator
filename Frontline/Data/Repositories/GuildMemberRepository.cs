using Frontline.Data.Entities;

namespace Frontline.Data.Repositories;

public interface IGuildMemberRepository : IRepository<GuildMemberEntity>
{
}

public class GuildMemberRepository : RepositoryBase<GuildMemberEntity>, IGuildMemberRepository
{
    public GuildMemberRepository(AppDb db) : base(db)
    {
    }
}