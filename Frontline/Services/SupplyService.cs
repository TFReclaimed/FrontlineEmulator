using Frontline.Data.Entities;
using Frontline.Data.Repositories;

namespace Frontline.Services;

public interface ISupplyService
{
    Task<DateTime> UpdateSupplyAsync(int userId);
    Task<DateTime> UpdateSupplyAsync(PlayerEntity player);
}

public class SupplyService : ISupplyService
{
    private readonly IPlayerRepository _playerRepository;

    private const int MaxAutoSupply = 1000;

    public SupplyService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<DateTime> UpdateSupplyAsync(int userId)
    {
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            throw new Exception($"Player with ID {userId} not found.");
        }

        return await UpdateSupplyAsync(player);
    }

    public async Task<DateTime> UpdateSupplyAsync(PlayerEntity player)
    {
        var lastSupplySync = player.LastSupplySync;

        if (player.Supply >= MaxAutoSupply)
        {
            return lastSupplySync;
        }

        var minutesSinceLastSync = (DateTime.UtcNow - lastSupplySync).TotalMinutes;
        var supplyToAdd = (int) minutesSinceLastSync;
        if (supplyToAdd > 0)
        {
            player.Supply = Math.Min(player.Supply + supplyToAdd, MaxAutoSupply);
            player.LastSupplySync = DateTime.UtcNow;
            await _playerRepository.UpdateAsync(player);
        }

        return player.LastSupplySync;
    }
}