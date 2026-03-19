using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BSERVE_API.Services
{
    // Este provedor vai dizer pro SignalR qual Id de usuário usar
    public class UserIdProviderCustom : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Pega o Claim do usuário logado (NameIdentifier)
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}