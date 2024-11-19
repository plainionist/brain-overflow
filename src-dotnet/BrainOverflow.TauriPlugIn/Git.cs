namespace BrainOverflow.TauriPlugIn;

using System.Diagnostics;
using System.Text;

public class Git(string workspace)
{
    public string Execute(string args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"-C {workspace} {args}",
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