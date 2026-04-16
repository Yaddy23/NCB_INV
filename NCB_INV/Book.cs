using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCB_INV
{
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

        // This Constructor is REQUIRED for the "new Book(...)" code to work
        public Book(string isbn, string title, string edition, string year,
                    string author, string bind, int qty, decimal price, string publisher)
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
        }
    }
}
