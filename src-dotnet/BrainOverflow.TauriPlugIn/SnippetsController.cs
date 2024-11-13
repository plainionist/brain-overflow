namespace BrainOverflow.TauriPlugIn;

using static System.Environment;

public class Snippet
{
    public string? Id { get; set; }
    public string? Text { get; set; }
}

public class SearchRequest
{
    public string? Text { get; set; }
}

public class SearchResult
{
    public string? Match { get; set; }
    public Snippet? Snippet { get; set; }
}

public class SnippetsController
{
    private static readonly string Store = Path.Combine(GetFolderPath(SpecialFolder.Personal), "BrainOverflow");

    public SnippetsController()
    {
        if (!Directory.Exists(Store))
        {
            Directory.CreateDirectory(Store);
        }
    }

    public void Save(Snippet snippet)
    {
        var id = snippet.Id ?? Guid.NewGuid().ToString();
        File.WriteAllText(Path.Combine(Store, id + ".md"), snippet.Text);
    }

    public IReadOnlyCollection<SearchResult> Search(SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return [];
        }

        return Directory.EnumerateFiles(Store, "*.md")
            .Select(file => new Snippet
            {
                Id = Path.GetFileNameWithoutExtension(file),
                Text = File.ReadAllText(file) ?? ""
            })
            .Select(snippet => new SearchResult
            {
                Match = snippet.Text!.Split(Environment.NewLine)
                    .FirstOrDefault(line => line.Contains(request.Text, StringComparison.OrdinalIgnoreCase)),
                Snippet = snippet
            })
            .Where(result => result.Match is not null)
            .ToList();
    }
}
