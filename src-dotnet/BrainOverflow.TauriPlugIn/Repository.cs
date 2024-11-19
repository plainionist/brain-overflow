namespace BrainOverflow.TauriPlugIn;

using System.Collections.Generic;
using static System.Environment;

public class Repository
{
    private readonly string myRoot = GetEnvironmentVariable("BRAINOVERFLOW_STORE")
        ?? Path.Combine(GetFolderPath(SpecialFolder.Personal), "BrainOverflow");

    public Repository()
    {
        if (!Directory.Exists(myRoot))
        {
            Directory.CreateDirectory(myRoot);
        }

        Git = new Git(myRoot);
    }

    public Git Git { get; }

    public void Save(string id, string? text)
    {
        File.WriteAllText(Path.Combine(myRoot, id + ".md"), text);

        Git.Execute("add -A");
        Git.Execute("commit -m \".\"");
        Git.Execute("push");
    }

    public IEnumerable<string> Query() =>
        Directory.EnumerateFiles(myRoot, "*.md");
}