using WizardRPG.Api.Models;

namespace WizardRPG.Api.Data;

public static class SeedData
{
    public static void Seed(AppDbContext context)
    {
        if (context.Spells.Any()) return;

        var spells = new List<Spell>
        {
            new() { Id = Guid.NewGuid(), Name = "Fireball", Description = "Hurls a blazing ball of fire at the enemy.", ManaCost = 30, BaseDamage = 45, Effect = "Burn", Element = SpellElement.Fire },
            new() { Id = Guid.NewGuid(), Name = "Ice Lance", Description = "Launches a razor-sharp shard of ice.", ManaCost = 25, BaseDamage = 40, Effect = "Slow", Element = SpellElement.Ice },
            new() { Id = Guid.NewGuid(), Name = "Thunder Strike", Description = "Calls down a bolt of lightning.", ManaCost = 35, BaseDamage = 55, Effect = "Stun", Element = SpellElement.Lightning },
            new() { Id = Guid.NewGuid(), Name = "Stone Wall", Description = "Summons a wall of stone to crush the foe.", ManaCost = 20, BaseDamage = 30, Effect = "Knockback", Element = SpellElement.Earth },
            new() { Id = Guid.NewGuid(), Name = "Arcane Blast", Description = "Unleashes raw arcane energy.", ManaCost = 40, BaseDamage = 60, Effect = "Silence", Element = SpellElement.Arcane },
            new() { Id = Guid.NewGuid(), Name = "Frost Nova", Description = "Freezes the enemy in place.", ManaCost = 30, BaseDamage = 35, Effect = "Freeze", Element = SpellElement.Ice },
            new() { Id = Guid.NewGuid(), Name = "Lava Surge", Description = "Sends a torrent of lava at the enemy.", ManaCost = 45, BaseDamage = 65, Effect = "Burn", Element = SpellElement.Fire },
            new() { Id = Guid.NewGuid(), Name = "Earthquake", Description = "Shakes the ground beneath the enemy.", ManaCost = 50, BaseDamage = 70, Effect = "Knockdown", Element = SpellElement.Earth },
        };

        var items = new List<Item>
        {
            new() { Id = Guid.NewGuid(), Name = "Staff of Flames", Description = "A staff imbued with fire magic.", Type = ItemType.Wand, MagicBonus = 15, StrengthBonus = 0, WisdomBonus = 5, SpeedBonus = 0, Price = 500 },
            new() { Id = Guid.NewGuid(), Name = "Archmage Robe", Description = "Flowing robes of a powerful archmage.", Type = ItemType.Robe, MagicBonus = 10, StrengthBonus = 0, WisdomBonus = 10, SpeedBonus = 0, Price = 750 },
            new() { Id = Guid.NewGuid(), Name = "Wizard Hat", Description = "The classic pointy hat.", Type = ItemType.Hat, MagicBonus = 5, StrengthBonus = 0, WisdomBonus = 8, SpeedBonus = 0, Price = 300 },
            new() { Id = Guid.NewGuid(), Name = "Thunderbroom", Description = "A racing broom crackling with electricity.", Type = ItemType.Broom, MagicBonus = 0, StrengthBonus = 5, WisdomBonus = 0, SpeedBonus = 20, Price = 1200 },
            new() { Id = Guid.NewGuid(), Name = "Elixir of Power", Description = "Temporarily boosts all stats.", Type = ItemType.Potion, MagicBonus = 5, StrengthBonus = 5, WisdomBonus = 5, SpeedBonus = 5, Price = 200 },
            new() { Id = Guid.NewGuid(), Name = "Amulet of Wisdom", Description = "An ancient amulet that enhances wisdom.", Type = ItemType.Amulet, MagicBonus = 3, StrengthBonus = 0, WisdomBonus = 15, SpeedBonus = 0, Price = 900 },
        };

        context.Spells.AddRange(spells);
        context.Items.AddRange(items);
        context.SaveChanges();
    }
}
