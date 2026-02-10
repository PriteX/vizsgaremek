using Idopontfoglalo.Core.Models;

namespace Idopontfoglalo.Core.Interfaces;

public interface IAvailabilityService
{
    Task<List<SlotDto>> GetSlotsAsync(int employeeId, int serviceId, DateOnly date);
    Task AddWeeklyAvailabilityAsync(AvailabilityUpsertModel model);
}
