using MediatR;

namespace EventAz.Application.Features.Admin.Statistics;

public sealed record GetAdminStatisticsQuery : IRequest<AdminStatisticsDto>;
