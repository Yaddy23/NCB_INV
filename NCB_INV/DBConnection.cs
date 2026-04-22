using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;


namespace NCB_INV
{
    public static class DBConnection
    {
        private static string connString = Properties.Settings.Default.MongoConn;
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

        public static async Task<Book> ScrapeBookData(string isbn)
        {
            // 1. Try Google Books First
            Book book = await GetFromGoogleBooks(isbn);

            // 2. Fallback to OpenLibrary if Google fails
            if (book == null)
            {
                book = await GetFromOpenLibrary(isbn);
            }

            return book;
        }

        private static async Task<Book> GetFromGoogleBooks(string isbn)
        {
            string cleanIsbn = isbn.Replace("-", "").Replace(" ", "").Trim();

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# Inventory App");

                    string url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{cleanIsbn}";
                    string response = await client.GetStringAsync(url);

                    System.Diagnostics.Debug.WriteLine("API Response: " + response);

                    JObject json = JObject.Parse(response);

                    if (json["totalItems"]?.Value<int>() > 0)
                    {
                        var info = json["items"][0]["volumeInfo"];

                        return new Book(
                            cleanIsbn,
                            info["title"]?.ToString() ?? "Unknown",
                            "1st",
                            info["publishedDate"]?.ToString().Split('-')[0] ?? "N/A",
                            info["authors"] != null ? string.Join(", ", info["authors"]) : "Unknown",
                            "Unknown",
                            1,
                            0.00m,
                            info["publisher"]?.ToString() ?? "Unknown"
                        );
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Scraper Error: " + ex.Message);
                }
                return null;
            }
        } //google api to find book details

