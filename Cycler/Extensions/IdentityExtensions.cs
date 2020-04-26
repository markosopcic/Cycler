using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Cycler.Data.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace Cycler.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetSpecificClaim(this IIdentity user, string claimType)
        {
            return GetSpecificClaim((ClaimsIdentity) user,claimType);
        }

        public static ObjectId GetUserId(this IIdentity user)
        {
            return ObjectId.Parse(GetSpecificClaim((ClaimsIdentity) user, ClaimTypes.NameIdentifier));
        }
    }
}