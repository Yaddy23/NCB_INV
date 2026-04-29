using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCB_INV
{
    public static class SearchEngine
    {
        public static int GetEditDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s?.Length ?? 0;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public static (List<Book> Results, string Suggestion) FuzzySearch(string query, List<Book> source)
        {
            if (string.IsNullOrWhiteSpace(query)) return (new List<Book>(), null);
            query = query.ToLower().Trim();

            var matches = source.Where(b =>
                b.Title.ToLower().Contains(query) ||
                b.ISBN.Contains(query) ||
                b.Author.ToLower().Contains(query)
            ).ToList();

            if (matches.Any()) return (matches, null);

            var bestSuggestion = source
                .Select(b => {
                    var words = b.Title.ToLower().Split(new[] { ' ', '-', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    double bestWordSimilarity = 0;
                    foreach (var word in words)
                    {
                        int distance = GetEditDistance(query, word);
                        double maxLength = Math.Max(query.Length, word.Length);
                        double similarity = 1.0 - (distance / maxLength);
                        if (similarity > bestWordSimilarity) bestWordSimilarity = similarity;
                    }

                    return new { Book = b, MaxSimilarity = bestWordSimilarity };
                })
                .Where(x => x.MaxSimilarity >= 0.70) 
                .OrderByDescending(x => x.MaxSimilarity)
                .FirstOrDefault();

            return (new List<Book>(), bestSuggestion?.Book.Title);
        }
    }

}
