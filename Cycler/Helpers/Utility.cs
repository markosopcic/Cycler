using System;
using MongoDB.Bson;

namespace Cycler.Helpers
{
    public static class Utility
    {
        public static ObjectId? TryParseObjectId(string id)
        {
            try
            {
                return ObjectId.Parse(id);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}