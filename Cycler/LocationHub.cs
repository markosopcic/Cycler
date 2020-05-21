using Microsoft.AspNetCore.SignalR;

namespace Cycler
{
    public class LocationHub : Hub
    {
        private void BroadcastLocation(string id,string name,long longitude, long latitude)
        {
            Clients.Group(id).SendCoreAsync("location", new object[] {name, longitude, latitude});
        }
    }
}