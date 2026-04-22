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
        public string Id { get; set; }

        public string ISBN { get; set; }
        public string Title { get; set; }
        public int ChangeAmount { get; set; }
        public string NewTotal { get; set; }
        public string Reason { get; set; }
        public string performedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
