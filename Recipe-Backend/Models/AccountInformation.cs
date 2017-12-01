using MongoDB.Bson.Serialization.Attributes;

namespace RecipeBackend.Models
{
    public class AccountInformation
    {
        [BsonId]
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] Salt { get; set; }
    }
}
