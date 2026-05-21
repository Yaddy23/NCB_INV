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
        public string Name { get; set; }
    }

    public class Publisher
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int SqliteId { get; set; }
        public string Name { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Book
    {
        public string Subject { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Edition { get; set; }
        public string Year { get; set; }
        public string AuthorId { get; set; }
        public string Bind { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public string PublisherId { get; set; }
        public DateTime LastModified { get; set; }

        [BsonIgnore]
        public string AuthorName { get; set; } = "Unknown";

        [BsonIgnore]
        public string PublisherName { get; set; } = "Unknown";

        public Book(string subject, string isbn, string title, string edition, string year,
                    string authorId, string bind, int qty, decimal price, string publisherId, DateTime lastModified)
        {
            Subject = subject;
            ISBN = isbn;
            Title = title;
            Edition = edition;
            Year = year;
            AuthorId = authorId;
            Bind = bind;
            Qty = qty;
            Price = price;
            PublisherId = publisherId;
            LastModified = lastModified;
        }
    }
}