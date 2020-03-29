using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Hash")]
        public string Hash { get; set; }

        [BsonElement("LastLoggedIn")]
        public BsonDateTime LastLoggedIn { get; set; }

        [BsonElement("IncorrectAttempts")]
        public int IncorrectAttempts { get; set; }

        [BsonElement("LoginIp")]
        public int LoginIp { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Created")]
        public BsonDateTime Created { get; set; }
    }
}
