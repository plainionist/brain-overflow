namespace BrainOverflow.TauriPlugIn;

using static System.Environment;

public class Store
{
    public string Root { get; } = Path.Combine(GetFolderPath(SpecialFolder.Personal), "BrainOverflow");

    public Store()
    {
        if (!Directory.Exists(Root))
        {
            Directory.CreateDirectory(Root);
        }
    }
}