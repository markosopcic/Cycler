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
            int offsetTime;
            var parsed = int.TryParse(offset, out offsetTime);
            return !parsed ? time : time.AddMinutes(-offsetTime);
        } 
        
        public static DateTime UtcFromUser(this DateTime time, ClaimsPrincipal user)
        {
            var offset = user.Identity.GetSpecificClaim("TimeOffset");
            if (offset == null) return time;
            int offsetTime;
            var parsed = int.TryParse(offset, out offsetTime);
            return !parsed ? DateTime.SpecifyKind(time,DateTimeKind.Utc) : DateTime.SpecifyKind(time.AddMinutes(offsetTime),DateTimeKind.Utc);
        } 
    }
}