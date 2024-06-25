using FastEndpoints;
using FluentValidation;

namespace Frontline.Features.Guilds.UpdateMemberRank;

public class UpdateMemberRankRequest
{
    public Guid GuildId { get; set; }
    public int UserId { get; set; }
    [FromBody]
    public GuildMember Member { get; set; }
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