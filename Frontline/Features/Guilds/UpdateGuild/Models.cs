using System.Text.Json.Serialization;
using FastEndpoints;
using FluentValidation;
using Frontline.Data.Entities;

namespace Frontline.Features.Guilds.UpdateGuild;

public class UpdateGuildRequest
{
    public Guid GuildId { get; set; }
    public string Description { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GuildMode Mode { get; set; }
    public string AvatarId { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GuildLocale Locale { get; set; }
}

public class Validator : Validator<UpdateGuildRequest>
{
    private static readonly List<string> Avatars =
    [
        "Flag",
        "Skull_Patch",
        "Rocket_Goblin",
        "Cobra",
        "Broken_Star",
        "Gen3",
        "Interstellar",
        "Akimbo",
        "Gun_Wing",
        "First_to_Fall",
        "Crazy_Bomb",
        "Phoenix",
        "Gen5",
        "Rook",
        "100_Guns",
        "Defiant",
        "Gooser",
        "Gooser_Elite",
        "Cosmonaut",
        "Winged_Skull_Red",
        "Animal_Skull",
        "Dice",
        "AceOfSpades",
        "High_Five",
        "Pirate_Banner",
        "Gen6",
        "Eagle_Shield",
        "V-Twin_Star",
        "Marksman",
        "Hammond_Aerospace",
        "Gen2",
        "Penny_Arcade_M5",
        "Dragon",
        "Wings",
        "Rising_Star",
        "Angled_Eagle",
        "Gen4",
        "Blue_Star",
        "Alligator",
        "Gen8",
        "BulletHash",
        "Gen7",
        "Chevrons",
        "Vortex_Shield",
        "Blitzkrieg",
        "Bonehead",
        "Stinger",
        "Gen9",
        "Four_of_a_Kind"
    ];
    
    public Validator()
    {
        RuleFor(x => x.Description)
            .MinimumLength(1)
            .MaximumLength(200);
        
        RuleFor(x => x.Mode)
            .IsInEnum();
        
        RuleFor(x => x.AvatarId)
            .Must(BeAValidAvatar);
        
        RuleFor(x => x.Locale)
            .IsInEnum();
    }

    private static bool BeAValidAvatar(string avatar)
    {
        return Avatars.Contains(avatar);
    }
}