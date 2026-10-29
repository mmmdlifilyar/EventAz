using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Admin.Statistics;

public sealed class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
{
    private readonly IAdminStatisticsRepository _statistics;
    public GetAdminStatisticsQueryHandler(IAdminStatisticsRepository statistics) => _statistics = statistics;

    public Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken ct)
        => _statistics.GetAsync(ct);
}
