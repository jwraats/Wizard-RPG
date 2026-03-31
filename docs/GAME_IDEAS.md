# Wizard School Mini-Games — Brainstorm

This document outlines text-based mini-game ideas that can be played within the Wizard School RPG on a web browser. Each game fits the magical school theme, works as a text/UI-based experience, and can be implemented with HTML, CSS, and JavaScript.

---

## 1. Spell Duel Arena

**Type:** Turn-based combat  
**Players:** 1v1 (PvP or PvE)  
**Summary:** Two wizards face off in a text-based spell duel. Each turn, players choose from available spells (attack, defend, counter). Spells have elemental types (fire, ice, lightning, arcane) with a rock-paper-scissors style advantage system.

**Core Mechanics:**
- Choose spells from your spellbook each turn
- Mana management (spells cost mana, mana regenerates slowly)
- Elemental advantage system (fire > ice > lightning > fire)
- Health points — first to 0 loses
- Betting gold on the outcome

**Why it fits:** Classic RPG combat translated to text, easy to understand, high replayability.

---

## 2. Potion Brewing Lab

**Type:** Crafting / puzzle  
**Players:** Single-player  
**Summary:** Players combine magical ingredients in the correct order and quantities to brew potions. A recipe book gives hints, but some recipes are secret. Wrong combinations may cause explosions (lose ingredients) or create mystery potions.

**Core Mechanics:**
- Drag-and-drop or click-to-add ingredients to a cauldron
- Temperature and timing controls (heat level, stir direction)
- Recipe discovery system — successful brews unlock recipe entries
- Ingredients can be bought with gold or found through other mini-games
- Brewed potions usable in Spell Duels or tradeable

**Why it fits:** Crafting systems are a natural match for wizard schools; encourages experimentation.

---

## 3. Enchanted Library Quiz

**Type:** Trivia / knowledge  
**Players:** Single-player or multiplayer race  
**Summary:** Answer magical trivia questions to earn house points and gold. Questions come from the "Enchanted Library" and cover spell lore, magical creatures, potion ingredients, and wizard history. Timed rounds add pressure.

**Core Mechanics:**
- Multiple choice and fill-in-the-blank questions
- Timed rounds (faster answers = more points)
- Daily challenge mode with unique questions
- Leaderboard with weekly resets
- Questions get harder as you progress (difficulty tiers)

**Why it fits:** Educational element fits the school theme; easy to expand content over time.

---

## 4. Magical Creature Taming

**Type:** Collection / management sim  
**Players:** Single-player with social features  
**Summary:** Discover, tame, and care for magical creatures. Each creature has stats, moods, and abilities. Feed them, train them, and eventually use them as companions in duels or trade them with other players.

**Core Mechanics:**
- Exploration events to discover new creatures (text-based encounters)
- Feed / train / rest actions with cooldown timers
- Creature mood and loyalty system
- Creatures provide passive bonuses (gold generation, spell boosts)
- Rare creatures from special events

**Why it fits:** Collection mechanics drive engagement; creature care is a natural wizard school activity.

---

## 5. Broom Racing League

**Type:** Betting / strategy  
**Players:** Spectator + betting  
**Summary:** Watch (text-simulated) broom races between AI riders and place bets on the outcome. Each rider has visible stats (speed, agility, broom quality) that hint at performance. Races are narrated in real-time text updates.

**Core Mechanics:**
- View rider stats and odds before placing bets
- Real-time text narration of the race ("Rider 3 takes the lead on the final turn!")
- Bet gold on winner, top-3, or exact finishing order
- Special events with higher stakes and rare rewards
- Fellowship leaderboards for total winnings

**Why it fits:** Already mentioned in the project requirements; adds excitement without requiring complex graphics.

---

## 6. Dungeon Crawler — The Forbidden Corridor

**Type:** Roguelike / exploration  
**Players:** Single-player  
**Summary:** Navigate procedurally generated dungeon floors beneath the school. Each room presents a text-based choice: fight a monster, solve a riddle, find treasure, or flee. Go deeper for better rewards but higher risk. If you fall, you lose your loot.

**Core Mechanics:**
- Room-by-room navigation with choices at each step
- Random encounters (monsters, traps, treasure, NPCs)
- Riddle/puzzle rooms that require text input answers
- Risk/reward — carry loot out or push deeper
- Permadeath per run (roguelike), but persistent character upgrades

**Why it fits:** Roguelike exploration is perfect for text-based gameplay; high replayability with procedural generation.

---

## 7. Wizard Chess

**Type:** Strategy / board game  
**Players:** 1v1 (PvP or vs AI)  
**Summary:** A simplified chess variant where pieces are magical creatures with special abilities. Plays on a smaller board (6x6) with each piece having a unique magical move (e.g., the Phoenix can resurrect once, the Golem can't be captured from the front).

**Core Mechanics:**
- Simplified board (6x6) for faster games
- Each piece type has a unique magical ability (usable once per game)
- Standard movement rules with magical twists
- Ranked matchmaking with ELO-style rating
- Bet gold on matches

**Why it fits:** Board games translate perfectly to web; the magical twist makes it unique.

---

## 8. The Whispering Walls — Text Adventure

**Type:** Interactive fiction / choose-your-own-adventure  
**Players:** Single-player  
**Summary:** Explore mysterious storylines within the school by making choices at key moments. Each story arc has multiple endings, hidden paths, and consequences that affect your character's reputation and relationships with NPCs.

**Core Mechanics:**
- Branching narrative with meaningful choices
- Multiple endings per story arc
- Reputation system (choices affect NPC relationships)
- Hidden paths requiring specific items or spells to unlock
- New story chapters released periodically (live content)

**Why it fits:** Text adventures are the purest form of text-based gaming; perfect for world-building and lore.

---

## Summary Matrix

| Game | Type | Players | Complexity | Gold Sink | Social |
|------|------|---------|------------|-----------|--------|
| Spell Duel Arena | Combat | 1v1 | Medium | Betting | High |
| Potion Brewing Lab | Crafting | Solo | Medium | Ingredients | Low |
| Enchanted Library Quiz | Trivia | Solo/Multi | Low | Entry fee | Medium |
| Magical Creature Taming | Collection | Solo+Social | High | Feeding/Trading | Medium |
| Broom Racing League | Betting | Spectator | Low | Betting | High |
| Dungeon Crawler | Roguelike | Solo | High | Equipment | Low |
| Wizard Chess | Strategy | 1v1 | Medium | Betting | High |
| Whispering Walls | Story | Solo | Low | Items | Low |

---

## Recommended Priority

1. **Spell Duel Arena** — Core RPG combat, high engagement
2. **Broom Racing League** — Already in requirements, quick to build
3. **Dungeon Crawler** — High replayability, drives item/gold economy
4. **Potion Brewing Lab** — Connects crafting to combat system
5. **Enchanted Library Quiz** — Easy to build, good for daily engagement
6. **Whispering Walls** — World-building, can be added incrementally
7. **Magical Creature Taming** — Complex but adds depth long-term
8. **Wizard Chess** — Fun addition once core systems are stable

---

## Wireframes

Interactive HTML wireframes for each game concept are available in the `wireframes/` directory. Open `wireframes/index.html` in a browser to view all wireframes.
