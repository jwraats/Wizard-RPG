export const trailerScenes = [
  {
    id: 1,
    prompt:
      'Cinematic sweeping aerial shot of a grand magical wizard school perched on misty mountain cliffs at golden hour. Towering spires glow with arcane energy, enchanted books fly between towers, and a faint aurora ripples across the sky. Epic orchestral music swells. Camera slowly pushes toward the main gate.',
    title: '🏰 A World of Magic Awaits',
    subtitle: '...rendered entirely in plain text, because we blew the graphics budget on fonts.',
    durationSeconds: 18,
    bgGradient: 'from-indigo-900 via-purple-900 to-indigo-950',
    emoji: '🏰',
  },
  {
    id: 2,
    prompt:
      'Intense close-up of two powerful wizards facing off in a dark stone arena. Lightning crackles between their wands. One casts a massive fireball that illuminates the entire coliseum. Sparks and magical particles fill the air. Dramatic slow-motion as spells collide in a blinding explosion of light.',
    title: '⚔️ Epic Wizard Battles',
    subtitle: "You click 'Attack'. The enemy loses 3 HP. Riveting.",
    durationSeconds: 16,
    bgGradient: 'from-red-900 via-orange-900 to-red-950',
    emoji: '⚔️',
  },
  {
    id: 3,
    prompt:
      'High-speed first-person view of a wizard on a racing broomstick weaving through enchanted forest obstacles at breakneck speed. Neon magical trail streams behind. Other racers jostle for position as they barrel through glowing checkpoints. Wind whips robes dramatically. Camera spins during a barrel roll.',
    title: '🧹 Heart-Pounding Broom Racing',
    subtitle: 'Press arrow keys. Dodge a tree. Wow. Such adrenaline. Much speed.',
    durationSeconds: 17,
    bgGradient: 'from-amber-900 via-yellow-900 to-amber-950',
    emoji: '🧹',
  },
  {
    id: 4,
    prompt:
      'A vast underground goblin bank vault door slowly opens to reveal mountains of glittering gold coins, enchanted gemstones, and ancient artifacts. A dragon sleeps atop the hoard. Magical security runes pulse on the walls. Camera dollies through the treasure as coins cascade and shimmer.',
    title: '🏦 Manage Your Fortune',
    subtitle: "It's a number. In a database. You can make it go up. Or down. Thrilling finance.",
    durationSeconds: 15,
    bgGradient: 'from-yellow-900 via-amber-800 to-yellow-950',
    emoji: '🏦',
  },
  {
    id: 5,
    prompt:
      'A fellowship of diverse wizards stands united on a cliff edge at dawn, cloaks billowing in the wind, overlooking an endless magical battlefield. They raise their wands together creating a massive combined spell that shoots into the sky, forming a shield of pure light. Emotional cinematic moment.',
    title: '🤝 Join a Fellowship',
    subtitle: 'Group chat, but with wizard hats. You will still be ghosted.',
    durationSeconds: 18,
    bgGradient: 'from-green-900 via-emerald-900 to-green-950',
    emoji: '🤝',
  },
  {
    id: 6,
    prompt:
      'Fast-paced montage: wizard casting spells, broomstick racing through clouds, gold coins raining from the sky, epic battle explosions, a leaderboard with names climbing ranks. Everything freezes. Camera zooms out to reveal it is all just white text on a dark terminal screen. A cursor blinks. Title card fades in.',
    title: '🎮 Wizard RPG',
    subtitle:
      "Honestly, it's a text-based RPG. But you read this far, so clearly you have nothing better to do. Welcome home.",
    durationSeconds: 20,
    bgGradient: 'from-purple-900 via-indigo-900 to-slate-950',
    emoji: '🎮',
  },
] as const

export type TrailerScene = (typeof trailerScenes)[number]
