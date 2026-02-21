using Frontline.Game;

namespace Frontline.Battle;

public class GameResources
{
    public sbyte commandAccum;

    public sbyte commandUnits;

    public sbyte health;

    public sbyte maxHealth;

    public sbyte drawDamage;

    public void Create(sbyte baseHealth)
    {
        maxHealth = baseHealth;
        health = maxHealth;
        drawDamage = 1;
    }

    public void NewTurn(GameTemplate rules)
    {
        commandAccum += rules.NewTurnCommand;
        sbyte maxCommandAccum = rules.MaxCommandAccum;
        if (commandAccum > maxCommandAccum)
        {
            commandAccum = maxCommandAccum;
        }

        commandUnits = commandAccum;
    }

    public void Deploy(sbyte cost)
    {
        commandUnits -= cost;
    }

    public void AddCommandPoints(sbyte points, GameTemplate rules)
    {
        commandUnits += points;
        if (commandUnits > rules.MaxCommandAccum)
        {
            commandUnits = rules.MaxCommandAccum;
        }
        else if (commandUnits < 0)
        {
            commandUnits = 0;
        }
    }

    public sbyte HealDamage(sbyte heal)
    {
        sbyte b = health;
        if (health + heal > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += heal;
        }

        return (sbyte) (health - b);
    }
}