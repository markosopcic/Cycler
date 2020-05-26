using System.Collections.Generic;
using Cycler.Data.Models;

namespace Cycler.Views.Models
{
    public class HomeViewModel
    {
        public List<User> ActiveFriends { get; set; }
        public List<Event> ActiveEvents { get; set; }
    }
}