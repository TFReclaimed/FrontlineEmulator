using FastEndpoints;
using FluentValidation;

namespace Frontline.Features.Session.Inventory.Dropships.SaveDropship;

public class SaveDropshipRequest
{
    public int DropshipId { get; set; }
    public required SaveDropshipParams Param { get; set; }
}

public class SaveDropshipParams
{
    public required int[] InstanceIds { get; set; }
}

public class Validator : Validator<SaveDropshipRequest>
{
    public Validator()
    {
        RuleFor(x => x.Param.InstanceIds)
            .Must(x => x.Length is >= 31 and <= 41);
    }
}