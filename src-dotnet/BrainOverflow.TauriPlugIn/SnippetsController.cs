namespace BrainOverflow.TauriPlugIn;

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

public class SnippetsController(Repository repository)
{
    public void Save(Snippet snippet)
    {
        var id = snippet.Id ?? Guid.NewGuid().ToString();
        repository.Save(id, snippet.Text);
    }

    public IReadOnlyCollection<SearchResult> Search(SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return [];
        }

        return repository.Query()
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
