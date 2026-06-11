using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NCB_INV
{
    public class Author
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int SqliteId { get; set; }
        public string ?Name { get; set; }
    }

    public class Publisher
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int SqliteId { get; set; }
        public string ?Name { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Book(string subject, string isbn, string title, string edition, string year,
                string authorId, string bind, int qty, decimal price, string publisherId, DateTime lastModified)
    {
        public string Subject { get; set; } = subject;
        public string ISBN { get; set; } = isbn;
        public string Title { get; set; } = title;
        public string Edition { get; set; } = edition;
        public string Year { get; set; } = year;
        public string AuthorId { get; set; } = authorId;
        public string Bind { get; set; } = bind;
        public int Qty { get; set; } = qty;
        public decimal Price { get; set; } = price;
        public string PublisherId { get; set; } = publisherId;
        public DateTime LastModified { get; set; } = lastModified;

        [BsonIgnore]
        public string AuthorName { get; set; } = "Unknown";

        [BsonIgnore]
        public string PublisherName { get; set; } = "Unknown";
    }
}