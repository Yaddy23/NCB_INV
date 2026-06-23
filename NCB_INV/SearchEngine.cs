using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCB_INV
{
    public static class SearchEngine
    {
        private static readonly char[] TitleSplitSeparators = new[] { ' ', '-', '.', ',' };

        public static int GetEditDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s?.Length ?? 0;

            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            for(int i = 0; i < v0.Length; i++) v0[i] = i;

            for(int i = 0; i < s.Length; i++)
            {
                v1[0] = i + 1;
                for(int j = 0; j < t.Length; j++)
                {
                    int cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }
               for(int j = 0; j < v0.Length; j++) v0[j] = v1[j];
            }
           return v1[t.Length];
        }

        public static (List<Book> Results, string Suggestion) FuzzySearch(string query, List<Book> source)
        {
            if (string.IsNullOrWhiteSpace(query)) return (new List<Book>(), null!);
            query = query.ToLower().Trim();

            var matches = source.Where(b =>
                b.Title.ToLower().Contains(query) ||
                b.ISBN.Contains(query) ||
                b.AuthorId.ToLower().Contains(query)
            ).ToList();

            if (matches.Any()) return (matches, null!);

            var bestSuggestion = source
                .AsParallel()
                .Select(b => {
                    var words = b.SearchableWords;

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

            return (new List<Book>(), bestSuggestion?.Book.Title!);
        }
    }

}
