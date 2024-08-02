using FastEndpoints;
using FluentValidation;

namespace Frontline.Features.Profiles.UpdateGameProfile;

public class GameProfileUpdateRequest
{
    public int ActiveDeckId { get; set; }
}

public class Validator : Validator<GameProfileUpdateRequest>
{
    public Validator()
    {
        RuleFor(x => x.ActiveDeckId)
            .Must(x => x is 0 or 1 or 10 or 11);
    }
}