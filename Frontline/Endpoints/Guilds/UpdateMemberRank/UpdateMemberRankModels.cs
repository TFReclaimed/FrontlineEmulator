using FastEndpoints;
using FluentValidation;

namespace Frontline.Endpoints.Guilds.UpdateMemberRank;

public class UpdateMemberRankRequest
{
    public Guid GuildId { get; set; }
    public int UserId { get; set; }
    [FromBody]
    public required GuildMemberDto Member { get; set; }
}

public class Validator : Validator<UpdateMemberRankRequest>
{
    public Validator()
    {
        RuleFor(x => x.Member)
            .NotNull();
        
        RuleFor(x => x.Member.Rank)
            .IsInEnum();
    }
}