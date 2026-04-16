using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace NCB_INV
{
    public static class DBConnection
    {
        private static string connString = "Data Source=NCBDB.db";

        public static DataTable GetInventory()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var con = new SqliteConnection(connString))
                {
                    con.Open();
                    string query = "SELECT * FROM Books";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Loading Database: " + ex.Message);
            }
            return dt;
        }

        public static DataTable SearchBooks(string searchTerm)
        {
            DataTable dt = new DataTable();
            using (var con = new SqliteConnection(connString))
            {
                con.Open();
                string query = "SELECT * FROM Books WHERE Title LIKE @search OR ISBN LIKE @search";
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchTerm + "%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }

            }
            return dt;
        }

        public static bool DoesISBNExist(string isbn)
        {
            using (var con = new SqliteConnection(connString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM Books WHERE ISBN = @isbn";
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@isbn", isbn);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public static void SaveBook(Book book)
        {
            using (var con = new SqliteConnection(connString))
            {
                con.Open();
                string query = @"INSERT OR REPLACE INTO Books (ISBN, Title, Edition, Year, Author, Bind, Qty, Price, Publisher) 
                VALUES (@isbn, @title, @edition, @year, @author, @bind, @qty, @price, @publisher)";

                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@isbn", book.ISBN);
                    cmd.Parameters.AddWithValue("@title", book.Title);
                    cmd.Parameters.AddWithValue("@edition", book.Edition);
                    cmd.Parameters.AddWithValue("@year", book.Year);
                    cmd.Parameters.AddWithValue("@author", book.Author);
                    cmd.Parameters.AddWithValue("@bind", book.Bind);
                    cmd.Parameters.AddWithValue("@qty", book.Qty);
                    cmd.Parameters.AddWithValue("@price", book.Price);
                    cmd.Parameters.AddWithValue("@publisher", book.Publisher);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteBook(string isbn)
        {
            using (var con = new SqliteConnection(connString))
            {
                con.Open();
                string query = "DELETE FROM Books WHERE ISBN = @isbn";
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@isbn", isbn);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
