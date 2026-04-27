using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace NCB_INV
{
    [BsonIgnoreExtraElements]
    public class Book
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Edition { get; set; }
        public string Year { get; set; }
        public string Author { get; set; }
        public string Bind { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public string Publisher { get; set; }

        public DateTime LastModified { get; set; }

        public Book(string isbn, string title, string edition, string year,
                    string author, string bind, int qty, decimal price, string publisher,DateTime lastModified)
        {
            ISBN = isbn;
            Title = title;
            Edition = edition;
            Year = year;
            Author = author;
            Bind = bind;
            Qty = qty;
            Price = price;
            Publisher = publisher;
            LastModified = lastModified;
        }
    }
}
