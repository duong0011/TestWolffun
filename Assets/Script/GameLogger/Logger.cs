using System;
using System.IO;

public static class Logger
{
    private static readonly string LogFilePath = "log.txt";
    private static readonly bool WriteToFile = false; 

    public static void Info(string message)
    {
        string log = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(log);
        if (WriteToFile)
        {
            WriteToLogFile(log);
        }
    }

    public static void Warning(string message)
    {
        string log = $"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(log);
        if (WriteToFile)
        {
            WriteToLogFile(log);
        }
    }

    public static void Error(string message)
    {
        string log = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(log);
        if (WriteToFile)
        {
            WriteToLogFile(log);
        }
    }

    private static void WriteToLogFile(string log)
    {
        try
        {
            File.AppendAllText(LogFilePath, log + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to write to log file: {ex.Message}");
        }
    }
}