using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankItem> BankItems => Set<BankItem>();
    public DbSet<BroomLeague> BroomLeagues => Set<BroomLeague>();
    public DbSet<BroomTeam> BroomTeams => Set<BroomTeam>();
    public DbSet<BroomBet> BroomBets => Set<BroomBet>();
    public DbSet<Fellowship> Fellowships => Set<Fellowship>();
    public DbSet<FellowshipMember> FellowshipMembers => Set<FellowshipMember>();
    public DbSet<Battle> Battles => Set<Battle>();
    public DbSet<BattleTurn> BattleTurns => Set<BattleTurn>();
    public DbSet<Spell> Spells => Set<Spell>();
    public DbSet<PotionRecipe> PotionRecipes => Set<PotionRecipe>();
    public DbSet<PotionIngredient> PotionIngredients => Set<PotionIngredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<BrewAttempt> BrewAttempts => Set<BrewAttempt>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Username).IsUnique();
            e.HasIndex(p => p.Email).IsUnique();
            e.HasIndex(p => p.ReferralCode).IsUnique();
            e.Property(p => p.GoldCoins).HasDefaultValue(0L);
            e.Property(p => p.Level).HasDefaultValue(1);
        });

        modelBuilder.Entity<BankAccount>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Player)
             .WithOne(p => p.BankAccount)
             .HasForeignKey<BankAccount>(b => b.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BankItem>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Player)
             .WithMany(p => p.BankItems)
             .HasForeignKey(b => b.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Item)
             .WithMany(i => i.BankItems)
             .HasForeignKey(b => b.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BroomLeague>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.WinnerTeam)
             .WithMany()
             .HasForeignKey(l => l.WinnerTeamId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        modelBuilder.Entity<BroomTeam>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Odds).HasPrecision(10, 2);
            e.HasOne(t => t.League)
             .WithMany(l => l.Teams)
             .HasForeignKey(t => t.LeagueId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BroomBet>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Player)
             .WithMany(p => p.BroomBets)
             .HasForeignKey(b => b.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.League)
             .WithMany(l => l.Bets)
             .HasForeignKey(b => b.LeagueId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Team)
             .WithMany(t => t.Bets)
             .HasForeignKey(b => b.TeamId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Fellowship>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasIndex(f => f.ReferralCode).IsUnique();
            e.HasOne(f => f.Owner)
             .WithMany()
             .HasForeignKey(f => f.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FellowshipMember>(e =>
        {
            e.HasKey(fm => fm.Id);
            e.Property(fm => fm.ContributionPercent).HasPrecision(5, 2);
            e.HasOne(fm => fm.Fellowship)
             .WithMany(f => f.Members)
             .HasForeignKey(fm => fm.FellowshipId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(fm => fm.Player)
             .WithMany(p => p.FellowshipMemberships)
             .HasForeignKey(fm => fm.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Battle>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Challenger)
             .WithMany()
             .HasForeignKey(b => b.ChallengerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Defender)
             .WithMany()
             .HasForeignKey(b => b.DefenderId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Winner)
             .WithMany()
             .HasForeignKey(b => b.WinnerId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        modelBuilder.Entity<BattleTurn>(e =>
        {
            e.HasKey(bt => bt.Id);
            e.HasOne(bt => bt.Battle)
             .WithMany(b => b.Turns)
             .HasForeignKey(bt => bt.BattleId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(bt => bt.Attacker)
             .WithMany()
             .HasForeignKey(bt => bt.AttackerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(bt => bt.Spell)
             .WithMany(s => s.BattleTurns)
             .HasForeignKey(bt => bt.SpellId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PotionRecipe>(e =>
        {
            e.HasKey(r => r.Id);
        });

        modelBuilder.Entity<PotionIngredient>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Price).HasDefaultValue(10L);
        });

        modelBuilder.Entity<RecipeIngredient>(e =>
        {
            e.HasKey(ri => ri.Id);
            e.HasOne(ri => ri.Recipe)
             .WithMany(r => r.Ingredients)
             .HasForeignKey(ri => ri.RecipeId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ri => ri.Ingredient)
             .WithMany(i => i.RecipeIngredients)
             .HasForeignKey(ri => ri.IngredientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BrewAttempt>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Player)
             .WithMany(p => p.BrewAttempts)
             .HasForeignKey(a => a.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Recipe)
             .WithMany(r => r.BrewAttempts)
             .HasForeignKey(a => a.RecipeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizQuestion>(e =>
        {
            e.HasKey(q => q.Id);
        });

        modelBuilder.Entity<QuizAttempt>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Player)
             .WithMany(p => p.QuizAttempts)
             .HasForeignKey(a => a.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
