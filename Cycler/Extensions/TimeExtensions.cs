using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Cycler.Extensions
{
    public static  class TimeExtensions
    {
        public static DateTime ToUserTime(this DateTime time, ClaimsPrincipal user)
        {
            var offset = user.Identity.GetSpecificClaim("TimeOffset");
            if (offset == null) return time;
            Int32 offsetTime;
            var parsed = Int32.TryParse(offset, out offsetTime);
            if (!parsed) return time;
            return time.AddMinutes(offsetTime);
        } 
        
        public static DateTime UtcFromUser(this DateTime time, ClaimsPrincipal user)
        {
            var offset = user.Identity.GetSpecificClaim("TimeOffset");
            if (offset == null) return time;
            Int32 offsetTime;
            var parsed = Int32.TryParse(offset, out offsetTime);
            if (!parsed) return time;
            return time.AddMinutes(-offsetTime);
        } 
    }
}