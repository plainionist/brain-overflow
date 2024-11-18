namespace BrainOverflow.TauriPlugIn;

using TauriDotNetBridge.Contracts;
using System.Diagnostics;
using System.Text;

public class Change
{
    public string? ChangeType { get; set; }
    public string? Path { get; set; }
}

public class GitObserver(IEventPublisher publisher, Store store) : IHostedService
{
    private Dictionary<string, string> myKnownFiles = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Observing '{store.Root}'");

        Console.WriteLine("Reading initial files ...");
        myKnownFiles = GetFilesWithHashes();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var changes = GetChanges();
            publisher.Publish("store-updates", changes);
        }
    }

    private IReadOnlyCollection<Change> GetChanges()
    {
        Console.WriteLine($"{DateTime.Now}|Checking for changes ...");

        ExecuteGitCommand("pull --rebase");

        var currentFiles = GetFilesWithHashes();

        var addedFiles = currentFiles.Keys
            .Except(myKnownFiles.Keys)
            .Select(file => new Change { ChangeType = "Added", Path = file })
            .ToList();

        var modifiedFiles = myKnownFiles.Keys
            .Intersect(currentFiles.Keys)
            .Where(IsFileModified)
            .Select(file => new Change { ChangeType = "Modified", Path = file })
            .ToList();

        myKnownFiles = currentFiles;

        Console.WriteLine($"... added = {addedFiles.Count}, modified = {modifiedFiles.Count}");

        return addedFiles.Concat(modifiedFiles).ToList();

        bool IsFileModified(string file)
        {
            myKnownFiles.TryGetValue(file, out var hash);
            currentFiles.TryGetValue(file, out var newHash);
            return hash != newHash;
        }
    }

    private Dictionary<string, string> GetFilesWithHashes()
    {
        var output = ExecuteGitCommand("ls-tree -r HEAD");

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
    
    private string ExecuteGitCommand(string args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"-C {store.Root} {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var outputBuilder = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            Console.WriteLine($"'git {args}' failed: {error}");
        }

        return outputBuilder.ToString().Trim();
    }
}
