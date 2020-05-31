using System.Threading.Tasks;
using Cycler.Data.Models;
using Cycler.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Cycler
{
    
    public class LocationHub : Hub
    {

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        [HubMethodName("BroadcastPosition")]
        public  Task BroadcastPosition(string id,string name,string longitude, string latitude)
        {
            var myName  = Context.User.Identity.GetSpecificClaim("FirstName") +
                       Context.User.Identity.GetSpecificClaim("LastName");
            return Clients.Group(id).SendCoreAsync("Position", new object[] {name, longitude, latitude});
        }

        public async Task AddNewFollowing(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        
    }
}