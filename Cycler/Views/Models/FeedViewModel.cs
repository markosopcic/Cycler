using System.Collections.Generic;
using Cycler.Data.Models;

namespace Cycler.Views.Models
{
    public class FeedViewModel
    {
        public List<Data.Models.Event> EventFeed { get; set; }
        
        public List<User> Users { get; set; }
    }
}