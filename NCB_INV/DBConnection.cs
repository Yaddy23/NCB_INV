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


namespace NCB_INV
{
    public static class DBConnection
    {
        private static readonly string connString = Properties.Settings.Default.MongoConn;
        private static readonly IMongoCollection<Book>? _bookCollection;
        private static readonly IMongoDatabase? _database;
        private static readonly string sqliteConn = "Data Source=local_inventory.db";

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

        public static async Task<Book?> ScrapeBookData(string isbn)
        {
            Book? book = await GetFromGoogleBooks(isbn);

            book ??= await GetFromOpenLibrary(isbn);

            return book;
        }

        private static async Task<Book?> GetFromGoogleBooks(string isbn)
        {
            string cleanIsbn = isbn.Replace("-", "").Replace(" ", "").Trim();

            using HttpClient client = new();

            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# Inventory App");

                string url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{cleanIsbn}";
                string response = await client.GetStringAsync(url);

                System.Diagnostics.Debug.WriteLine("API Response: " + response);

                JObject json = JObject.Parse(response);

                if (json["totalItems"]?.Value<int>() > 0)
                {
                    var items = json["items"] as JArray;
                    var firstItem = items?.FirstOrDefault();
                    var info = firstItem?["volumeInfo"];
                    if (info == null) return null;

                    string title = info["title"]?.ToString() ?? "Unknown";
                    string publishedYear = info["publishedDate"]?.ToString()?.Split('-')[0] ?? "N/A";
                    string authors = info["authors"] is JArray authorsArray
                        ? string.Join(", ", authorsArray.Select(a => a.ToString()))
                        : "Unknown";
                    string publisher = info["publisher"]?.ToString() ?? "Unknown";

                    return new Book(
                        cleanIsbn,
                        title,
                        "1st",
                        publishedYear,
                        authors,
                        "Unknown",
                        1,
                        0.00m,
                        publisher,
                        DateTime.UtcNow
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Scraper Error: " + ex.Message);
            }
            return null;
        }

        private static async Task<Book?> GetFromOpenLibrary(string isbn)
        {
            string cleanIsbn = isbn.Replace("-", "").Replace(" ", "").Trim();

            using HttpClient client = new();
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
                        info?["title"]?.ToString() ?? "Unknown",
                        "1st",
                        info?["publish_date"]?.ToString().Split(' ').Last() ?? "N/A",
                        info?["authors"] is JArray authorsArray && authorsArray.Count > 0
                            ? string.Join(", ", authorsArray.Select(a => a?["name"]?.ToString() ?? "Unknown"))
                            : "Unknown",
                        "Unknown",
                        1,
                        0.00m,
                        info?["publishers"] is JArray publishersArray && publishersArray.Count > 0
                            ? publishersArray[0]?["name"]?.ToString() ?? "Unknown"
                            : "Unknown",
                        DateTime.UtcNow
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OpenLibrary Error: " + ex.Message);
            }
            return null;
        }

        // Fetches missing ISBN metadata from external providers.

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
        } 

        private static void LogTransactionLocally(string isbn, string title, int change, string total, string reason, string username)
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();

