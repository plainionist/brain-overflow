namespace BrainOverflow.TauriPlugIn;

using TauriDotNetBridge.Contracts;

public class Change
{
    public string? ChangeType { get; set; }
    public string? Path { get; set; }
}

public class GitObserver(IEventPublisher publisher, Repository repository) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var changes = GetChanges();
            if (changes.Any())
            {
                publisher.Publish("store-updates", changes);
            }
        }
    }

    private IReadOnlyCollection<Change> GetChanges()
    {
        Console.Write($"{DateTime.Now}|Checking for changes ...");

        var knownFiles = GetFilesWithHashes();

        repository.Git.Execute("pull --rebase");

        var currentFiles = GetFilesWithHashes();

        var addedFiles = currentFiles.Keys
            .Except(knownFiles.Keys)
            .Select(file => new Change { ChangeType = "Added", Path = file })
            .ToList();

        var modifiedFiles = knownFiles.Keys
            .Intersect(currentFiles.Keys)
            .Where(IsFileModified)
            .Select(file => new Change { ChangeType = "Modified", Path = file })
            .ToList();

        Console.WriteLine($"added: {addedFiles.Count}, modified: {modifiedFiles.Count}");

        return addedFiles.Concat(modifiedFiles).ToList();

        bool IsFileModified(string file)
        {
            knownFiles.TryGetValue(file, out var hash);
            currentFiles.TryGetValue(file, out var newHash);
            return hash != newHash;
        }
    }

    private Dictionary<string, string> GetFilesWithHashes()
    {
        var output = repository.Git.Execute("ls-tree -r HEAD");

        var fileHashes = new Dictionary<string, string>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', 3);
            if (parts.Length >= 3)
            {
                var hashAndFile = parts[2].Split('\t');
                if (hashAndFile.Length == 2)
                {
                    var file = hashAndFile[1];
                    var hash = hashAndFile[0];
                    fileHashes[file] = hash;
                }
            }
        }

        return fileHashes;
    }
}
