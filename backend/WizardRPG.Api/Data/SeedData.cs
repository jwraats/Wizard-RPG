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

        if (!context.PotionIngredients.Any())
        {
            var dragonScale = new PotionIngredient { Id = Guid.NewGuid(), Name = "Dragon Scale", Description = "A shimmering scale shed by an ancient dragon.", Price = 50 };
            var moonstoneDust = new PotionIngredient { Id = Guid.NewGuid(), Name = "Moonstone Dust", Description = "Fine powder from a moonstone, glows faintly at night.", Price = 30 };
            var phoenixFeather = new PotionIngredient { Id = Guid.NewGuid(), Name = "Phoenix Feather", Description = "A radiant feather that is warm to the touch.", Price = 80 };
            var toadstoolCap = new PotionIngredient { Id = Guid.NewGuid(), Name = "Toadstool Cap", Description = "A spotted mushroom cap with mild magical properties.", Price = 15 };
            var fairyWing = new PotionIngredient { Id = Guid.NewGuid(), Name = "Fairy Wing", Description = "A delicate wing donated by a friendly fairy.", Price = 40 };
            var mandrakeRoot = new PotionIngredient { Id = Guid.NewGuid(), Name = "Mandrake Root", Description = "A gnarled root that screams when pulled from the earth.", Price = 25 };

            context.PotionIngredients.AddRange(dragonScale, moonstoneDust, phoenixFeather, toadstoolCap, fairyWing, mandrakeRoot);

            var healingBrew = new PotionRecipe
            {
                Id = Guid.NewGuid(),
                Name = "Healing Brew",
                Description = "A soothing potion that mends minor wounds.",
                Difficulty = 1,
                GoldReward = 20,
                XpReward = 15
            };

            var invisibilityDraught = new PotionRecipe
            {
                Id = Guid.NewGuid(),
                Name = "Invisibility Draught",
                Description = "Turns the drinker invisible for a short time.",
                Difficulty = 3,
                GoldReward = 60,
                XpReward = 40
            };

            var fireResistanceElixir = new PotionRecipe
            {
                Id = Guid.NewGuid(),
                Name = "Fire Resistance Elixir",
                Description = "Grants temporary immunity to fire damage.",
                Difficulty = 4,
                GoldReward = 100,
                XpReward = 65
            };

            var felixFelicis = new PotionRecipe
            {
                Id = Guid.NewGuid(),
                Name = "Felix Felicis",
                Description = "Liquid luck — everything seems to go right for a while.",
                Difficulty = 5,
                GoldReward = 200,
                XpReward = 100
            };

            context.PotionRecipes.AddRange(healingBrew, invisibilityDraught, fireResistanceElixir, felixFelicis);
            context.SaveChanges();

            var recipeIngredients = new List<RecipeIngredient>
            {
                // Healing Brew: 2 Toadstool Cap + 1 Mandrake Root
                new() { RecipeId = healingBrew.Id, IngredientId = toadstoolCap.Id, Quantity = 2 },
                new() { RecipeId = healingBrew.Id, IngredientId = mandrakeRoot.Id, Quantity = 1 },
                // Invisibility Draught: 1 Moonstone Dust + 2 Fairy Wing
                new() { RecipeId = invisibilityDraught.Id, IngredientId = moonstoneDust.Id, Quantity = 1 },
                new() { RecipeId = invisibilityDraught.Id, IngredientId = fairyWing.Id, Quantity = 2 },
                // Fire Resistance Elixir: 2 Dragon Scale + 1 Phoenix Feather
                new() { RecipeId = fireResistanceElixir.Id, IngredientId = dragonScale.Id, Quantity = 2 },
                new() { RecipeId = fireResistanceElixir.Id, IngredientId = phoenixFeather.Id, Quantity = 1 },
                // Felix Felicis: 1 Phoenix Feather + 1 Moonstone Dust + 1 Fairy Wing + 1 Dragon Scale
                new() { RecipeId = felixFelicis.Id, IngredientId = phoenixFeather.Id, Quantity = 1 },
                new() { RecipeId = felixFelicis.Id, IngredientId = moonstoneDust.Id, Quantity = 1 },
                new() { RecipeId = felixFelicis.Id, IngredientId = fairyWing.Id, Quantity = 1 },
                new() { RecipeId = felixFelicis.Id, IngredientId = dragonScale.Id, Quantity = 1 },
            };

            context.RecipeIngredients.AddRange(recipeIngredients);
            context.SaveChanges();
        }

        if (!context.QuizQuestions.Any())
        {
            var quizQuestions = new List<QuizQuestion>
            {
                // SpellLore - Easy
                new() { QuestionText = "Which element is associated with the Fireball spell?", OptionA = "Ice", OptionB = "Fire", OptionC = "Lightning", OptionD = "Earth", CorrectOption = "B", Difficulty = QuizDifficulty.Easy, Category = QuizCategory.SpellLore },
                // SpellLore - Medium
                new() { QuestionText = "What is the mana cost of the Thunder Strike spell?", OptionA = "25", OptionB = "30", OptionC = "35", OptionD = "40", CorrectOption = "C", Difficulty = QuizDifficulty.Medium, Category = QuizCategory.SpellLore },
                // SpellLore - Hard
                new() { QuestionText = "Which spell has the highest base damage and costs 50 mana?", OptionA = "Arcane Blast", OptionB = "Lava Surge", OptionC = "Earthquake", OptionD = "Fireball", CorrectOption = "C", Difficulty = QuizDifficulty.Hard, Category = QuizCategory.SpellLore },

                // MagicalCreatures - Easy
                new() { QuestionText = "Which magical creature is reborn from its own ashes?", OptionA = "Dragon", OptionB = "Unicorn", OptionC = "Phoenix", OptionD = "Griffin", CorrectOption = "C", Difficulty = QuizDifficulty.Easy, Category = QuizCategory.MagicalCreatures },
                // MagicalCreatures - Medium
                new() { QuestionText = "What is a Dragon's most prized possession used in potion brewing?", OptionA = "Dragon Claw", OptionB = "Dragon Scale", OptionC = "Dragon Tooth", OptionD = "Dragon Eye", CorrectOption = "B", Difficulty = QuizDifficulty.Medium, Category = QuizCategory.MagicalCreatures },
                // MagicalCreatures - Hard
                new() { QuestionText = "Which creature's feather is described as warm to the touch and costs 80 gold?", OptionA = "Griffin", OptionB = "Hippogriff", OptionC = "Phoenix", OptionD = "Thunderbird", CorrectOption = "C", Difficulty = QuizDifficulty.Hard, Category = QuizCategory.MagicalCreatures },

                // PotionIngredients - Easy
                new() { QuestionText = "Which ingredient glows faintly at night?", OptionA = "Dragon Scale", OptionB = "Mandrake Root", OptionC = "Moonstone Dust", OptionD = "Toadstool Cap", CorrectOption = "C", Difficulty = QuizDifficulty.Easy, Category = QuizCategory.PotionIngredients },
                // PotionIngredients - Medium
                new() { QuestionText = "What happens when you pull a Mandrake Root from the earth?", OptionA = "It glows", OptionB = "It screams", OptionC = "It explodes", OptionD = "It vanishes", CorrectOption = "B", Difficulty = QuizDifficulty.Medium, Category = QuizCategory.PotionIngredients },
                // PotionIngredients - Hard
                new() { QuestionText = "Which two ingredients are needed to brew the Fire Resistance Elixir?", OptionA = "Moonstone Dust and Fairy Wing", OptionB = "Toadstool Cap and Mandrake Root", OptionC = "Dragon Scale and Phoenix Feather", OptionD = "Fairy Wing and Dragon Scale", CorrectOption = "C", Difficulty = QuizDifficulty.Hard, Category = QuizCategory.PotionIngredients },

                // WizardHistory - Easy
                new() { QuestionText = "What is the name of the academy where young wizards first learn to cast spells?", OptionA = "The Arcane Academy", OptionB = "The Crystal Tower", OptionC = "The Shadow Keep", OptionD = "The Ember Sanctum", CorrectOption = "A", Difficulty = QuizDifficulty.Easy, Category = QuizCategory.WizardHistory },
                // WizardHistory - Medium
                new() { QuestionText = "Who is credited with creating the first Arcane Blast spell?", OptionA = "Thalindra the Wise", OptionB = "Eldric Stormweaver", OptionC = "Meraxis the Bold", OptionD = "Solara Dawnfire", CorrectOption = "B", Difficulty = QuizDifficulty.Medium, Category = QuizCategory.WizardHistory },
                // WizardHistory - Hard
                new() { QuestionText = "During the Great Mage War, which school of magic was temporarily forbidden?", OptionA = "Fire Magic", OptionB = "Ice Magic", OptionC = "Arcane Magic", OptionD = "Earth Magic", CorrectOption = "C", Difficulty = QuizDifficulty.Hard, Category = QuizCategory.WizardHistory },
            };

            context.QuizQuestions.AddRange(quizQuestions);
            context.SaveChanges();
        }
    }
}
