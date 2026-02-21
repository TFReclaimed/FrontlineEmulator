namespace Frontline.Battle;

public class ActiveEntityCardData : ActiveCardData
{
    public const sbyte Deploy = 1;

    public const sbyte Attack = 2;

    public const sbyte Move = 4;

    public const sbyte Activate = 8;

    public const sbyte AnyActionMask = 15;

    public const sbyte AnyButDeployMask = 14;

    public const sbyte MoveAttackMask = 6;

    public const sbyte MoveActivateMask = 12;

    public const sbyte ActivateAttackMask = 10;

    public sbyte currentHealth;

    public sbyte acted;

    public override void Setup(Card card)
    {
        base.Setup(card);
        acted = 0;
        EntityCard entityCard = (EntityCard) card;
        currentHealth = entityCard.GetMaxHealth();
    }
}