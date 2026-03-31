using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.WhisperingWalls;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IWhisperingWallsService
{
    Task<List<StoryArcResponse>> GetStoryArcsAsync();
    Task<StoryChapterResponse> StartStoryAsync(Guid playerId, string storyArc);
    Task<StoryChapterResponse> GetCurrentChapterAsync(Guid playerId, string storyArc);
    Task<MakeChoiceResponse> MakeChoiceAsync(Guid playerId, MakeChoiceRequest request);
    Task<List<PlayerStoryProgressResponse>> GetProgressAsync(Guid playerId);
}

public class WhisperingWallsService : IWhisperingWallsService
{
    private readonly AppDbContext _db;

    public WhisperingWallsService(AppDbContext db) => _db = db;

    public async Task<List<StoryArcResponse>> GetStoryArcsAsync()
    {
        var arcs = await _db.StoryChapters
            .GroupBy(c => c.StoryArc)
            .Select(g => new StoryArcResponse(
                g.Key,
                g.OrderBy(c => c.OrderIndex).First().Title,
                g.Count()))
            .ToListAsync();

        return arcs;
    }

    public async Task<StoryChapterResponse> StartStoryAsync(Guid playerId, string storyArc)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var firstChapter = await _db.StoryChapters
            .Include(c => c.Choices)
            .Where(c => c.StoryArc == storyArc)
            .OrderBy(c => c.OrderIndex)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Story arc '{storyArc}' not found.");

        // Check for existing progress on this story arc
        var existing = await _db.PlayerStoryProgress
            .FirstOrDefaultAsync(p => p.PlayerId == playerId && p.StoryArc == storyArc);

        if (existing != null)
        {
            // Reset progress to restart the story
            existing.CurrentChapterId = firstChapter.Id;
            existing.IsCompleted = false;
            existing.CompletedAt = null;
            existing.StartedAt = DateTime.UtcNow;
        }
        else
        {
            var progress = new PlayerStoryProgress
            {
                PlayerId = playerId,
                CurrentChapterId = firstChapter.Id,
                StoryArc = storyArc
            };
            _db.PlayerStoryProgress.Add(progress);
        }

        await _db.SaveChangesAsync();
        return await MapChapterToResponse(firstChapter, playerId);
    }

    public async Task<StoryChapterResponse> GetCurrentChapterAsync(Guid playerId, string storyArc)
    {
        var progress = await _db.PlayerStoryProgress
            .FirstOrDefaultAsync(p => p.PlayerId == playerId && p.StoryArc == storyArc)
            ?? throw new KeyNotFoundException("No progress found for this story arc. Start the story first.");

        var chapter = await _db.StoryChapters
            .Include(c => c.Choices)
            .FirstOrDefaultAsync(c => c.Id == progress.CurrentChapterId)
            ?? throw new KeyNotFoundException("Current chapter not found.");

        return await MapChapterToResponse(chapter, playerId);
    }

    public async Task<MakeChoiceResponse> MakeChoiceAsync(Guid playerId, MakeChoiceRequest request)
    {
        var choice = await _db.StoryChoices
            .Include(c => c.Chapter)
            .Include(c => c.NextChapter)
                .ThenInclude(nc => nc!.Choices)
            .FirstOrDefaultAsync(c => c.Id == request.ChoiceId)
            ?? throw new KeyNotFoundException("Choice not found.");

        var progress = await _db.PlayerStoryProgress
            .FirstOrDefaultAsync(p => p.PlayerId == playerId && p.StoryArc == choice.Chapter!.StoryArc)
            ?? throw new KeyNotFoundException("No progress found for this story. Start the story first.");

        if (progress.IsCompleted)
            throw new InvalidOperationException("This story is already completed. Start it again to replay.");

        if (progress.CurrentChapterId != choice.ChapterId)
            throw new InvalidOperationException("This choice does not belong to your current chapter.");

        // Check requirements
        var player = await _db.Players
            .Include(p => p.BankItems)
                .ThenInclude(bi => bi.Item)
            .FirstOrDefaultAsync(p => p.Id == playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (choice.RequiredItemName != null)
        {
            var hasItem = player.BankItems.Any(bi => bi.Item != null && bi.Item.Name == choice.RequiredItemName);
            if (!hasItem)
                throw new InvalidOperationException($"You need '{choice.RequiredItemName}' to make this choice.");
        }

        if (choice.MinWisdom.HasValue && player.Wisdom < choice.MinWisdom.Value)
            throw new InvalidOperationException($"You need at least {choice.MinWisdom.Value} Wisdom to make this choice.");

        if (choice.NextChapterId == null)
            throw new InvalidOperationException("This choice leads nowhere.");

        var nextChapter = choice.NextChapter!;

        // Advance progress
        progress.CurrentChapterId = nextChapter.Id;

        int? goldEarned = null;
        int? xpEarned = null;

        if (nextChapter.IsEnding)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;

            if (nextChapter.GoldReward.HasValue)
            {
                player.GoldCoins += nextChapter.GoldReward.Value;
                goldEarned = nextChapter.GoldReward.Value;
            }

            if (nextChapter.XpReward.HasValue)
            {
                player.Experience += nextChapter.XpReward.Value;
                xpEarned = nextChapter.XpReward.Value;
            }
        }

        await _db.SaveChangesAsync();

        var chapterResponse = await MapChapterToResponse(nextChapter, playerId);

        return new MakeChoiceResponse(
            nextChapter.Content,
            nextChapter.IsEnding,
            goldEarned,
            xpEarned,
            chapterResponse);
    }

    public async Task<List<PlayerStoryProgressResponse>> GetProgressAsync(Guid playerId)
    {
        var progress = await _db.PlayerStoryProgress
            .Include(p => p.CurrentChapter)
            .Where(p => p.PlayerId == playerId)
            .OrderByDescending(p => p.StartedAt)
            .ToListAsync();

        return progress.Select(p => new PlayerStoryProgressResponse(
            p.Id,
            p.StoryArc,
            p.CurrentChapterId,
            p.CurrentChapter?.Title ?? "Unknown",
            p.IsCompleted,
            p.StartedAt,
            p.CompletedAt)).ToList();
    }

    private async Task<StoryChapterResponse> MapChapterToResponse(StoryChapter chapter, Guid playerId)
    {
        var player = await _db.Players
            .Include(p => p.BankItems)
                .ThenInclude(bi => bi.Item)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        var choiceResponses = chapter.Choices.Select(c =>
        {
            var isAvailable = true;
            string? hint = null;

            if (c.RequiredItemName != null)
            {
                var hasItem = player?.BankItems.Any(bi => bi.Item != null && bi.Item.Name == c.RequiredItemName) ?? false;
                if (!hasItem)
                {
                    isAvailable = false;
                    hint = $"Requires {c.RequiredItemName}";
                }
            }

            if (c.MinWisdom.HasValue)
            {
                if ((player?.Wisdom ?? 0) < c.MinWisdom.Value)
                {
                    isAvailable = false;
                    hint = $"Requires {c.MinWisdom.Value} Wisdom";
                }
            }

            return new StoryChoiceResponse(c.Id, c.ChoiceText, isAvailable, hint);
        }).ToList();

        return new StoryChapterResponse(
            chapter.Id,
            chapter.Title,
            chapter.Content,
            chapter.StoryArc,
            chapter.IsEnding,
            chapter.GoldReward,
            chapter.XpReward,
            choiceResponses);
    }
}
