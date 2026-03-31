/* ── Enums (as const objects for erasableSyntaxOnly compatibility) ─── */

export const BattleStatus = {
  Pending: 0,
  Active: 1,
  Finished: 2,
} as const
export type BattleStatus = (typeof BattleStatus)[keyof typeof BattleStatus]

export const BetStatus = {
  Pending: 0,
  Won: 1,
  Lost: 2,
} as const
export type BetStatus = (typeof BetStatus)[keyof typeof BetStatus]

export const LeagueStatus = {
  Upcoming: 0,
  Running: 1,
  Finished: 2,
} as const
export type LeagueStatus = (typeof LeagueStatus)[keyof typeof LeagueStatus]

export const ItemType = {
  Wand: 0,
  Robe: 1,
  Hat: 2,
  Broom: 3,
  Potion: 4,
  Amulet: 5,
} as const
export type ItemType = (typeof ItemType)[keyof typeof ItemType]

export const SpellElement = {
  Fire: 0,
  Ice: 1,
  Lightning: 2,
  Earth: 3,
  Arcane: 4,
} as const
export type SpellElement = (typeof SpellElement)[keyof typeof SpellElement]

/* ── Auth ──────────────────────────────────────────── */

export interface RegisterRequest {
  username: string
  email: string
  password: string
  referralCode?: string
  house?: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface PlayerAuthInfo {
  id: string
  username: string
  email: string
  isAdmin: boolean
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  player: PlayerAuthInfo
}

/* ── Player ────────────────────────────────────────── */

export interface PlayerProfileResponse {
  id: string
  username: string
  email: string
  goldCoins: number
  level: number
  experience: number
  magicPower: number
  strength: number
  wisdom: number
  speed: number
  referralCode: string
  createdAt: string
  isAdmin: boolean
  eloRating: number
  house: string
  rankTier: string
  rankBadge: string
  hasCompletedOnboarding: boolean
  loginStreak: number
}

export interface UpdateProfileRequest {
  username?: string
  email?: string
}

/* ── Battle ────────────────────────────────────────── */

export interface SpellResponse {
  id: string
  name: string
  description: string
  manaCost: number
  baseDamage: number
  effect: string
  element: SpellElement
}

export interface BattleTurnResponse {
  turnNumber: number
  attackerId: string
  attackerUsername: string
  spellName: string
  element: SpellElement
  damageDealt: number
  narrative: string | null
}

export interface BattleResponse {
  id: string
  challengerId: string
  challengerUsername: string
  defenderId: string
  defenderUsername: string
  status: BattleStatus
  winnerId: string | null
  winnerUsername: string | null
  startedAt: string
  finishedAt: string | null
  narratorStory: string | null
  turns: BattleTurnResponse[]
}

export interface ChallengeBattleRequest {
  defenderId: string
}

export interface CastSpellRequest {
  spellId: string
}

/* ── Bank ──────────────────────────────────────────── */

export interface BankAccountResponse {
  playerId: string
  goldBalance: number
  updatedAt: string
}

export interface BankItemResponse {
  id: string
  itemId: string
  itemName: string
  itemDescription: string
  itemType: ItemType
  magicBonus: number
  strengthBonus: number
  wisdomBonus: number
  speedBonus: number
  acquiredAt: string
}

/* ── BroomGame ────────────────────────────────────── */

export interface BroomTeamResponse {
  id: string
  name: string
  odds: number
}

export interface BroomLeagueResponse {
  id: string
  name: string
  startTime: string
  endTime: string
  status: LeagueStatus
  winnerTeamId: string | null
  teams: BroomTeamResponse[]
}

export interface PlaceBetRequest {
  leagueId: string
  teamId: string
  amount: number
}

export interface BroomBetResponse {
  id: string
  leagueId: string
  leagueName: string
  teamId: string
  teamName: string
  amount: number
  status: BetStatus
  payout: number
  placedAt: string
}

/* ── Fellowship ───────────────────────────────────── */

export interface FellowshipMemberResponse {
  id: string
  playerId: string
  username: string
  joinedAt: string
  contributionPercent: number
}

export interface FellowshipResponse {
  id: string
  name: string
  ownerId: string
  ownerUsername: string
  referralCode: string
  goldPerHour: number
  createdAt: string
  members: FellowshipMemberResponse[]
}

export interface CreateFellowshipRequest {
  name: string
}

export interface JoinFellowshipRequest {
  referralCode: string
}

/* ── Item ─────────────────────────────────────────── */

export interface ItemResponse {
  id: string
  name: string
  description: string
  type: ItemType
  magicBonus: number
  strengthBonus: number
  wisdomBonus: number
  speedBonus: number
  price: number
}

/* ── Admin ────────────────────────────────────────── */

export interface AdminPlayerResponse {
  id: string
  username: string
  email: string
  goldCoins: number
  level: number
  experience: number
  isAdmin: boolean
  createdAt: string
}

export interface SetGoldRequest {
  playerId: string
  amount: number
}

export interface SetAdminRequest {
  playerId: string
  isAdmin: boolean
}

export interface GrantItemRequest {
  playerId: string
  itemId: string
}

export interface RevokeItemRequest {
  bankItemId: string
}

export interface CreateItemRequest {
  name: string
  description: string
  type: ItemType
  magicBonus: number
  strengthBonus: number
  wisdomBonus: number
  speedBonus: number
  price: number
}

export interface CreateLeagueRequest {
  name: string
  startTime: string
  endTime: string
  teams: CreateTeamRequest[]
}

export interface CreateTeamRequest {
  name: string
  odds: number
}

export interface ResolveLeagueRequest {
  winnerTeamId: string
}

/* ── Quest ────────────────────────────────────────── */

export const QuestType = {
  Daily: 0,
  Weekly: 1,
} as const
export type QuestType = (typeof QuestType)[keyof typeof QuestType]

export const QuestStatus = {
  Active: 0,
  Completed: 1,
  Expired: 2,
} as const
export type QuestStatus = (typeof QuestStatus)[keyof typeof QuestStatus]

export interface QuestResponse {
  id: string
  title: string
  description: string
  type: QuestType
  status: QuestStatus
  targetCount: number
  currentCount: number
  goldReward: number
  xpReward: number
  expiresAt: string
}

/* ── Achievement ──────────────────────────────────── */

export interface AchievementResponse {
  id: string
  key: string
  name: string
  description: string
  unlockedAt: string
}

/* ── Notification ─────────────────────────────────── */

export interface NotificationResponse {
  id: string
  title: string
  message: string
  type: string
  isRead: boolean
  createdAt: string
}

/* ── Equipment ────────────────────────────────────── */

export interface EquippedItemResponse {
  bankItemId: string
  itemId: string
  name: string
  description: string
  type: ItemType
  magicBonus: number
  strengthBonus: number
  wisdomBonus: number
  speedBonus: number
}

export interface EquipmentSlots {
  wand: EquippedItemResponse | null
  robe: EquippedItemResponse | null
  hat: EquippedItemResponse | null
  amulet: EquippedItemResponse | null
  broom: EquippedItemResponse | null
}

export interface EquipItemRequest {
  bankItemId: string
}

export interface UnequipItemRequest {
  slot: string
}

/* ── House ────────────────────────────────────────── */

export interface HouseLeaderboardEntry {
  house: string
  totalPoints: number
  memberCount: number
}

export interface HousePointsResponse {
  id: string
  playerId: string
  playerUsername: string
  house: string
  points: number
  activity: string
  earnedAt: string
}

export interface SelectHouseRequest {
  house: string
}

/* ── Login Reward ─────────────────────────────────── */

export interface LoginRewardResponse {
  day: number
  goldReward: number
  itemReward: string | null
  loginStreak: number
}

export interface LoginRewardStatus {
  loginStreak: number
  canClaimToday: boolean
  lastClaimDate: string | null
}

/* ── Battle Stats ─────────────────────────────────── */

export interface BattleStatsResponse {
  totalBattles: number
  wins: number
  losses: number
  winRate: number
  totalDamageDealt: number
  totalDamageReceived: number
  mostUsedSpell: string | null
  currentWinStreak: number
  bestWinStreak: number
}
