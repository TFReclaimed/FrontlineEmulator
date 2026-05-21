using FastEndpoints;
using FluentValidation;

namespace Frontline.Endpoints.Profiles.UpdateGameProfile;

public class GameProfileUpdateRequest
{
    public int ActiveDeckId { get; set; }
}

public class Validator : Validator<GameProfileUpdateRequest>
{
    public Validator()
    {
        RuleFor(x => x.ActiveDeckId)
            .Must(x => x is 0 or 1 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18);
    }
}