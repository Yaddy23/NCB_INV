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
        private static readonly object _dbLock = new();
        private static readonly string connString = Properties.Settings.Default.MongoConn;
        private static readonly IMongoCollection<Book>? _bookCollection;
        private static readonly IMongoDatabase? _database;
        private static readonly string sqliteConn = "Data Source=local_inventory.db;";

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
                client.DefaultRequestHeaders.Add("Yad", "Software");

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

                    string subject = info["categories"]?.ToString() ?? "Unknown";
                    string title = info["title"]?.ToString() ?? "Unknown";
                    string publishedYear = info["publishedDate"]?.ToString()?.Split('-')[0] ?? "N/A";
                    string authors = info["authors"] is JArray authorsArray
                        ? string.Join(", ", authorsArray.Select(a => a.ToString()))
                        : "Unknown";
                    string publisher = info["publisher"]?.ToString() ?? "Unknown";

                    return new Book(
                        subject,
                        cleanIsbn,
                        title,
                        "1st",
                        publishedYear,
                        "0",
                        "Unknown",
                        1,
                        0.00m,
                        "0",
                        DateTime.UtcNow
                    )
                    {
                        AuthorId = authors,
                        PublisherId = publisher
                    };
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
                client.DefaultRequestHeaders.Add("Yad", "Software");

                string url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{cleanIsbn}&format=json&jscmd=data";
                string response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);

                string key = $"ISBN:{cleanIsbn}";

                if (json[key] != null)
                {
                    var info = json[key];

                    return new Book(
                        info?["subjects"] is JArray subjectsArray && subjectsArray.Count > 0
                            ? string.Join(", ", subjectsArray.Select(s => s?["name"]?.ToString() ?? "Unknown"))
                            : "Unknown",
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

        private static string Normalize(string input)
        {
            return input?.Trim().ToLower() ?? "";
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

        // Tracks stock-in/release events, falling back to SQLite if the cloud is down.

        private static void InitSQLite()
        {
            using var connection = new SqliteConnection(sqliteConn);
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    PRAGMA journal_mode=WAL; 
                    PRAGMA synchronous=NORMAL;
                    PRAGMA busy_timeout = 5000";
                cmd.ExecuteNonQuery();
            }

            using var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Publishers (
                    PublisherID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE COLLATE NOCASE
                );

                CREATE TABLE IF NOT EXISTS Authors (
                    AuthorID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE COLLATE NOCASE
                );

                CREATE TABLE IF NOT EXISTS OfflineBooks (
                    ISBN TEXT PRIMARY KEY,
                    Title TEXT,
                    Subject TEXT,
                    Edition TEXT,
                    Year TEXT,
                    Bind TEXT,
                    Qty INTEGER,
                    Price DECIMAL,
                    PublisherID INTEGER,
                    AuthorID INTEGER,
                    SyncRequired INTEGER DEFAULT 0,
                    LastModified DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PublisherID) REFERENCES Publishers(PublisherID),
                    FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID)
                );

                CREATE TABLE IF NOT EXISTS UserCache (
                    Username TEXT PRIMARY KEY,
                    DisplayName TEXT,
                    Role TEXT,
                    PasswordHash TEXT,
                    LastSync DATETIME
                );";

            command.ExecuteNonQuery();
            transaction.Commit();
        }

        private static int GetOrCreateEntity(SqliteConnection conn, SqliteTransaction trans, string tableName, string name)
        {

            if (string.IsNullOrWhiteSpace(name)) return -1;

            string cleanName = Normalize(name);
            string col = tableName.TrimEnd('s') + "ID";

            string selectSql = $"SELECT {col} FROM {tableName} WHERE Name COLLATE NOCASE = @name";

            using var selectCmd = conn.CreateCommand();
            selectCmd.Transaction = trans;
            selectCmd.CommandText = selectSql;
            selectCmd.Parameters.AddWithValue("@name", cleanName);

            var result = selectCmd.ExecuteScalar();
            if (result != null) return Convert.ToInt32(result);

            using var insertCmd = conn.CreateCommand();
            insertCmd.Transaction = trans;
            insertCmd.CommandText = $"INSERT INTO {tableName} (Name) VALUES (@name); SELECT last_insert_rowid();";
            insertCmd.Parameters.AddWithValue("@name", cleanName);

            return Convert.ToInt32(insertCmd.ExecuteScalar());

        } //helper

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

                await PushLocalChangesToCloud(bookCollection);

                var cloudBooks = await bookCollection.Find(new BsonDocument()).ToListAsync();

                using var conn = new SqliteConnection(sqliteConn);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    var mongoAuthors = (await _database.GetCollection<Author>("Authors").Find(_ => true).ToListAsync())
                   .ToDictionary(a => a.Id.ToString(), a => a.Name);
                    var mongoPubs = (await _database.GetCollection<Publisher>("Publishers").Find(_ => true).ToListAsync())
                                       .ToDictionary(p => p.Id.ToString(), p => p.Name);

                    foreach (var cBook in cloudBooks)
                    {
                        string authorName = mongoAuthors.TryGetValue(cBook.AuthorId, out var aName) ? aName : "Unknown";
                        string pubName = mongoPubs.TryGetValue(cBook.PublisherId, out var pName) ? pName : "Unknown";

                        UpdateLocalFromCloudInternal(cBook, conn, transaction, authorName, pubName);
                    }

                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"Delta Sync: {cloudBooks.Count} books processed.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine("Delta Sync Transaction Failed: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Delta Sync General Error: " + ex.Message);
            }
        }

        public static async Task PushLocalChangesToCloud(IMongoCollection<Book> bookCollection)
        {
            if (_database == null) return;

            var authorsColl = _database.GetCollection<Author>("Authors");
            var pubsColl = _database.GetCollection<Publisher>("Publishers");

            var mongoAuthors = (await authorsColl.Find(_ => true).ToListAsync()).ToDictionary(a => a.Name.ToLower().Trim(), a => a);
            var mongoPubs = (await pubsColl.Find(_ => true).ToListAsync()).ToDictionary(p => p.Name.ToLower().Trim(), p => p);

            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();

            string query = @"
                SELECT b.*, a.Name as AuthorName, p.Name as PublisherName 
                FROM OfflineBooks b
                LEFT JOIN Authors a ON b.AuthorID = a.AuthorID
                LEFT JOIN Publishers p ON b.PublisherID = p.PublisherID
                WHERE b.SyncRequired = 1";

            using var cmd = new SqliteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var syncedIsbns = new List<string>();

            while (await reader.ReadAsync())
            {
                try
                {
                    string isbn = reader["ISBN"].ToString()?.Trim() ?? "";
                    string authorName = reader["AuthorName"]?.ToString()?.Trim() ?? "Unknown";
                    string pubName = reader["PublisherName"]?.ToString()?.Trim() ?? "Unknown";

                    var cloudAuthor = await GetOrCreateCloudEntity(authorsColl, mongoAuthors, authorName);
                    var cloudPub = await GetOrCreateCloudEntity(pubsColl, mongoPubs, pubName);

                    var book = new Book(
                        reader["Subject"].ToString() ?? "",
                        isbn,
                        reader["Title"].ToString() ?? "",
                        reader["Edition"].ToString() ?? "",
                        reader["Year"].ToString() ?? "",
                        cloudAuthor.Id.ToString(),
                        reader["Bind"].ToString() ?? "",
                        Convert.ToInt32(reader["Qty"]),
                        Convert.ToDecimal(reader["Price"]),
                        cloudPub.Id.ToString(),
                        Convert.ToDateTime(reader["LastModified"])
                    );

                    var result = await bookCollection.ReplaceOneAsync(
                        b => b.ISBN == book.ISBN,
                        book,
                        new ReplaceOptions { IsUpsert = true }
                    );

                    if (result.IsAcknowledged)
                    {
                        syncedIsbns.Add(isbn);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync book {reader["ISBN"]}: {ex.Message}");
                }
            }

            if (syncedIsbns.Count > 0)
            {
                using var trans = conn.BeginTransaction();
                foreach (var isbn in syncedIsbns)
                {
                    using var updateCmd = new SqliteCommand(
                        "UPDATE OfflineBooks SET SyncRequired = 0 WHERE ISBN = @isbn", conn, trans);
                    updateCmd.Parameters.AddWithValue("@isbn", isbn);
                    await updateCmd.ExecuteNonQueryAsync();
                }
                trans.Commit();
            }
        }

        private static async Task<T> GetOrCreateCloudEntity<T>(IMongoCollection<T> collection, Dictionary<string, T> cache, string name) where T : new()
        {
            string normName = name.ToLower().Trim();
            if (cache.TryGetValue(normName, out var existing)) return existing;

            var entity = new T();
            var prop = typeof(T).GetProperty("Name");
            prop?.SetValue(entity, name);

            await collection.InsertOneAsync(entity);
            cache[normName] = entity;
            return entity;
        }

        private static void UpdateLocalFromCloudInternal(Book book, SqliteConnection conn, SqliteTransaction trans, string authorName, string publisherName)
        {
            int localAuthorId = GetOrCreateEntity(conn, trans, "Authors", authorName);
            int localPubId = GetOrCreateEntity(conn, trans, "Publishers", publisherName);

            using var cmd = conn.CreateCommand();
            cmd.Transaction = trans;

            cmd.CommandText = @"
                INSERT INTO OfflineBooks (Subject, ISBN, Title, Edition, Year, AuthorID, Bind, Qty, Price, PublisherID, LastModified, SyncRequired)
                VALUES ($subject, $isbn, $title, $edition, $year, $authorId, $bind, $qty, $price, $pubId, $date, 0)
                ON CONFLICT(ISBN) DO UPDATE SET 
                Subject=$subject, 
                Title=$title, 
                Edition=$edition, 
                AuthorID=$authorId, 
                Qty=$qty, 
                Price=$price, 
                PublisherID=$pubId, 
                LastModified=$date, 
                SyncRequired=0;";

            cmd.Parameters.AddWithValue("$subject", book.Subject ?? "");
            cmd.Parameters.AddWithValue("$isbn", book.ISBN ?? "");
            cmd.Parameters.AddWithValue("$title", book.Title ?? "");
            cmd.Parameters.AddWithValue("$edition", book.Edition ?? "");
            cmd.Parameters.AddWithValue("$year", book.Year ?? "");
            cmd.Parameters.AddWithValue("$authorId", localAuthorId);
            cmd.Parameters.AddWithValue("$bind", book.Bind ?? "");
            cmd.Parameters.AddWithValue("$qty", book.Qty);
            cmd.Parameters.AddWithValue("$price", book.Price);
            cmd.Parameters.AddWithValue("$pubId", localPubId);
            cmd.Parameters.AddWithValue("$date", book.LastModified);

            cmd.ExecuteNonQuery();
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

                    var authors = _database.GetCollection<Author>("Authors").Find(new BsonDocument()).ToList();
                    var publishers = _database.GetCollection<Publisher>("Publishers").Find(new BsonDocument()).ToList();

                    var authorDict = authors.ToDictionary(a => a.Id.ToString(), a => a.Name);
                    var pubDict = publishers.ToDictionary(p => p.Id.ToString(), p => p.Name);

                    foreach (var b in cloudList)
                    {
                        b.AuthorName = authorDict.GetValueOrDefault(b.AuthorId, "Unknown").ToUpper();
                        b.PublisherName = pubDict.GetValueOrDefault(b.PublisherId, "Unknown").ToUpper();
                    }

                }
                catch(Exception e) { Console.WriteLine($"Error fetching cloud data: {e.Message}"); }
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

        public static List<Book> GetLocalBooks(string filter = "")
        {
            var books = new List<Book>();
            using var connection = new SqliteConnection(sqliteConn);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
                SELECT b.*, a.Name as AuthorName, p.Name as PublisherName 
                FROM OfflineBooks b
                LEFT JOIN Authors a ON b.AuthorID = a.AuthorID
                LEFT JOIN Publishers p ON b.PublisherID = p.PublisherID";

            if (!string.IsNullOrEmpty(filter))
            {
                cmd.CommandText += " WHERE b.Title LIKE $f OR b.ISBN LIKE $f";
                cmd.Parameters.AddWithValue("$f", $"%{filter}%");
            }

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var book = new Book(
                        reader["Subject"]?.ToString() ?? "",
                        reader["ISBN"]?.ToString() ?? "",
                        reader["Title"]?.ToString() ?? "",
                        reader["Edition"]?.ToString() ?? "",
                        reader["Year"]?.ToString() ?? "",
                        reader["AuthorID"]?.ToString() ?? "0",
                        reader["Bind"]?.ToString() ?? "",
                        Convert.ToInt32(reader["Qty"]),
                        Convert.ToDecimal(reader["Price"]),
                        reader["PublisherID"]?.ToString() ?? "0",
                        Convert.ToDateTime(reader["LastModified"])
                    );
                    book.AuthorName = reader["AuthorName"]?.ToString() ?? "Unknown";
                    book.PublisherName = reader["PublisherName"]?.ToString() ?? "Unknown";

                    books.Add(book);
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
            if (string.IsNullOrWhiteSpace(isbn)) return null;

            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();

            string query = @"
                SELECT b.*, a.Name as AuthorName, p.Name as PublisherName 
                FROM OfflineBooks b
                LEFT JOIN Authors a ON b.AuthorID = a.AuthorID
                LEFT JOIN Publishers p ON b.PublisherID = p.PublisherID
                WHERE b.ISBN = @isbn LIMIT 1";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@isbn", isbn.Trim());

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Book(
                    reader["Subject"].ToString() ?? "",
                    reader["ISBN"].ToString() ?? "",
                    reader["Title"].ToString() ?? "",
                    reader["Edition"].ToString() ?? "",
                    reader["Year"].ToString() ?? "",
                    reader["AuthorName"].ToString() ?? "Unknown",
                    reader["Bind"].ToString() ?? "",
                    Convert.ToInt32(reader["Qty"]),
                    Convert.ToDecimal(reader["Price"]),
                    reader["PublisherName"].ToString() ?? "Unknown",
                    Convert.ToDateTime(reader["LastModified"])
                );
            }
            return null;
        }

        // Retrieves and merges datasets from both local and cloud databases.

        public static bool DoesISBNExist(string isbn)
        {
            return _bookCollection.Find(b => b.ISBN == isbn).Any();
        }

        private static void SaveToSQLite(Book book, bool isSyncing = false)
        {
            lock (_dbLock)
            {
                using var connection = new SqliteConnection(sqliteConn);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    int authorId = GetOrCreateEntity(connection, transaction, "Authors", book.AuthorId);
                    int publisherId = GetOrCreateEntity(connection, transaction, "Publishers", book.PublisherId);

                    int syncFlag = isSyncing ? 0 : 1;
                    DateTime modifiedDate = isSyncing ? book.LastModified : DateTime.UtcNow;
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;

                    cmd.CommandText = @"
                        INSERT INTO OfflineBooks (Subject, ISBN, Title, Edition, Year, AuthorID, Bind, Qty, Price, PublisherID, LastModified, SyncRequired)
                        VALUES ($subject, $isbn, $title, $edition, $year, $authorId, $bind, $qty, $price, $pubId, $date, $sync)
                        ON CONFLICT(ISBN) DO UPDATE SET 
                        Subject=$subject, Title=$title, Edition=$edition, Year=$year, AuthorID=$authorId, Bind=$bind, 
                        Price=$price, PublisherID=$pubId, Qty=$qty, LastModified=$date, SyncRequired=$sync;";

                    cmd.Parameters.AddWithValue("$subject", book.Subject ?? "");
                    cmd.Parameters.AddWithValue("$isbn", book.ISBN ?? "");
                    cmd.Parameters.AddWithValue("$title", book.Title ?? "");
                    cmd.Parameters.AddWithValue("$edition", book.Edition ?? "");
                    cmd.Parameters.AddWithValue("$year", book.Year ?? "");
                    cmd.Parameters.AddWithValue("$authorId", authorId);
                    cmd.Parameters.AddWithValue("$bind", book.Bind ?? "");
                    cmd.Parameters.AddWithValue("$qty", book.Qty);
                    cmd.Parameters.AddWithValue("$price", book.Price);
                    cmd.Parameters.AddWithValue("$pubId", publisherId);
                    cmd.Parameters.AddWithValue("$date", modifiedDate);
                    cmd.Parameters.AddWithValue("$sync", syncFlag);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void BulkSaveToSQLite(List<Book> books)
        {
            lock (_dbLock)
            {
                using var connection = new SqliteConnection(sqliteConn);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;

                    cmd.CommandText = @"INSERT OR REPLACE INTO OfflineBooks 
                        (Subject, ISBN, Title, Edition, Year, AuthorID, Bind, Price, Qty, PublisherID, SyncRequired, LastModified) 
                        VALUES ($subject, $isbn, $title, $edition, $year, $authorId, $bind, $price, $qty, $publisherId, 1, $lastMod)";

                    cmd.Parameters.Add("$subject", SqliteType.Text);
                    cmd.Parameters.Add("$isbn", SqliteType.Text);
                    cmd.Parameters.Add("$title", SqliteType.Text);
                    cmd.Parameters.Add("$edition", SqliteType.Text);
                    cmd.Parameters.Add("$year", SqliteType.Text);
                    cmd.Parameters.Add("$authorId", SqliteType.Integer);
                    cmd.Parameters.Add("$bind", SqliteType.Text);
                    cmd.Parameters.Add("$price", SqliteType.Real);
                    cmd.Parameters.Add("$qty", SqliteType.Integer);
                    cmd.Parameters.Add("$publisherId", SqliteType.Integer);
                    cmd.Parameters.Add("$lastMod", SqliteType.Text);

                    foreach (var b in books)
                    {
                        int authorId = GetOrCreateEntity(connection, transaction, "Authors", b.AuthorId);
                        int publisherId = GetOrCreateEntity(connection, transaction, "Publishers", b.PublisherId);

                        cmd.Parameters["$subject"].Value = b.Subject ?? (object)DBNull.Value;
                        cmd.Parameters["$isbn"].Value = b.ISBN ?? (object)DBNull.Value;
                        cmd.Parameters["$title"].Value = b.Title ?? (object)DBNull.Value;
                        cmd.Parameters["$edition"].Value = b.Edition ?? (object)DBNull.Value;
                        cmd.Parameters["$year"].Value = b.Year ?? (object)DBNull.Value;
                        cmd.Parameters["$authorId"].Value = authorId;
                        cmd.Parameters["$bind"].Value = b.Bind ?? (object)DBNull.Value;
                        cmd.Parameters["$price"].Value = b.Price;
                        cmd.Parameters["$qty"].Value = b.Qty;
                        cmd.Parameters["$publisherId"].Value = publisherId;
                        cmd.Parameters["$lastMod"].Value = b.LastModified.ToString("yyyy-MM-dd HH:mm:ss");

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void SaveBook(Book book, string authorName, string publisherName)
        {
            string cleanAuthor = Normalize(authorName);
            string cleanPub = Normalize(publisherName);

            book.AuthorId = cleanAuthor;
            book.PublisherId = cleanPub;

            if (IsCloudAvailable())
            {
                try
                {
                    var authorColl = _database.GetCollection<Author>("Authors");
                    var publisherColl = _database.GetCollection<Publisher>("Publishers");

                    var author = authorColl.Find(a => a.Name.ToLower() == cleanAuthor).FirstOrDefault();
                    if (author == null)
                    {
                        author = new Author { Name = cleanAuthor };
                        authorColl.InsertOne(author);
                    }

                    var publisher = publisherColl.Find(p => p.Name.ToLower() == cleanPub).FirstOrDefault();
                    if (publisher == null)
                    {
                        publisher = new Publisher { Name = cleanPub };
                        publisherColl.InsertOne(publisher);
                    }

                    var filter = Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN);
                    _bookCollection.ReplaceOne(filter, book, new ReplaceOptions { IsUpsert = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cloud Sync Failed: " + ex.Message);
                }
            }

            lock (_dbLock)
            {
                SaveToSQLite(book);
            }
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
            lock (_dbLock)
            {
                if (books == null || books.Count == 0) return;

                if (IsCloudAvailable())
                {

                    var localZeroQtyBooks = new List<Book>();
                    foreach (var b in books)
                    {
                        localZeroQtyBooks.Add(new Book(
                            b.Subject, b.ISBN, b.Title, b.Edition, b.Year,
                            b.AuthorId, b.Bind, 0,
                            b.Price, b.PublisherId, b.LastModified
                        ));
                    }

                    var bulkOps = new List<WriteModel<Book>>();

                    foreach (var book in books)
                    {
                        var updateModel = new UpdateOneModel<Book>(
                            filter: Builders<Book>.Filter.Eq(b => b.ISBN, book.ISBN),
                            update: Builders<Book>.Update
                                .Set(b => b.Subject, book.Subject)
                                .Set(b => b.Title, book.Title)
                                .Set(b => b.Year, book.Year)
                                .Set(b => b.AuthorId, book.AuthorId)
                                .Set(b => b.Bind, book.Bind)
                                .Set(b => b.Price, book.Price)
                                .Set(b => b.PublisherId, book.PublisherId)
                                .Set(b => b.Qty, book.Qty)
                        )
                        { IsUpsert = true };

                        bulkOps.Add(updateModel);
                    }

                    var collection = _bookCollection ?? _database?.GetCollection<Book>("Books");
                    if (collection != null)
                    {
                        collection.BulkWrite(bulkOps, new BulkWriteOptions { IsOrdered = false });
                        BulkSaveToSQLite(localZeroQtyBooks);
                    }
                    else
                    {
                        MessageBox.Show("BulkImportBooks: cloud collection unavailable, saving locally.");
                        BulkSaveToSQLite(books);
                    }
                }
                else
                {
                    BulkSaveToSQLite(books);
                }
            }
        }


        // Handles single and bulk stock updates across Mongo and SQLite.

        public static void CompactLocalDatabase()
        {
            try
            {
                using var conn = new SqliteConnection(sqliteConn);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "VACUUM;"; 
                cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine("SQLite Compaction Complete.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Compaction Error: " + ex.Message);
            }
        }
        // Rebuilds the database file into a compact version

        private static DataTable ToDataTable(List<Book> books)
        {
            DataTable dt = new();
            dt.Columns.Add("Subject");
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
                dt.Rows.Add(b.Subject, b.ISBN, b.Title, b.Edition, b.Year, b.AuthorName, b.Bind, b.Qty, b.Price, b.PublisherName);
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
            lock (_dbLock)
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

                            UpdateLocalUserCache(user.Username, user.DisplayName, user.Role, hashedpass);
                            return user;
                        }
                    }
                    catch { /* Fall through to offline if cloud fails */ }
                }

                return AuthenticateOffline(trimmedUser, hashedpass);
            }
        }

        private static UserAccount? AuthenticateOffline(string username, string hashedPass)
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();
            string query = "SELECT Username, DisplayName, Role FROM UserCache WHERE Username = @user AND PasswordHash = @hash";
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
                    Role = reader["Role"]?.ToString() ?? string.Empty,
                    Password = hashedPass
                };
            }
            return null;
        }

        public static void UpdateLocalUserCache(string user, string display, string role, string hash)
        {
            using var conn = new SqliteConnection(sqliteConn);
            conn.Open();

            string upsertQuery = @"
            INSERT OR REPLACE INTO UserCache (Username, DisplayName, Role, PasswordHash, LastSync) 
            VALUES (@user, @display, @role, @hash, @now)";

            using var cmd = new SqliteCommand(upsertQuery, conn);
            cmd.Parameters.AddWithValue("@user", user);
            cmd.Parameters.AddWithValue("@display", display);
            cmd.Parameters.AddWithValue("@role", role ?? "User");
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