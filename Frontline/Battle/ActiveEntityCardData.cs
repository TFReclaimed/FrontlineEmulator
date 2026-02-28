namespace Frontline.Battle;

public class ActiveEntityCardData : ActiveCardData
{
    public sbyte CurrentHealth { get; set; }

    public sbyte Acted { get; set; }

    public override void Setup(Card card)
    {
        base.Setup(card);
        Acted = 0;
        var entityCard = (EntityCard) card;
        CurrentHealth = entityCard.GetMaxHealth();
    }
}

[Flags]
public enum EntityActionType
{
    Deploy = 1,
    Attack = 2,
    Move = 4,
    Activate = 8,
    MoveAttackMask = Move | Attack,
    MoveActivateMask = Move | Activate,
    ActivateAttackMask = Activate | Attack,
    AnyButDeployMask = Move | Attack | Activate,
    AnyActionMask = Deploy | Move | Attack | Activate
}