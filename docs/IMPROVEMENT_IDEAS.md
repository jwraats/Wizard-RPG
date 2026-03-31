# Wizard RPG — Improvement Ideas & Feature Proposals

This document outlines improvement ideas for the Wizard School RPG, organized as actionable proposals. Each section can be turned into one or more GitHub issues. The focus areas are: **ranking systems**, **house points & departments**, **collaboration features**, and **player acquisition & retention**.

---

## Table of Contents

1. [Ranking System](#1-ranking-system)
2. [House System & House Points](#2-house-system--house-points)
3. [Collaboration & Working Together](#3-collaboration--working-together)
4. [Player Acquisition — Getting More Players](#4-player-acquisition--getting-more-players)
5. [Player Retention — Keeping Players Engaged](#5-player-retention--keeping-players-engaged)
6. [Economy & Progression Improvements](#6-economy--progression-improvements)
7. [Social Features](#7-social-features)
8. [Technical Improvements](#8-technical-improvements)

---

## 1. Ranking System

### 1.1 PvP ELO Rating

**Problem:** Currently, winning a battle gives flat rewards (100 XP, 50 gold). There is no skill-based ranking, so experienced players have no distinction from newcomers beyond level.

**Proposal:**
- Introduce an ELO-style rating for PvP spell duels (starting at 1000)
- Rating changes based on opponent's rating (beating a stronger player gives more points)
- Display rating on player profile and leaderboard
- Use rating for optional matchmaking (suggest fair opponents)

**Implementation notes:**
- Add `eloRating` field to the Player model (default 1000)
- Calculate ELO change in `BattleService` when a battle finishes
- Update `LeaderboardView` to show ranking by ELO

---

### 1.2 Rank Tiers & Badges

**Problem:** Numeric ratings are hard to relate to. Players need visible progression milestones.

**Proposal:**
- Define rank tiers based on ELO ranges:
  | Tier | ELO Range | Badge |
  |------|-----------|-------|
  | Apprentice | 0–999 | 🪄 |
  | Adept | 1000–1199 | ⭐ |
  | Sorcerer | 1200–1399 | 🔮 |
  | Archmage | 1400–1599 | 🧙 |
  | Grand Wizard | 1600+ | 👑 |
- Display tier badge next to player names across the UI
- Announce tier promotions/demotions in real-time via SignalR

---

### 1.3 Seasonal Leaderboards

**Problem:** Permanent leaderboards can feel stale; top players become unreachable.

**Proposal:**
- Introduce monthly or bi-weekly seasons with ELO resets (soft reset: move toward 1000)
- Season-end rewards: exclusive items, gold, or titles for top players
- Historical season archive viewable in the leaderboard
- "Hall of Fame" for past season champions

---

### 1.4 Battle Statistics Dashboard

**Problem:** Players cannot see their win/loss record, favorite spells, or performance trends.

**Proposal:**
- Add a statistics page showing:
  - Total battles played, won, lost, win rate
  - Damage dealt/received totals
  - Most used spells and most effective spells
  - Current win/loss streak
  - Rating history chart over time
- Track battle history with detailed per-turn data

---

## 2. House System & House Points

### 2.1 House/Department Assignment

**Problem:** The game has no house or department system, which is a core element of wizard school themes and a powerful social driver.

**Proposal:**
- On registration (or first login), players choose or are sorted into one of four houses:
  | House | Theme | Color | Bonus |
  |-------|-------|-------|-------|
  | Pyromancers | Fire & Courage | Red | +5% fire spell damage |
  | Frostwardens | Ice & Wisdom | Blue | +5% ice spell damage |
  | Stormcallers | Lightning & Speed | Yellow | +5% lightning spell damage |
  | Earthshapers | Earth & Strength | Green | +5% earth spell damage |
- Sorting can be a fun mini-quiz (personality questions) or player's free choice
- House affiliation is displayed on profile, in battles, and on the leaderboard
- House-themed UI accents (border colors, badge icons)

**Implementation notes:**
- Add `house` field to Player model (enum: Pyromancers, Frostwardens, Stormcallers, Earthshapers)
- Create a `HouseController` and `HouseService` for house-related endpoints
- Add a house sorting quiz view on the frontend

---

### 2.2 House Points System

**Problem:** Individual achievements don't contribute to a larger collective goal.

**Proposal:**
- Players earn house points through various activities:
  | Activity | Points |
  |----------|--------|
  | Win a PvP battle | +10 |
  | Complete a daily quest | +5 |
  | Win a broom race bet | +3 |
  | Reach a new level | +15 |
  | Recruit a new player (referral) | +20 |
  | Complete a dungeon run | +10 |
  | Brew a rare potion | +5 |
- House points are tallied for each house and displayed on a prominent house leaderboard
- Weekly/monthly house cup winner announced with rewards for all members
- "House Points" counter visible in the navigation bar or dashboard

**Implementation notes:**
- Create a `HousePoints` model tracking individual contributions
- Aggregate points per house with efficient queries
- Add a `HouseLeaderboardView` to the frontend
- Use SignalR to push real-time house point updates

---

### 2.3 Inter-House Competitions

**Problem:** Houses need head-to-head events to make the system exciting.

**Proposal:**
- **House Cup**: Monthly competition where all house member activities count toward cumulative points
- **House Wars**: Scheduled PvP events where battles between different houses award bonus points
- **House Challenges**: Weekly mini-objectives (e.g., "Pyromancers: deal 10,000 total fire damage this week")
- **Cross-House Quests**: Special quests that require players from multiple houses to cooperate
- Event calendar visible on the dashboard

---

### 2.4 House Common Room

**Problem:** House members have no space to interact or feel a sense of belonging.

**Proposal:**
- Each house has a "Common Room" page showing:
  - House leaderboard (top contributors this week)
  - House chat (text messages between house members)
  - House announcements from house leaders
  - Upcoming house events and challenges
  - House statistics (total points, average level, total battles won)
- House leaders are the top-contributing members (auto-assigned or voted)

---

## 3. Collaboration & Working Together

### 3.1 Fellowship Group Quests

**Problem:** Fellowships currently only share gold income. There is no gameplay reason to be in a fellowship.

**Proposal:**
- Introduce group quests that require multiple fellowship members to participate:
  - "Fellowship Dungeon Raid" — multiple members explore a dungeon simultaneously
  - "Boss Battles" — a powerful boss that requires 2–4 members to defeat in coordinated turns
  - "Fellowship Challenges" — weekly goals the whole fellowship works toward (e.g., "Win 50 battles collectively")
- Rewards are shared among participants
- Fellowship quest board visible on the Fellowship page

---

### 3.2 Cooperative Boss Battles

**Problem:** All combat is currently 1v1. There is no PvE or group content.

**Proposal:**
- Introduce powerful boss monsters that require teamwork:
  - 2–4 players take turns casting spells at the boss
  - Boss has special mechanics (shields, phases, area attacks)
  - Players can use support spells (heal ally, buff damage, debuff boss)
  - Loot is distributed among participants based on contribution
- Bosses rotate weekly; each boss has unique mechanics and weaknesses
- Boss battle lobby where players can form groups and join

**Implementation notes:**
- Extend the Battle model to support multi-player vs. NPC
- Add support spell types (Heal, Buff, Debuff)
- Create `BossBattleService` with multi-player turn logic
- Use SignalR for real-time updates across all participants

---

### 3.3 Mentorship System

**Problem:** New players may feel lost; experienced players have no way to help.

**Proposal:**
- Players above level 10 can become mentors
- New players are optionally matched with a mentor on registration
- Mentor benefits:
  - Earn bonus XP and gold when their mentee levels up
  - Earn a "Mentor" badge after helping 5 players reach level 5
  - Access to a mentor-exclusive chat
- Mentee benefits:
  - Guided tutorial quests with mentor tips
  - Bonus XP for the first 10 battles while mentored
  - Direct chat with their mentor

---

### 3.4 Trading System

**Problem:** Items can only be obtained through the bank/admin. Players cannot interact economically.

**Proposal:**
- Allow player-to-player item trading:
  - Trade request with items and gold on each side
  - Both players must confirm the trade
  - Trade history for transparency
- Optional: Auction house / marketplace where players list items for sale
- Fellowship members get reduced marketplace fees

---

### 3.5 Fellowship vs. Fellowship Battles

**Problem:** Fellowships have no competitive element against each other.

**Proposal:**
- Introduce "Fellowship Wars" — scheduled events where two fellowships battle:
  - Each member fights one opponent from the rival fellowship
  - Results are tallied (best of 5, best of 7, etc.)
  - Winning fellowship earns gold, XP, and prestige points
- Fellowship rankings based on war wins/losses
- "Fellowship of the Month" leaderboard

---

## 4. Player Acquisition — Getting More Players

### 4.1 Improved Onboarding & Tutorial

**Problem:** New players land on the dashboard with no guidance on what to do.

**Proposal:**
- Add a guided tutorial for new players:
  - Step-by-step introduction: create profile → learn about houses → first battle → join fellowship
  - Interactive tooltips highlighting each feature on first visit
  - "Starter Quest" chain that rewards gold and items for completing onboarding steps
  - Optional skip button for returning/experienced players

---

### 4.2 Referral Rewards Program

**Problem:** Referral codes exist but there is no meaningful incentive to share them.

**Proposal:**
- Enhance the referral system with tiered rewards:
  | Referrals | Reward |
  |-----------|--------|
  | 1 | 100 bonus gold |
  | 3 | Exclusive "Recruiter" badge |
  | 5 | Rare item (e.g., Golden Wand) |
  | 10 | "Grand Recruiter" title + unique robe |
  | 25 | Legendary item + permanent gold bonus |
- Referral tracking dashboard showing who you've recruited
- Both referrer and referee get a welcome bonus
- House points awarded for successful referrals

---

### 4.3 Social Media Sharing

**Problem:** Players have no easy way to share their achievements or invite friends.

**Proposal:**
- Add "Share" buttons for:
  - Battle victories (with narrative excerpt)
  - Level-up milestones
  - Rare item acquisitions
  - Leaderboard rankings
- Generate shareable cards/images with player stats
- Open Graph meta tags for rich link previews when sharing URLs
- Referral link integrated into share messages

---

### 4.4 Public Landing Page

**Problem:** The game needs a compelling entry point for new visitors.

**Proposal:**
- Create a public landing page (before login) showcasing:
  - Game features with animated illustrations
  - Live statistics (total players, battles today, top houses)
  - Recent battle highlights and narratives
  - Player testimonials / screenshots
  - Clear "Play Now" call-to-action
  - Quick demo or preview mode

---

### 4.5 Guest / Demo Mode

**Problem:** Requiring registration before trying the game creates a barrier.

**Proposal:**
- Allow visitors to play a quick demo battle without registering
- Demo mode: pre-built character, one battle against AI, see the narrative
- After demo: prompt to register and keep progress
- Reduces friction and lets players experience the core gameplay immediately

---

## 5. Player Retention — Keeping Players Engaged

### 5.1 Daily Login Rewards

**Problem:** There is no incentive to log in daily.

**Proposal:**
- Consecutive login reward calendar:
  - Day 1: 10 gold
  - Day 2: 20 gold
  - Day 3: 50 gold
  - Day 5: Random item
  - Day 7: Rare item + 200 gold
  - Day 14: Exclusive monthly item
  - Day 30: Legendary reward
- Streak resets if a day is missed (or give 1-day grace period)
- Visual calendar showing progress and upcoming rewards

---

### 5.2 Daily & Weekly Quests

**Problem:** After battling a few times, there is little structured direction for players.

**Proposal:**
- Daily quests (reset every 24 hours):
  - "Win 2 battles" — 50 gold
  - "Place a broom race bet" — 20 gold
  - "Deposit 100 gold in the bank" — 30 gold
- Weekly quests (reset every Monday):
  - "Win 10 battles" — 300 gold + rare item
  - "Earn 500 house points" — exclusive badge
  - "Recruit a new player" — 200 gold
- Quest log visible on the dashboard

**Implementation notes:**
- Create Quest model (type, target, reward, expiry)
- Create `QuestService` to track progress and award rewards
- Add `QuestView` to the frontend dashboard

---

### 5.3 Achievement System

**Problem:** There are no long-term goals beyond leveling up.

**Proposal:**
- Achievements for various milestones:
  - "First Blood" — Win your first battle
  - "Spell Master" — Use every spell at least once
  - "Gold Hoarder" — Accumulate 10,000 gold
  - "Social Butterfly" — Join a fellowship
  - "Mentor" — Help 5 new players reach level 5
  - "House Champion" — Be top contributor in your house for a week
  - "Legendary Duelist" — Reach Grand Wizard rank
- Achievements displayed on profile
- Rare achievements unlock exclusive cosmetic items

---

### 5.4 Seasonal Events

**Problem:** The game world feels static without time-limited content.

**Proposal:**
- Regular themed events:
  - **Halloween Spooktacular**: Special dark magic spells, spooky boss, costume items
  - **Winter Solstice Festival**: Ice-themed quests, gift exchange between players
  - **Spring Tournament**: Double house points, bracket-style PvP tournament
  - **Summer Broom Cup**: Special broom racing league with enhanced rewards
- Events run for 1–2 weeks with exclusive rewards
- Event countdown timer on the dashboard

---

### 5.5 Notification System

**Problem:** Players don't know when things happen unless they're actively looking.

**Proposal:**
- In-game notification center:
  - Battle challenges received
  - Fellowship invitations
  - Quest completion
  - House point milestones
  - Broom race results
  - Level-up congratulations
- Optional email notifications for important events
- Push notifications (PWA) for mobile users
- Real-time notification bell in the navigation bar

---

## 6. Economy & Progression Improvements

### 6.1 Item Equipment System

**Problem:** Items exist in the bank but don't affect gameplay. Item stat bonuses are not used in battle.

**Proposal:**
- Allow players to equip items (wand, robe, hat, amulet, broom) from their bank
- Equipped items apply their stat bonuses to the player during battles
- Equipment slots visible on the profile page
- Create item sets that provide bonus effects when all pieces are equipped

---

### 6.2 Spell Progression

**Problem:** All spells are static. There is no way to improve or customize spells.

**Proposal:**
- Spells gain experience with use and can be leveled up
- Higher-level spells deal more damage or cost less mana
- Unlock new spells at certain player levels
- Allow spell customization (choose between damage boost or mana cost reduction)

---

### 6.3 Skill Trees

**Problem:** Character progression is purely linear (XP → level). There are no meaningful choices.

**Proposal:**
- Introduce skill trees for each element:
  - Fire tree: Damage amplification, area effects
  - Ice tree: Defensive abilities, mana efficiency
  - Lightning tree: Speed bonuses, critical hit chance
  - Earth tree: Health bonuses, damage resistance
- Skill points earned on level-up
- Respec option available for gold

---

## 7. Social Features

### 7.1 Global Chat

**Problem:** Players cannot communicate with each other in-game.

**Proposal:**
- Add a global chat channel accessible from any page
- House-specific chat channels
- Fellowship chat (already suggested in fellowship features)
- Direct messages between players
- Chat moderation tools for admins

**Implementation notes:**
- Leverage existing SignalR infrastructure (GameHub)
- Create a `ChatMessage` model with sender, channel, content, timestamp
- Add a chat sidebar/panel component to the frontend

---

### 7.2 Player Profiles & Friends

**Problem:** Players cannot view other players' profiles or build a friends list.

**Proposal:**
- Public player profiles showing:
  - Level, rank tier, house, ELO rating
  - Battle statistics
  - Equipped items
  - Achievements
  - Fellowship membership
- Friend system: send friend requests, view friends list
- "Challenge" button directly from a player's profile
- Activity feed showing friends' recent achievements

---

### 7.3 Spectator Mode

**Problem:** Battles are private between two players. Others cannot watch.

**Proposal:**
- Allow other players to spectate ongoing battles in real-time
- Spectators can cheer (react with emojis) during battles
- Featured battles on the dashboard (highest-ranked players)
- Battle replays viewable after the match ends

---

## 8. Technical Improvements

### 8.1 Mobile-Responsive Design

**Problem:** While Tailwind provides some responsiveness, the game should be fully optimized for mobile.

**Proposal:**
- Audit and improve mobile layouts for all views
- Touch-friendly battle interface
- Progressive Web App (PWA) support for mobile installation
- Offline mode for viewing profile and achievements

---

### 8.2 Real-Time Notifications via SignalR

**Problem:** SignalR is used for battles but could power more features.

**Proposal:**
- Extend SignalR to support:
  - Live house points ticker
  - Chat messages
  - Friend activity notifications
  - Quest completion alerts
  - Broom race live commentary
- Reduce polling by using WebSocket events for all real-time data

---

### 8.3 API Rate Limiting & Security

**Problem:** The API needs protection against abuse.

**Proposal:**
- Implement rate limiting on all endpoints
- Add CAPTCHA for registration to prevent bot accounts
- Input validation and sanitization on all user-provided data
- Implement account lockout after failed login attempts

---

### 8.4 Analytics & Monitoring

**Problem:** No visibility into player behavior or game health.

**Proposal:**
- Track key metrics:
  - Daily active users (DAU), monthly active users (MAU)
  - Average session duration
  - Most played features
  - Player retention curves
  - Battle completion rates
- Admin dashboard with charts and graphs
- Alert system for anomalies (e.g., suspicious gold generation)

---

## Priority Matrix

| Proposal | Impact | Effort | Priority |
|----------|--------|--------|----------|
| House System (2.1, 2.2) | 🔴 High | 🟡 Medium | ⭐ P1 |
| Ranking System (1.1, 1.2) | 🔴 High | 🟢 Low | ⭐ P1 |
| Daily Quests (5.2) | 🔴 High | 🟡 Medium | ⭐ P1 |
| Improved Onboarding (4.1) | 🔴 High | 🟢 Low | ⭐ P1 |
| Referral Rewards (4.2) | 🟡 Medium | 🟢 Low | ⭐ P1 |
| Fellowship Group Quests (3.1) | 🔴 High | 🟡 Medium | P2 |
| Cooperative Boss Battles (3.2) | 🔴 High | 🔴 High | P2 |
| Item Equipment System (6.1) | 🔴 High | 🟡 Medium | P2 |
| Daily Login Rewards (5.1) | 🟡 Medium | 🟢 Low | P2 |
| Achievement System (5.3) | 🟡 Medium | 🟡 Medium | P2 |
| Global Chat (7.1) | 🟡 Medium | 🟡 Medium | P2 |
| Player Profiles & Friends (7.2) | 🟡 Medium | 🟡 Medium | P2 |
| Public Landing Page (4.4) | 🟡 Medium | 🟡 Medium | P2 |
| Social Media Sharing (4.3) | 🟡 Medium | 🟢 Low | P3 |
| Guest / Demo Mode (4.5) | 🟡 Medium | 🟡 Medium | P3 |
| Inter-House Competitions (2.3) | 🟡 Medium | 🟡 Medium | P3 |
| House Common Room (2.4) | 🟡 Medium | 🔴 High | P3 |
| Trading System (3.4) | 🟡 Medium | 🟡 Medium | P3 |
| Fellowship Wars (3.5) | 🟡 Medium | 🔴 High | P3 |
| Mentorship System (3.3) | 🟢 Low | 🟡 Medium | P3 |
| Seasonal Events (5.4) | 🟡 Medium | 🔴 High | P3 |
| Notification System (5.5) | 🟡 Medium | 🟡 Medium | P3 |
| Spectator Mode (7.3) | 🟢 Low | 🟡 Medium | P4 |
| Spell Progression (6.2) | 🟡 Medium | 🔴 High | P4 |
| Skill Trees (6.3) | 🟡 Medium | 🔴 High | P4 |
| Seasonal Leaderboards (1.3) | 🟢 Low | 🟡 Medium | P4 |
| Battle Stats Dashboard (1.4) | 🟢 Low | 🟡 Medium | P4 |
| Mobile PWA (8.1) | 🟡 Medium | 🟡 Medium | P4 |
| Analytics (8.4) | 🟢 Low | 🟡 Medium | P4 |

---

## Quick Wins (Can Be Done This Week)

1. **Add ELO rating field** to Player model and calculate on battle finish
2. **Add house enum** to Player model with selection on registration
3. **Enhance referral rewards** with bonus gold for recruiter and recruit
4. **Add a "Getting Started" guide** to the dashboard for new players
5. **Enable item equipping** so bank items affect battle stats

---

## Long-Term Vision

The ultimate goal is to transform Wizard RPG from a solo battle game into a thriving wizard school community where:

- **Houses compete** for the monthly House Cup, driving team spirit
- **Rankings provide motivation** through clear progression tiers
- **Collaborative content** (boss battles, group quests) encourages teamwork
- **Social features** (chat, friends, spectating) make the game a hangout
- **Regular events** keep the world fresh and exciting
- **Referral and sharing tools** enable organic growth
- **New players** are guided smoothly into the game through tutorials and mentorship

Each feature builds on the existing foundation and can be implemented incrementally without breaking the current gameplay experience.
