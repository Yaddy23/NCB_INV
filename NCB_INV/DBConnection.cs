using System;
using System.Collections.Generic;
using System.Data;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Data.Sqlite;


namespace NCB_INV
{
    public static class DBConnection
    {
        private static string connString = "mongodb+srv://ncbdb_yad:Yadiyadiyad23@ncbinventory.hxsrfot.mongodb.net/?appName=NCBINVENTORY";
        private static IMongoCollection<Book> _bookCollection;
        private static IMongoDatabase _database;
        private static string sqliteConn = "Data Source=local_inventory.db";

        static DBConnection()
        {
            try
            {
                var client = new MongoClient(connString);
                _database = client.GetDatabase("NCB_INVENTORY");
                _bookCollection = _database.GetCollection<Book>("Books");
            }
            catch (Exception ex)
            {
                _database = null;
                Console.WriteLine("Initial Cloud Connection Failed: " + ex.Message);
            }

            InitSQLite();
        }

        public static async Task SyncOfflineData()
        {
            if (!IsCloudAvailable()) return;

            List<Book> offlineBooks = GetLocalBooks();
            if (offlineBooks.Count == 0) return;

            try
            {
                // Use your existing BulkImport logic to push to Atlas
                BulkImportBooks(offlineBooks);

                // If successful, clear the local SQLite table so we don't sync duplicates next time
                using (var connection = new SqliteConnection(sqliteConn))
                {
                    await connection.OpenAsync();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "DELETE FROM OfflineBooks";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sync failed: " + ex.Message);
            }
        }

        private static void InitSQLite()
        {
            using (var connection = new SqliteConnection(sqliteConn))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS OfflineBooks (
                        ISBN TEXT PRIMARY KEY,
                        Title TEXT,
                        Edition TEXT,
                        Year TEXT,
                        Author TEXT,
                        Bind TEXT,
                        Qty INTEGER,
                        Price DECIMAL,
                        Publisher TEXT,
                        SyncRequired INTEGER DEFAULT 1
                    );";
                command.ExecuteNonQuery();
            }
        }

        public static bool IsCloudAvailable()
        {
            if (_database == null)
            {
                return false;
            }
            try
            {
                return _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
            }
            catch
            {
                return false;
            }
        }

        public static DataTable GetInventory()
        {
            List<Book> cloudList = new List<Book>();
            List<Book> localList = new List<Book>();

            if (IsCloudAvailable())
            {
                try { cloudList = _bookCollection.Find(new BsonDocument()).ToList(); }
                catch {  }
            }

            localList = GetLocalBooks();

            var combined = localList.Union(cloudList, new BookIsbnComparer()).ToList();

            return ToDataTable(combined);
        }

        public static DataTable SearchBooks(string searchTerm)
        {
            List<Book> cloudList = new List<Book>();
            List<Book> localList = GetLocalBooks(searchTerm);

            if (IsCloudAvailable())
            {
                var filter = Builders<Book>.Filter.Regex(b => b.Title, new BsonRegularExpression(searchTerm, "i")) |
                             Builders<Book>.Filter.Regex(b => b.ISBN, new BsonRegularExpression(searchTerm, "i"));

                try { cloudList = _bookCollection.Find(filter).ToList(); }
                catch { }
            }

            var combined = localList.Union(cloudList, new BookIsbnComparer()).ToList();
            return ToDataTable(combined);
        }

        private static List<Book> GetLocalBooks(string filter = "")
        {
            List<Book> books = new List<Book>();
            using (var connection = new SqliteConnection(sqliteConn))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                if (string.IsNullOrEmpty(filter))
                    cmd.CommandText = "SELECT * FROM OfflineBooks";
                else
                    cmd.CommandText = "SELECT * FROM OfflineBooks WHERE Title LIKE $f OR ISBN LIKE $f";

                cmd.Parameters.AddWithValue("$f", $"%{filter}%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book(
                            reader["ISBN"].ToString(),
                            reader["Title"].ToString(),
                            reader["Edition"].ToString(),
                            reader["Year"].ToString(),
                            reader["Author"].ToString(),
                            reader["Bind"].ToString(),
                            Convert.ToInt32(reader["Qty"]),
                            Convert.ToDecimal(reader["Price"]),
                            reader["Publisher"].ToString()
                        ));
                    }
                }
            }
            return books;
        }
        public static bool DoesISBNExist(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).Any();
        }

        private static void SaveToSQLite(Book book)
        {
            using (var connection = new SqliteConnection(sqliteConn))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO OfflineBooks (ISBN, Qty) VALUES ($isbn, $qty) 
                    ON CONFLICT(ISBN) DO UPDATE SET 
                    Title=$title, Edition=$edition, Year=$year, Author=$author, 
                    Bind=$bind, Price=$price, Publisher=$publisher;";

                cmd.Parameters.AddWithValue("$isbn", book.ISBN);
                cmd.Parameters.AddWithValue("$title", book.Title);
                cmd.Parameters.AddWithValue("$edition", book.Edition);
                cmd.Parameters.AddWithValue("$year", book.Year);
                cmd.Parameters.AddWithValue("$author", book.Author);
                cmd.Parameters.AddWithValue("$bind", book.Bind);
                cmd.Parameters.AddWithValue("$qty", book.Qty);
                cmd.Parameters.AddWithValue("$price", book.Price);
                cmd.Parameters.AddWithValue("$publisher", book.Publisher);

                cmd.ExecuteNonQuery();
            }
        }

        public static void SaveBook(Book book)
        {
            if (IsCloudAvailable())
            {
                var filter = Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN);
                _bookCollection.ReplaceOne(filter, book, new ReplaceOptions { IsUpsert = true });
            }
            else
            {
                SaveToSQLite(book);
                MessageBox.Show("Cloud unavailable. Saved to local database.", "Offline Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void DeleteBook(string isbn)
        {
            _bookCollection.DeleteOne(b => b.ISBN == isbn);
        }

        public static Book GetBookByISBN(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).FirstOrDefault();
        }

        public static void BulkImportBooks(List<Book> books)
        {
            if (books == null || books.Count == 0) return;

            if (IsCloudAvailable())
            {
                var bulkOps = new List<WriteModel<Book>>();

                foreach (var book in books)
                {
                    var updateModel = new UpdateOneModel<Book>(
                        filter: Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN),
                        update: Builders<Book>.Update
                            .Set(b => b.Title, book.Title)
                            .Set(b => b.Edition, book.Edition)
                            .Set(b => b.Year, book.Year)
                            .Set(b => b.Author, book.Author)
                            .Set(b => b.Bind, book.Bind)
                            .Set(b => b.Price, book.Price)
                            .Set(b => b.Publisher, book.Publisher)
                            .SetOnInsert(b => b.Qty, book.Qty)
                    )
                    { IsUpsert = true };

                    bulkOps.Add(updateModel);
                }

                _bookCollection.BulkWrite(bulkOps, new BulkWriteOptions { IsOrdered = false });
            }
            else
            {
                foreach (var b in books) SaveToSQLite(b);
            }
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

        public class BookIsbnComparer : IEqualityComparer<Book>
        {
            public bool Equals(Book x, Book y) => x.ISBN == y.ISBN;
            public int GetHashCode(Book obj) => obj.ISBN.GetHashCode();
        }
    }
}