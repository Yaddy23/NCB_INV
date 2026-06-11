using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NCB_INV
{
    public class UserAccount
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Username")]
        public required string Username { get; set; }

        [BsonElement("Password")]
        public required string Password { get; set; }

        [BsonElement("Role")]
        public required string Role { get; set; }

        [BsonElement("DisplayName")]
        public required string DisplayName { get; set; }
    }
}
