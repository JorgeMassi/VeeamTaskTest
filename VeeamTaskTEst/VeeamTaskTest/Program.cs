using System;
using System.IO;
using System.Threading;

class Program
{
    private static string source;
    private static string replica;
    private static string logDir;
    private static string logFile;

    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: Program.exe <source_directory> <replica_directory> <log_directory> <sync_interval_seconds>");
            return;
        }

        source = args[0];
        replica = args[1];
        logDir = args[2];
        logFile = Path.Combine(logDir, "log.txt");
        int syncIntervalSeconds;

        if (!int.TryParse(args[3], out syncIntervalSeconds) || syncIntervalSeconds <= 0)
        {
            Console.WriteLine("Invalid sync interval. Please provide a positive integer.");
            return;
        }

        Console.WriteLine($"Source Directory: {source}");
        Console.WriteLine($"Replica Directory: {replica}");
        Console.WriteLine($"Log Directory: {logDir}");
        Console.WriteLine($"Sync Interval: {syncIntervalSeconds} seconds");

        CreateDirectory(source, replica, logDir);


        SynchronizeFolders();

        Timer timer = new Timer(_ => SynchronizeFolders(), null, TimeSpan.Zero, TimeSpan.FromSeconds(syncIntervalSeconds));

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void CreateDirectory(params string[] directories)
    {
        foreach (string directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LogMessage($"Created directory: {directory}");
            }
        }
    }

    private static void SynchronizeFolders()
    {
        try
        {
            MoveContents(source, replica);
            RemoveContents(source, replica);
            LogMessage("Folders synchronized successfully.");
        }
        catch (Exception e)
        {
            LogMessage($"Error synchronizing folders: {e.Message}");
        }
    }

    private static void MoveContents(string source, string replica)
    {
        foreach (string sourceFile in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(sourceFile);
            string replicaFile = Path.Combine(replica, fileName);

            File.Copy(sourceFile, replicaFile, true);
            LogMessage($"Copied: {fileName}");
        }
    }

    private static void RemoveContents(string source, string replica)
    {
        foreach (string replicaFile in Directory.GetFiles(replica))
        {
            string fileName = Path.GetFileName(replicaFile);
            string sourceFile = Path.Combine(source, fileName);
            if (!File.Exists(sourceFile))
            {
                File.Delete(replicaFile);
                LogMessage($"Removed: {fileName}");
            }
        }
    }

    public static void LogMessage(string message)
    {
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(logEntry);
        File.AppendAllText(logFile, logEntry + Environment.NewLine);
    }
}