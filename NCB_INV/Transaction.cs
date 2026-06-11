using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NCB_INV
{
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public required string ISBN { get; set; }
        public required string Title { get; set; }
        public required int ChangeAmount { get; set; }
        public required string NewTotal { get; set; }
        public required string Reason { get; set; }
        public required string PerformedBy { get; set; }
        public required DateTime Timestamp { get; set; }
    }
}
