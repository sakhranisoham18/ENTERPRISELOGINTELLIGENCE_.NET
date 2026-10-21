using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseLogIntelligence.Hubs;

[Authorize]
public sealed class DashboardHub : Hub
{
}