        private static async Task<Book> GetFromOpenLibrary(string isbn)
        {
            string cleanIsbn = isbn.Replace("-", "").Replace(" ", "").Trim();

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# Inventory App");

                    string url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{cleanIsbn}&format=json&jscmd=data";
                    string response = await client.GetStringAsync(url);
                    JObject json = JObject.Parse(response);

                    string key = $"ISBN:{cleanIsbn}";

                    if (json[key] != null)
                    {
                        var info = json[key];

                        return new Book(
                            cleanIsbn,
                            info["title"]?.ToString() ?? "Unknown",
                            "1st",
                            info["publish_date"]?.ToString().Split(' ').Last() ?? "N/A",
                            info["authors"] != null ? string.Join(", ", info["authors"].Select(a => a["name"])) : "Unknown",
                            "Unknown",
                            1,
                            0.00m,
                            info["publishers"] != null ? info["publishers"][0]["name"]?.ToString() : "Unknown"
                        );
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("OpenLibrary Error: " + ex.Message);
                }
                return null;
            }
        } //openlibrary api for book details

        public static void LogTransaction(string isbn, string title, int change, string total, string reason, string username)
        {
            try
            {
                if (_database != null)
                {
                    var collection = _database.GetCollection<Transaction>("Transactions");
                    var entry = new Transaction
                    {
                        ISBN = isbn,
                        Title = title,
                        ChangeAmount = change,
                        NewTotal = total,
                        Reason = reason,
                        performedBy = username,
                        Timestamp = DateTime.Now
                    };
                    collection.InsertOne(entry);
                }
                else
                {
                    LogTransactionLocally(isbn, title, change, total, reason, username);
                }
            }
            catch (Exception)
            {
                LogTransactionLocally(isbn, title, change, total, reason, username);
            }
        } //online logs

        private static void LogTransactionLocally(string isbn, string title, int change, string total, string reason, string username)
        {
            using (var conn = new SqliteConnection(sqliteConn))
            {
                conn.Open();

                string createTable = @"CREATE TABLE IF NOT EXISTS OfflineTransactions (
                ISBN TEXT, Title TEXT, ChangeAmount INTEGER, NewTotal TEXT, 
                Reason TEXT, PerformedBy TEXT, Timestamp DATETIME)";

                using (var createCmd = new SqliteCommand(createTable, conn)) { createCmd.ExecuteNonQuery(); }

                string insertSql = @"INSERT INTO OfflineTransactions 
                (ISBN, Title, ChangeAmount, NewTotal, Reason, PerformedBy, Timestamp) 
                VALUES (@isbn, @title, @change, @total, @reason, @user, @time)";

                using (var cmd = new SqliteCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@isbn", isbn);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@change", change);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@reason", reason);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        } //offline logs

        public static void LogBulkTransactions(List<Transaction> transactions)
        {
            try
            {
                if (transactions == null || transactions.Count == 0) return;

                var collection = _database.GetCollection<Transaction>("Transactions");

                collection.InsertMany(transactions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Bulk Log Error: " + ex.Message);
            }
        }// bulk log for transactions

        public static List<Transaction> GetLocalTransactions()
        {
            List<Transaction> transactions = new List<Transaction>();
            using (var conn = new SqliteConnection(sqliteConn))
            {
                conn.Open();
                string query = "SELECT * FROM OfflineTransactions";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                ISBN = reader["ISBN"].ToString(),
                                Title = reader["Title"].ToString(),
                                ChangeAmount = Convert.ToInt32(reader["ChangeAmount"]),
                                NewTotal = reader["NewTotal"].ToString(),
                                Reason = reader["Reason"].ToString(),
                                performedBy = reader["PerformedBy"].ToString(),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"])
                            });
                        }
                    }
                }
            }
            return transactions;
        }

        public static async Task SyncOfflineData()
        {
            if (_database == null) return; // Internet is still out

            try
            {
                List<Book> allPending = GetLocalBooks();
                List<Book> pendingBooks = allPending.Where(b => b.Qty != 0).ToList();

                if (pendingBooks.Count > 0)
                {
                    var bookOps = new List<WriteModel<Book>>();
                    foreach (var book in pendingBooks)
                    {
                        bookOps.Add(new UpdateOneModel<Book>(
                            Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN),
                            Builders<Book>.Update.Inc(b => b.Qty, book.Qty)
                        )
                        { IsUpsert = true });
                    }

                    await _bookCollection.BulkWriteAsync(bookOps);

                    using (var conn = new SqliteConnection(sqliteConn))
                    {
                        conn.Open();
                        using (var trans = conn.BeginTransaction())
                        {
                            var cmd = conn.CreateCommand();

                            var paramNames = pendingBooks.Select((p, i) =>
                            {
                                var name = $"@isbn{i}";
                                cmd.Parameters.AddWithValue(name, p.ISBN);
                                return name;
                            }).ToArray();

                            cmd.CommandText = $"UPDATE OfflineBooks SET Qty = 0 WHERE ISBN IN ({string.Join(",", paramNames)})";
                            cmd.Transaction = trans;
                            cmd.ExecuteNonQuery();

                            trans.Commit();
                        }
                    }
                }

                List<Transaction> pendingLogs = GetLocalTransactions();
                if (pendingLogs.Count > 0)
                {
                    var transCollection = _database.GetCollection<Transaction>("Transactions");
                    await transCollection.InsertManyAsync(pendingLogs);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Sync failed: " + ex.Message);
            }
        }//sync offline changes to cloud db

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
        }//check if cloud db is available

        public static DataTable GetInventory()
        {
            List<Book> cloudList = new List<Book>();
            List<Book> localList = GetLocalBooks();
            bool online = IsCloudAvailable();

            if (online)
            {
                try
                {
                    cloudList = _bookCollection.Find(new BsonDocument()).ToList();
                }
                catch { /* Fallback to offline only if Mongo fails */ }
            }

            List<Book> combined;

            if (online)
            {
                combined = cloudList.Union(localList, new BookIsbnComparer()).ToList();
            }
            else
            {
                combined = localList.Union(cloudList, new BookIsbnComparer()).ToList();
            }

            return ToDataTable(combined);
        }//get inventory from cloud and local db, combine and return as datatable

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
        }//search books in cloud and local db, combine results and return as datatable

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
        }//get local books with optional filter

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
                    INSERT INTO OfflineBooks (ISBN, Title, Edition, Year, Author, Bind, Qty, Price, Publisher, SyncRequired)
                    VALUES ($isbn, $title, $edition, $year, $author, $bind, $qty, $price, $publisher, 1)
                    ON CONFLICT(ISBN) DO UPDATE SET 
                        Title = $title,
                        Edition = $edition,
                        Year = $year,
                        Author = $author,
                        Bind = $bind,
                        Price = $price,
                        Publisher = $publisher,
                        Qty = $qty,
                        SyncRequired = 1;";

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

        public static void BulkSaveToSQLite(List<Book> books)
        {
            using (var connection = new SqliteConnection(sqliteConn))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"INSERT OR REPLACE INTO OfflineBooks 
                    (ISBN, Title, Edition,Year, Author, Bind, Price, Qty, Publisher, SyncRequired) 
                    VALUES ($isbn, $title, $edition, $year, $author, $bind, $price, $qty, $publisher, 1)";

                    cmd.Parameters.Add("$isbn", SqliteType.Text);
                    cmd.Parameters.Add("$title", SqliteType.Text);
                    cmd.Parameters.Add("$edition", SqliteType.Text);
                    cmd.Parameters.Add("$year", SqliteType.Text);
                    cmd.Parameters.Add("$author", SqliteType.Text);
                    cmd.Parameters.Add("$bind", SqliteType.Text);
                    cmd.Parameters.Add("$price", SqliteType.Real);
                    cmd.Parameters.Add("$qty", SqliteType.Integer);
                    cmd.Parameters.Add("$publisher", SqliteType.Text);

                    foreach (var b in books)
                    {
                        cmd.Parameters["$isbn"].Value = b.ISBN;
                        cmd.Parameters["$title"].Value = b.Title;
                        cmd.Parameters["$edition"].Value = b.Edition;
                        cmd.Parameters["$year"].Value = b.Year;
                        cmd.Parameters["$author"].Value = b.Author;
                        cmd.Parameters["$bind"].Value = b.Bind;
                        cmd.Parameters["$price"].Value = b.Price;
                        cmd.Parameters["$qty"].Value = b.Qty;
                        cmd.Parameters["$publisher"].Value = b.Publisher;
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }//bulk save to sqlite for multiple books

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
                            .Set(b => b.Year, book.Year)
                            .Set(b => b.Author, book.Author)
                            .Set(b => b.Bind, book.Bind)
                            .Set(b => b.Price, book.Price)
                            .Set(b => b.Publisher, book.Publisher)
                            .Set(b => b.Qty, book.Qty)
                    )
                    { IsUpsert = true };

                    bulkOps.Add(updateModel);
                }

                _bookCollection.BulkWrite(bulkOps, new BulkWriteOptions { IsOrdered = false });
            }
            else
            {
                BulkSaveToSQLite(books);
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

        public static UserAccount Login(string username, string password)
        {
            string superClean = "";
            foreach (char c in password) { if (char.IsLetterOrDigit(c)) superClean += c; }
            string hashedpass = HashPassword(superClean);
            string trimmedUser = username.Trim();

            if (_database != null)
            {
                try
                {
                    var collection = _database.GetCollection<UserAccount>("Users");
                    var user = collection.Find(u => u.Username == trimmedUser && u.Password == hashedpass).FirstOrDefault();

                    if (user != null)
                    {
                        UpdateLocalUserCache(user.Username, user.DisplayName, hashedpass);
                        return user;
                    }
                }
                catch { /* Fall through to offline if cloud fails */ }
            }

            return AuthenticateOffline(trimmedUser, hashedpass);
        }

        private static UserAccount AuthenticateOffline(string username, string hashedPass)
        {
            using (var conn = new SqliteConnection(sqliteConn))
            {
                conn.Open();
                string query = "SELECT Username, DisplayName FROM UserCache WHERE Username = @user AND PasswordHash = @hash";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@hash", hashedPass);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserAccount
                            {
                                Username = reader["Username"].ToString(),
                                DisplayName = reader["DisplayName"].ToString(),
                                Password = hashedPass 
                            };
                        }
                    }
                }
            }
            return null;
        }

        private static void UpdateLocalUserCache(string user, string display, string hash)
        {
            using (var conn = new SqliteConnection(sqliteConn))
            {
                conn.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS UserCache (
                Username TEXT PRIMARY KEY,
                DisplayName TEXT,
                PasswordHash TEXT,
                LastSync DATETIME
                );";

                using (var createCmd = new SqliteCommand(createTableQuery, conn))
                {
                    createCmd.ExecuteNonQuery();
                }

                string upsertQuery = @"
                INSERT OR REPLACE INTO UserCache (Username, DisplayName, PasswordHash, LastSync) 
                VALUES (@user, @display, @hash, @now)";

                using (var cmd = new SqliteCommand(upsertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.Parameters.AddWithValue("@display", display);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@now", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static class CurrentSession
        {
            public static UserAccount User { get; set; }
        }

        public static void SyncBookQuantitiesLocal(List<Book> books)
        {
            if (books == null || books.Count == 0)
                return;

            BulkSaveToSQLite(books);
        }
        public static Book GetLocalBookByISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            var books = GetLocalBooks();
            return books?.Find(b => string.Equals(b.ISBN, isbn, StringComparison.OrdinalIgnoreCase));
        }
    }
}