using EventAz.Application.Features.Admin.Statistics;

namespace EventAz.Application.Interfaces;

public interface IAdminStatisticsRepository
{
    Task<AdminStatisticsDto> GetAsync(CancellationToken ct = default);
}