            string createTable = @"CREATE TABLE IF NOT EXISTS OfflineTransactions (
            ISBN TEXT, Title TEXT, ChangeAmount INTEGER, NewTotal TEXT, 
            Reason TEXT, PerformedBy TEXT, Timestamp DATETIME)";

            using var createCmd = new SqliteCommand(createTable, conn);
            createCmd.ExecuteNonQuery();

            string insertSql = @"INSERT INTO OfflineTransactions 
            (ISBN, Title, ChangeAmount, NewTotal, Reason, PerformedBy, Timestamp) 
            VALUES (@isbn, @title, @change, @total, @reason, @user, @time)";

            using var cmd = new SqliteCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@isbn", isbn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@change", change);
            cmd.Parameters.AddWithValue("@total", total);
            cmd.Parameters.AddWithValue("@reason", reason);
            cmd.Parameters.AddWithValue("@user", username);
            cmd.Parameters.AddWithValue("@time", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public static void LogBulkTransactions(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0) return;

            try
            {
                if (_database != null)
                {
                    var collection = _database.GetCollection<Transaction>("Transactions");
                    collection.InsertMany(transactions);
                    return;
                }

                using (var conn = new SqliteConnection(sqliteConn))
                {
                    conn.Open();
                    using (var sqliteTrans = conn.BeginTransaction())
                    {
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = @"INSERT INTO OfflineTransactions 
                                   (ISBN, Title, ChangeAmount, NewTotal, Reason, PerformedBy, Date, SyncRequired) 
                                   VALUES (@isbn, @title, @change, @total, @reason, @user, @date, 1)";

                        cmd.Parameters.Add("@isbn", SqliteType.Text);
                        cmd.Parameters.Add("@title", SqliteType.Text);
                        cmd.Parameters.Add("@change", SqliteType.Integer);
                        cmd.Parameters.Add("@total", SqliteType.Text);
                        cmd.Parameters.Add("@reason", SqliteType.Text);
                        cmd.Parameters.Add("@user", SqliteType.Text);
                        cmd.Parameters.Add("@date", SqliteType.Text);

                        foreach (var t in transactions)
                        {
                            cmd.Parameters["@isbn"].Value = t.ISBN ?? "";
                            cmd.Parameters["@title"].Value = t.Title ?? "";
                            cmd.Parameters["@change"].Value = t.ChangeAmount;
                            cmd.Parameters["@total"].Value = t.NewTotal ?? "";
                            cmd.Parameters["@reason"].Value = t.Reason ?? "";
                            cmd.Parameters["@user"].Value = t.performedBy ?? "";
                            cmd.Parameters["@date"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            cmd.ExecuteNonQuery();
                        }

                        sqliteTrans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Bulk Log Error: " + ex.Message);
            }
        }

        public static List<Transaction> GetLocalTransactions()
        {
            var transactions = new List<Transaction>();
            using var conn = new SqliteConnection(sqliteConn);

            conn.Open();
            string query = "SELECT * FROM OfflineTransactions";
            using var cmd = new SqliteCommand(query, conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                transactions.Add(new Transaction
                {
                    ISBN = reader["ISBN"].ToString() ?? string.Empty,
                    Title = reader["Title"].ToString() ?? string.Empty,
                    ChangeAmount = Convert.ToInt32(reader["ChangeAmount"]),
                    NewTotal = reader["NewTotal"].ToString() ?? string.Empty,
                    Reason = reader["Reason"].ToString() ?? string.Empty,
                    performedBy = reader["PerformedBy"].ToString() ?? string.Empty,
                    Timestamp = Convert.ToDateTime(reader["Timestamp"])
                });
            }

            return transactions;
        }

        // Tracks stock-in/release events, falling back to SQLite if the cloud is down.

        public static async Task SyncOfflineData()
        {
            if (_database == null || !IsCloudAvailable()) return;

            try
            {
                var localBooks = GetLocalBooks();
                var bookCollection = _bookCollection ?? _database.GetCollection<Book>("Books");

                foreach (var localBook in localBooks)
                {
                    var cloudBook = await bookCollection.Find(b => b.ISBN == localBook.ISBN).FirstOrDefaultAsync();

                    if (cloudBook == null || localBook.LastModified > cloudBook.LastModified)
                    {
                        await bookCollection.ReplaceOneAsync(
                            b => b.ISBN == localBook.ISBN,
                            localBook,
                            new ReplaceOptions { IsUpsert = true }
                        );
                    }
                    else if (cloudBook.LastModified > localBook.LastModified)
                    {
                        SaveToSQLite(cloudBook, isSyncing: true);
                    }
                }

                List<Transaction> pendingLogs = GetLocalTransactions();
                if (pendingLogs.Count > 0)
                {
                    var transCollection = _database.GetCollection<Transaction>("Transactions");
                    await transCollection.InsertManyAsync(pendingLogs);

                    ClearLocalTransactions();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Sync Error: " + ex.Message);
            }
        }

        private static void InitSQLite()
        {
            using var connection = new SqliteConnection(sqliteConn);
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
                    SyncRequired INTEGER DEFAULT 0,
                    LastModified DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE INDEX IF NOT EXISTS idx_title ON OfflineBooks(Title);";
            command.ExecuteNonQuery();
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

        public static async Task ExecuteDeltaSync()
        {
            if (_database == null || !IsCloudAvailable()) return;

            try
            {
                var bookCollection = _bookCollection ?? _database.GetCollection<Book>("Books");
                using var conn = new SqliteConnection(sqliteConn);
                conn.Open();

                string getDirtyQuery = "SELECT * FROM OfflineBooks WHERE SyncRequired = 1";
                using var cmd = new SqliteCommand(getDirtyQuery, conn);
                using var reader = cmd.ExecuteReader();

                var dirtyBooks = new List<Book>();
                while (reader.Read())
                {
                    dirtyBooks.Add(new Book(
                        reader["ISBN"].ToString() ?? "",
                        reader["Title"].ToString() ?? "",
                        reader["Edition"].ToString() ?? "",
                        reader["Year"].ToString() ?? "",
                        reader["Author"].ToString() ?? "",
                        reader["Bind"].ToString() ?? "",
                        Convert.ToInt32(reader["Qty"]),
                        Convert.ToDecimal(reader["Price"]),
                        reader["Publisher"].ToString() ?? "",
                        Convert.ToDateTime(reader["LastModified"])
                    ));
                }
                reader.Close();

                foreach (var book in dirtyBooks)
                {
                    await bookCollection.ReplaceOneAsync(b => b.ISBN == book.ISBN, book, new ReplaceOptions { IsUpsert = true });

                    using var updateCmd = new SqliteCommand(
                        "UPDATE OfflineBooks SET SyncRequired = 0 WHERE ISBN = @isbn AND LastModified <= @mod", conn);
                    updateCmd.Parameters.AddWithValue("@isbn", book.ISBN);
                    updateCmd.Parameters.AddWithValue("@mod", book.LastModified);
                    updateCmd.ExecuteNonQuery();
                }

                var localMaxDate = GetLocalLastModifiedMax();

                var cloudFilter = Builders<Book>.Filter.Gt(b => b.LastModified, localMaxDate);
                var cloudBooks = await bookCollection.Find(cloudFilter).ToListAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var cBook in cloudBooks)
                    {
                        SaveToSQLite(cBook, isSyncing: true);
                    }
                    transaction.Commit();
                }

                System.Diagnostics.Debug.WriteLine($"Sync: Pushed {dirtyBooks.Count}, Pulled {cloudBooks.Count}");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Delta Sync Error: " + ex.Message); }
        }

        private static DateTime GetLocalLastModifiedMax()
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();
            using var cmd = new SqliteCommand("SELECT MAX(LastModified) FROM OfflineBooks", conn);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(result);
        }

        // Manages the connection state and pushes local cached data to MongoDB.

        public static DataTable GetInventory()
        {
            var cloudList = new List<Book>();
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

            var combined = (online
                ? cloudList.Union(localList, new BookIsbnComparer())
                : localList.Union(cloudList, new BookIsbnComparer()))
                .ToList();

            return ToDataTable(combined);
        }

        public static DataTable SearchBooks(string searchTerm)
        {
            var cloudList = new List<Book>();
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
            var books = new List<Book>();
            using var connection = new SqliteConnection(sqliteConn);

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
                        reader["ISBN"].ToString() ?? string.Empty,
                        reader["Title"].ToString() ?? string.Empty,
                        reader["Edition"].ToString() ?? string.Empty,
                        reader["Year"].ToString() ?? string.Empty,
                        reader["Author"].ToString() ?? string.Empty,
                        reader["Bind"].ToString() ?? string.Empty,
                        Convert.ToInt32(reader["Qty"]),
                        Convert.ToDecimal(reader["Price"]),
                        reader["Publisher"].ToString() ?? string.Empty,
                        DateTime.UtcNow
                    ));
                }
            }
            return books;
        }

        public static Book GetBookByISBN(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).FirstOrDefault();
        }

        public static Book? GetLocalBookByISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            var books = GetLocalBooks();
            return books?.Find(b => string.Equals(b.ISBN, isbn, StringComparison.OrdinalIgnoreCase));
        }

        private static void ClearLocalTransactions()
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM OfflineTransactions", conn);
            cmd.ExecuteNonQuery();
        }

        // Retrieves and merges datasets from both local and cloud databases.

        public static bool DoesISBNExist(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).Any();
        }

        private static void SaveToSQLite(Book book, bool isSyncing = false)
        {
            using var connection = new SqliteConnection(sqliteConn);
            connection.Open();
            var cmd = connection.CreateCommand();

            int syncFlag = isSyncing ? 0 : 1;
            DateTime modifiedDate = isSyncing ? book.LastModified : DateTime.UtcNow;

            cmd.CommandText = @"
            INSERT INTO OfflineBooks (ISBN, Title, Edition, Year, Author, Bind, Qty, Price, Publisher, LastModified, SyncRequired)
            VALUES ($isbn, $title, $edition, $year, $author, $bind, $qty, $price, $publisher, $date, $sync)
            ON CONFLICT(ISBN) DO UPDATE SET 
            Title=$title, Edition=$edition, Year=$year, Author=$author, Bind=$bind, 
            Price=$price, Publisher=$publisher, Qty=$qty, LastModified=$date, SyncRequired=$sync;";

            cmd.Parameters.AddWithValue("$isbn", book.ISBN);
            cmd.Parameters.AddWithValue("$title", book.Title);
            cmd.Parameters.AddWithValue("$edition", book.Edition);
            cmd.Parameters.AddWithValue("$year", book.Year);
            cmd.Parameters.AddWithValue("$author", book.Author);
            cmd.Parameters.AddWithValue("$bind", book.Bind);
            cmd.Parameters.AddWithValue("$qty", book.Qty);
            cmd.Parameters.AddWithValue("$price", book.Price);
            cmd.Parameters.AddWithValue("$publisher", book.Publisher);
            cmd.Parameters.AddWithValue("$date", modifiedDate);
            cmd.Parameters.AddWithValue("$sync", syncFlag);

            cmd.ExecuteNonQuery();
        }

        public static void BulkSaveToSQLite(List<Book> books)
        {
            using var connection = new SqliteConnection(sqliteConn);

            connection.Open();
            using var transaction = connection.BeginTransaction();

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

        public static void SaveBook(Book book)
        {
            if (IsCloudAvailable())
            {
                var filter = Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN);
                var collection = _bookCollection ?? _database?.GetCollection<Book>("Books");
                if (collection != null)
                {
                    collection.ReplaceOne(filter, book, new ReplaceOptions { IsUpsert = true });
                    return;
                }

                System.Diagnostics.Debug.WriteLine("SaveBook: cloud collection unavailable, saving locally.");
            }

            SaveToSQLite(book);
            MessageBox.Show("Cloud unavailable. Saved to local database.", "Offline Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void DeleteBook(string isbn)
        {
            _bookCollection.DeleteOne(b => b.ISBN == isbn);
        }

        public static void SyncBookQuantitiesLocal(List<Book> books)
        {
            if (books == null || books.Count == 0)
                return;

            BulkSaveToSQLite(books);
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

                var collection = _bookCollection ?? _database?.GetCollection<Book>("Books");
                if (collection != null)
                {
                    collection.BulkWrite(bulkOps, new BulkWriteOptions { IsOrdered = false });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BulkImportBooks: cloud collection unavailable, saving locally.");
                    BulkSaveToSQLite(books);
                }
            }
            else
            {
                BulkSaveToSQLite(books);
            }
        }

        // Handles single and bulk stock updates across Mongo and SQLite.

        private static DataTable ToDataTable(List<Book> books)
        {
            DataTable dt = new();
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
            public bool Equals(Book? x, Book? y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (x is null || y is null)
                    return false;
                return x.ISBN == y.ISBN;
            }

            public int GetHashCode(Book obj) => obj.ISBN.GetHashCode();
        }

        // Data conversion and deduplication logic for UI binding.

        public static UserAccount? Login(string username, string password)
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

        private static UserAccount? AuthenticateOffline(string username, string hashedPass)
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();
            string query = "SELECT Username, DisplayName FROM UserCache WHERE Username = @user AND PasswordHash = @hash";
            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@user", username);
            cmd.Parameters.AddWithValue("@hash", hashedPass);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserAccount
                {
                    Username = reader["Username"]?.ToString() ?? string.Empty,
                    DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                    Password = hashedPass
                };
            }
            return null;
        }

        private static void UpdateLocalUserCache(string user, string display, string hash)
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();

            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS UserCache (
            Username TEXT PRIMARY KEY,
            DisplayName TEXT,
            PasswordHash TEXT,
            LastSync DATETIME
            );";

            using var createCmd = new SqliteCommand(createTableQuery, conn);
            createCmd.ExecuteNonQuery();

            string upsertQuery = @"
            INSERT OR REPLACE INTO UserCache (Username, DisplayName, PasswordHash, LastSync) 
            VALUES (@user, @display, @hash, @now)";

            using var cmd = new SqliteCommand(upsertQuery, conn);
            cmd.Parameters.AddWithValue("@user", user);
            cmd.Parameters.AddWithValue("@display", display);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public static string HashPassword(string password)
        {
            byte[] bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static class CurrentSession
        {
            public static UserAccount? User { get; set; }
        }

        // Manages user access, hashes passwords, and caches credentials for offline login.

    }
}