using System.Collections.Generic;
using Cycler.Data.Models;

namespace Cycler.Views.Models
{
    public class FeedViewModel
    {
        public List<Event> EventFeed { get; set; }
        
        public List<User> Users { get; set; }
    }
}