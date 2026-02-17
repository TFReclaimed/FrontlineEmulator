using FastEndpoints;
using FluentValidation;

namespace Frontline.Endpoints.Profiles.UpdateProfile;

public class ProfileUpdateRequest
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarId { get; set; } = string.Empty;
}

public class Validator : Validator<ProfileUpdateRequest>
{
    public Validator()
    {
        RuleFor(x => x.DisplayName)
            .MinimumLength(2)
            .MaximumLength(18);
        
        RuleFor(x => x.AvatarId)
            .Must(BeAValidAvatar);
    }
    
    private static bool BeAValidAvatar(string avatar)
    {
        if (string.IsNullOrEmpty(avatar))
        {
            return false;
        }

        if (!avatar.StartsWith("avatar") || avatar.Length != 9)
        {
            return false;
        }
        
        var numberPart = avatar.Substring(6, 3);
        if (int.TryParse(numberPart, out var number))
        {
            return number is >= 1 and <= 12;
        }

        return false;
    }
}