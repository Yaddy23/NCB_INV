using System;
using System.Collections.Generic;
using System.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace NCB_INV
{
    public static class DBConnection
    {
        // Your connection string
        private static string connString = "mongodb+srv://ncbdb_yad:Yadiyadiyad23@ncbinventory.hxsrfot.mongodb.net/?appName=NCBINVENTORY";
        private static IMongoCollection<Book> _bookCollection;
        private static IMongoDatabase _database;

        static DBConnection()
        {
            var client = new MongoClient(connString);
            _database = client.GetDatabase("NCB_INVENTORY");
            _bookCollection = _database.GetCollection<Book>("Books");
        }

        public static DataTable GetInventory()
        {
            var list = _bookCollection.Find(new BsonDocument()).ToList();
            return ToDataTable(list);
        }

        public static DataTable SearchBooks(string searchTerm)
        {
            var filter = Builders<Book>.Filter.Regex(b => b.Title, new BsonRegularExpression(searchTerm, "i")) |
                         Builders<Book>.Filter.Regex(b => b.ISBN, new BsonRegularExpression(searchTerm, "i"));

            var list = _bookCollection.Find(filter).ToList();
            return ToDataTable(list);
        }

        public static bool DoesISBNExist(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).Any();
        }

        public static void SaveBook(Book book)
        {
            var filter = Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN);
            _bookCollection.ReplaceOne(filter, book, new ReplaceOptions { IsUpsert = true });
        }

        public static void DeleteBook(string isbn)
        {
            _bookCollection.DeleteOne(b => b.ISBN == isbn);
        }

        public static Book GetBookByISBN(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).FirstOrDefault();
        }

        // IMPROVED: This now uses the collection initialized in the constructor
        public static void BulkImportBooks(List<Book> books)
        {
            if (books == null || books.Count == 0) return;

            // We use ReplaceOneModel for an "Upsert" (Update if exists, Insert if new)
            var bulkOps = new List<WriteModel<Book>>();

            foreach (var book in books)
            {
                var upsertModel = new ReplaceOneModel<Book>(
                    filter: Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN),
                    replacement: book
                )
                { IsUpsert = true };

                bulkOps.Add(upsertModel);
            }

            // This is the fastest way to push 20k records
            // Setting IsOrdered to false makes it even faster
            _bookCollection.BulkWrite(bulkOps, new BulkWriteOptions { IsOrdered = false });
        }

        private static DataTable ToDataTable(List<Book> books)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ISBN");
            dt.Columns.Add("Title");
            dt.Columns.Add("Edition");
            dt.Columns.Add("Year");
            dt.Columns.Add("Author");
            dt.Columns.Add("Bind");
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("Publisher");

            foreach (var b in books)
            {
                dt.Rows.Add(b.ISBN, b.Title, b.Edition, b.Year, b.Author, b.Bind, b.Qty, b.Price, b.Publisher);
            }
            return dt;
        }
    }
}