namespace Frontline.Battle;

public class TraitDuration
{
    public TraitDurationType Type { get; set; } = TraitDurationType.NumDurations;

    public sbyte Duration { get; set; }

    public sbyte Charges { get; set; }
}