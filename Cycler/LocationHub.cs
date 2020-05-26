using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Cycler
{
    public class LocationHub : Hub
    {
        public void BroadcastPosition(string id,string name,long longitude, long latitude)
        {
            Clients.Group(id).SendCoreAsync("Position", new object[] {name, longitude, latitude});
        }

        public async Task AddNewFollowing(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        
    }
}