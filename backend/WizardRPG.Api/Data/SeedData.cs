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

        if (!context.Creatures.Any())
        {
            var creatures = new List<Creature>
            {
                // Common
                new() { Id = Guid.NewGuid(), Name = "Fire Sprite", Description = "A tiny, mischievous flame spirit that hoards shiny coins.", Rarity = CreatureRarity.Common, BaseHealth = 30, BaseAttack = 8, BonusType = "gold", BonusValue = 5 },
                new() { Id = Guid.NewGuid(), Name = "Shadow Cat", Description = "A sleek feline that moves between shadows with uncanny speed.", Rarity = CreatureRarity.Common, BaseHealth = 35, BaseAttack = 12, BonusType = "speed", BonusValue = 5 },
                // Uncommon
                new() { Id = Guid.NewGuid(), Name = "Storm Hawk", Description = "A majestic bird of prey that channels lightning through its feathers.", Rarity = CreatureRarity.Uncommon, BaseHealth = 50, BaseAttack = 18, BonusType = "magic", BonusValue = 8 },
                new() { Id = Guid.NewGuid(), Name = "Iron Golem", Description = "A small but sturdy construct of enchanted metal that never tires.", Rarity = CreatureRarity.Uncommon, BaseHealth = 80, BaseAttack = 15, BonusType = "strength", BonusValue = 8 },
                // Rare
                new() { Id = Guid.NewGuid(), Name = "Phoenix Hatchling", Description = "A baby phoenix wreathed in golden flames, radiating arcane energy and ancient knowledge.", Rarity = CreatureRarity.Rare, BaseHealth = 60, BaseAttack = 25, BonusType = "magic+wisdom", BonusValue = 12 },
                new() { Id = Guid.NewGuid(), Name = "Frost Dragon", Description = "A young dragon of ice that combines raw power with blinding agility.", Rarity = CreatureRarity.Rare, BaseHealth = 90, BaseAttack = 30, BonusType = "strength+speed", BonusValue = 12 },
                // Legendary
                new() { Id = Guid.NewGuid(), Name = "Ancient Basilisk", Description = "A primordial serpent whose gaze petrifies and whose presence empowers all aspects of a wizard.", Rarity = CreatureRarity.Legendary, BaseHealth = 120, BaseAttack = 40, BonusType = "gold+magic+strength+wisdom+speed", BonusValue = 15 },
                new() { Id = Guid.NewGuid(), Name = "Celestial Unicorn", Description = "A divine equine that blesses its companion with fortune and profound insight.", Rarity = CreatureRarity.Legendary, BaseHealth = 100, BaseAttack = 35, BonusType = "gold+wisdom", BonusValue = 20 },
            };

            context.Creatures.AddRange(creatures);
            context.SaveChanges();
        }

        if (!context.StoryChapters.Any())
        {
            // Chapter 1: The Discovery
            var ch1 = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "The Discovery",
                Content = "While wandering the castle's lower halls, you notice a faint shimmer behind a tapestry. Pulling it aside reveals a narrow corridor you've never seen before. Cold air seeps from the darkness within, carrying whispers too faint to understand. The stones around the entrance are etched with ancient runes that pulse with a dim violet light.",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 0,
                IsEnding = false
            };

            // Chapter 2: The Guardian
            var ch2 = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "The Guardian",
                Content = "You step into the corridor and the tapestry falls shut behind you. Torches flicker to life along the walls, illuminating a massive stone golem blocking the passage ahead. Its hollow eyes glow with an amber light, and a deep rumble echoes as it speaks: 'None shall pass without proving their worth.' The ground trembles beneath your feet.",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 1,
                IsEnding = false
            };

            // Chapter 3: The Secret Chamber
            var ch3 = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "The Secret Chamber",
                Content = "Beyond the golem lies a vast underground chamber filled with towering bookshelves and floating candles. An ancient library, untouched for centuries. Dust motes dance in the candlelight. On a central pedestal sit two objects: a leather-bound tome radiating gentle warmth, and a crystalline artifact pulsing with raw magical energy. You sense you can only take one.",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 2,
                IsEnding = false
            };

            // Chapter 4: Knowledge Gained (ending)
            var ch4 = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "Knowledge Gained",
                Content = "You open the ancient tome and knowledge floods your mind — centuries of forgotten spells, lost histories, and arcane secrets. The whispers in the walls grow clear, thanking you for choosing wisdom over power. As you leave the corridor, you feel fundamentally changed. The runes on the entrance dim and seal shut, but the knowledge remains yours forever.",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 3,
                IsEnding = true,
                GoldReward = 50,
                XpReward = 100
            };

            // Chapter 5: The Artifact's Curse (ending)
            var ch5 = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "The Artifact's Curse",
                Content = "You grasp the crystalline artifact and power surges through you. The whispers turn to screams, then silence. The artifact crumbles into golden dust that seeps into your skin, granting you immense but volatile power. As you exit the corridor, you notice your reflection has changed — your eyes now carry an eerie glow. Great power, but at what cost?",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 4,
                IsEnding = true,
                GoldReward = 100,
                XpReward = 50
            };

            // Ending: Walk Away
            var chWalkAway = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "Discretion is Valor",
                Content = "You let the tapestry fall back into place and walk away. Some secrets are best left undiscovered. The whispers fade behind you, and by morning you almost convince yourself it was just the wind. But sometimes, late at night, you still hear them calling...",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 5,
                IsEnding = true,
                GoldReward = 10,
                XpReward = 10
            };

            // Ending: Run from golem
            var chRun = new StoryChapter
            {
                Id = Guid.NewGuid(),
                Title = "A Hasty Retreat",
                Content = "The golem is too imposing. You turn and sprint back through the corridor as the walls begin to shake. You burst through the tapestry and collapse in the hallway, heart pounding. When you look back, the corridor is gone — only solid stone remains. Perhaps you'll be braver next time.",
                StoryArc = "The Forbidden Corridor",
                OrderIndex = 6,
                IsEnding = true,
                GoldReward = 5,
                XpReward = 15
            };

            context.StoryChapters.AddRange(ch1, ch2, ch3, ch4, ch5, chWalkAway, chRun);
            context.SaveChanges();

            var choices = new List<StoryChoice>
            {
                // Chapter 1 choices
                new() { Id = Guid.NewGuid(), ChapterId = ch1.Id, ChoiceText = "Enter the forbidden corridor", NextChapterId = ch2.Id },
                new() { Id = Guid.NewGuid(), ChapterId = ch1.Id, ChoiceText = "Walk away — some doors are best left closed", NextChapterId = chWalkAway.Id },

                // Chapter 2 choices
                new() { Id = Guid.NewGuid(), ChapterId = ch2.Id, ChoiceText = "Channel your wisdom to outwit the golem", NextChapterId = ch3.Id, MinWisdom = 15 },
                new() { Id = Guid.NewGuid(), ChapterId = ch2.Id, ChoiceText = "Negotiate with the ancient guardian", NextChapterId = ch3.Id },
                new() { Id = Guid.NewGuid(), ChapterId = ch2.Id, ChoiceText = "Run back the way you came", NextChapterId = chRun.Id },

                // Chapter 3 choices
                new() { Id = Guid.NewGuid(), ChapterId = ch3.Id, ChoiceText = "Read the ancient tome", NextChapterId = ch4.Id },
                new() { Id = Guid.NewGuid(), ChapterId = ch3.Id, ChoiceText = "Take the crystalline artifact", NextChapterId = ch5.Id },
            };

            context.StoryChoices.AddRange(choices);
            context.SaveChanges();
        }
    }
}
