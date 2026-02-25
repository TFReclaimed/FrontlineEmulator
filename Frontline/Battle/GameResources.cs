using Frontline.Game;

namespace Frontline.Battle;

public class GameResources
{
    public sbyte CommandAccum { get; set; }

    public sbyte CommandUnits { get; set; }

    public sbyte Health { get; set; }

    public sbyte MaxHealth { get; set; }

    public sbyte DrawDamage { get; set; }

    public void Create(sbyte baseHealth)
    {
        MaxHealth = baseHealth;
        Health = MaxHealth;
        DrawDamage = 1;
    }

    public void NewTurn(GameTemplate rules)
    {
        CommandAccum += rules.NewTurnCommand;
        var maxCommandAccum = rules.MaxCommandAccum;
        if (CommandAccum > maxCommandAccum)
        {
            CommandAccum = maxCommandAccum;
        }

        CommandUnits = CommandAccum;
    }

    public void Deploy(sbyte cost)
    {
        CommandUnits -= cost;
    }

    public void AddCommandPoints(sbyte points, GameTemplate rules)
    {
        CommandUnits += points;
        if (CommandUnits > rules.MaxCommandAccum)
        {
            CommandUnits = rules.MaxCommandAccum;
        }
        else if (CommandUnits < 0)
        {
            CommandUnits = 0;
        }
    }

    public sbyte HealDamage(sbyte heal)
    {
        var b = Health;
        if (Health + heal > MaxHealth)
        {
            Health = MaxHealth;
        }
        else
        {
            Health += heal;
        }

        return (sbyte) (Health - b);
    }
}