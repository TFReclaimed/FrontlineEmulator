using FastEndpoints;

namespace Frontline.Features.Missions.StartMission;

public class StartMissionRequest
{
    public MissionKey Key { get; set; }
    public int RequiredCardTemplateId { get; set; }
    [BindFrom("RequiredCardInstanceId")]
    public int RequiredCardItemId { get; set; }
    public int BonusCard1TemplateId { get; set; }
    [BindFrom("BonusCard1InstanceId")]
    public int BonusCard1ItemId { get; set; }
    public int BonusCard2TemplateId { get; set; }
    [BindFrom("BonusCard2InstanceId")]
    public int BonusCard2ItemId { get; set; }
}