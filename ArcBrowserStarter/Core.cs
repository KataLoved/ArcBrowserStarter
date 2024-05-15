using System.Diagnostics;
using System.Threading;
using System.IO;
using System;

namespace ArcBrowserStarter;

/// <summary>
/// This program is used to fix the Arc Browser when it is not working. (Windows 10)
/// </summary>
public class Core
{
    public static void Main(string[] args)
    {
        Console.Title = "Arc Browser Starter";
        
        var path = GetProblemBrowserFolderPath();
        if (path == string.Empty)
        {
            Console.WriteLine("Arc Browser is working fine.");
            Console.WriteLine("Trying to run Arc Browser...");
            RunArcBrowser();
            return;
        }
        
        var status = FixArcBrowser(path);
        if (!status)
        {
            Console.WriteLine("Failed to fix Arc Browser.");
            Thread.Sleep(2000);
            Environment.Exit(0);
            return;
        }
        
        Console.WriteLine("Trying to run Arc Browser...");
        RunArcBrowser();
    }
    
    private static void RunArcBrowser()
    {
        if (!File.Exists(ArcBrowserPath))
        {
            Console.WriteLine("Arc Browser executable not found.");
            return;
        }

        Process.Start(ArcBrowserPath);
        Console.WriteLine("Arc Browser started.");
        Thread.Sleep(2000);
        Environment.Exit(0);
    }
    
    private static bool FixArcBrowser(string path)
    {
        if (path == string.Empty)
        {
            Console.WriteLine("Arc Browser folder not found.");
            return false;
        }

        try
        {
            Directory.Delete(path, true);
            Console.WriteLine("Arc Browser is fixed.");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            return false;
        }
    }
    
    private static string GetProblemBrowserFolderPath()
    {
        var user = Environment.UserName;
        var path = $@"C:\Users\{user}\AppData\Local\Packages\TheBrowserCompany.Arc_{ArcBrowserMaybeId}\LocalCache\Local\firestore\Arc";
        return Directory.Exists(path) ? path : string.Empty;
    }

    private const string ArcBrowserMaybeId = "ttt1ap7aakyb4";
    private const string ArcBrowserPath = $@"C:\Program Files\WindowsApps\TheBrowserCompany.Arc_1.1.1.27314_x64__{ArcBrowserMaybeId}\Arc.exe";
}