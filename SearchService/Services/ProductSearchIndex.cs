using System.Text.Json;
using SearchService.Models;

namespace SearchService.Services;

public sealed class ProductSearchIndex
{
    private readonly List<(Product Product, string NormalizedName)> _entries;
    private readonly Dictionary<string, List<Product>> _exactNameToProducts;
    private readonly Dictionary<string, List<int>> _tokenToEntryIndices;

    private ProductSearchIndex(
        List<(Product Product, string NormalizedName)> entries,
        Dictionary<string, List<Product>> exactNameToProducts,
        Dictionary<string, List<int>> tokenToEntryIndices)
    {
        _entries = entries;
        _exactNameToProducts = exactNameToProducts;
        _tokenToEntryIndices = tokenToEntryIndices;
    }

    public IReadOnlyList<Product> Search(string query)
    {
        var normalizedQuery = Normalize(query);
        if (normalizedQuery.Length == 0)
        {
            return [];
        }

        // Fast-path exact matches.
        if (_exactNameToProducts.TryGetValue(normalizedQuery, out var exactMatches))
        {
            return exactMatches;
        }

        // Substring search. For the common single-token case, use a token index to narrow candidates.
        IEnumerable<int> candidateIndices;
        if (!normalizedQuery.Contains(' ') && _tokenToEntryIndices.TryGetValue(normalizedQuery, out var tokenCandidates))
        {
            candidateIndices = tokenCandidates;
        }
        else
        {
            candidateIndices = Enumerable.Range(0, _entries.Count);
        }

        var matches = new List<Product>();
        foreach (var i in candidateIndices)
        {
            var entry = _entries[i];
            if (entry.NormalizedName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(entry.Product);
            }
        }

        return matches;
    }

    public static ProductSearchIndex LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Products file not found at '{path}'.", path);
        }

        using var stream = File.OpenRead(path);
        var products = JsonSerializer.Deserialize<List<Product>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        var entries = new List<(Product Product, string NormalizedName)>(products.Count);
        var exactNameToProducts = new Dictionary<string, List<Product>>(StringComparer.OrdinalIgnoreCase);
        var tokenToEntryIndices = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

        foreach (var product in products)
        {
            var normalizedName = Normalize(product.Name);
            if (normalizedName.Length == 0)
            {
                continue;
            }

            var entryIndex = entries.Count;
            entries.Add((product, normalizedName));

            if (!exactNameToProducts.TryGetValue(normalizedName, out var list))
            {
                list = [];
                exactNameToProducts[normalizedName] = list;
            }
            list.Add(product);

            foreach (var token in normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!tokenToEntryIndices.TryGetValue(token, out var indices))
                {
                    indices = [];
                    tokenToEntryIndices[token] = indices;
                }

                indices.Add(entryIndex);
            }
        }

        return new ProductSearchIndex(entries, exactNameToProducts, tokenToEntryIndices);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Trim and collapse whitespace (so "A  B" matches "A B").
        ReadOnlySpan<char> span = value.AsSpan().Trim();
        if (span.Length == 0)
        {
            return string.Empty;
        }

        var chars = new char[span.Length];
        var writeIndex = 0;
        var prevWasWhitespace = false;

        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            if (char.IsWhiteSpace(c))
            {
                if (!prevWasWhitespace)
                {
                    chars[writeIndex++] = ' ';
                    prevWasWhitespace = true;
                }

                continue;
            }

            chars[writeIndex++] = c;
            prevWasWhitespace = false;
        }

        return new string(chars, 0, writeIndex);
    }
}
