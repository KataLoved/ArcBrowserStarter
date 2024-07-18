using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Linq;
using System.IO;
using Octokit;
using System;

namespace ArcBrowserStarter;

public class Bootstrapper
{
    public async Task Initialize()
    {
        await CheckForNewRelease();
        
        var systemLanguage = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
        _language = LoadLanguage(systemLanguage);
        
        var paths = GetArcBrowserFolderPaths();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Found Arc Browser:");
        Console.WriteLine($"> Version: {paths["version"]}");
        Console.WriteLine($"> Package ID: {paths["packageId"]}");
        if (paths.ContainsKey("problemPath"))
        {
            Console.WriteLine($"> Problem Path: {paths["problemPath"]}");
        }
        
        Console.WriteLine($"> Executable Path: {paths["executablePath"]}");
        Console.ResetColor();
        if (paths.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_language["ArcBrowserNotFound"]);
            Console.ResetColor();
            Console.WriteLine(_language["PressAnyKeyToExit"]);
            Console.ReadLine();
            Environment.Exit(0);
        }


        if (paths.ContainsKey("problemPath"))
        {
            var fixStatus = FixArcBrowser(paths["problemPath"]);
            if (!fixStatus)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(_language["FailedToFixArcBrowser"]);
                Console.ResetColor();
                Console.WriteLine(_language["PressAnyKeyToExit"]);
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
        
        Console.WriteLine(_language["TryingToRunArcBrowser"]);
        Thread.Sleep(1000);
        CloseRequirementsWatcher();
        
        var executableStatus = RunArcBrowser(paths["executablePath"]);
        if (!executableStatus)
        {
            _arcStartWatcher.Stop();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_language["FailedToRunArcBrowser"]);
            Console.ResetColor();
            Console.WriteLine(_language["PressAnyKeyToExit"]);
            Console.ReadLine();
            Environment.Exit(0);
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(_language["ArcBrowserIsRunning"]);
        Console.ResetColor();
        
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await Task.Delay(-1, cts.Token);
        }
        catch (TaskCanceledException)
        {
            _arcStartWatcher.Stop();
            Environment.Exit(0);
        }
    }

    private static async Task CheckForNewRelease()
    {
        var client = new GitHubClient(new ProductHeaderValue(ProgramName));
        var releases = await client.Repository.Release.GetAll("kataloved", ProgramName);
        if (!releases.Any()) return;

        var latestRelease = releases.First();
        var latestVersion = latestRelease.TagName;
        if (latestVersion == Version) return;

        var lines = new[]
        {
            _language["NewVersionAvailable1"]?.Replace("{{LATEST_VERSION}}", latestVersion).Replace("{{VERSION}}", Version),
            _language["NewVersionAvailable2"],
            _language["NewVersionAvailable3"]?.Replace("{{URL}}", latestRelease.HtmlUrl),
            _language["NewVersionAvailable4"]
        };
        
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (var line in lines) Console.WriteLine(line);
        Console.ResetColor();
        
        var pressedKey = Console.ReadKey().Key;
        if (pressedKey != ConsoleKey.Enter) Environment.Exit(0);
    }

    private static Dictionary<string, string> GetArcBrowserFolderPaths()
    {
        const string windowsAppsPath = @"C:\Program Files\WindowsApps";

        var username = Environment.UserName;
        var packagesPath = $@"C:\Users\{username}\AppData\Local\Packages";

        var paths = new Dictionary<string, string>();
        var packages = Directory.GetDirectories(packagesPath);
        var apps = Directory.GetDirectories(windowsAppsPath);
        
        var arcPackages = packages.Where(p => p.Contains("TheBrowserCompany.Arc")).ToList();
        var arcExecutables = apps.Where(p => p.Contains("TheBrowserCompany.Arc")).ToList();
        if (!arcPackages.Any() || !arcExecutables.Any()) return paths;
        
        var arcExecutablePaths = arcExecutables.Select(path =>
        {
            var versionRegExp = new System.Text.RegularExpressions.Regex(@"\d+\.\d+\.\d+\.\d+");
            var version = versionRegExp.Match(path).Value;
            var numberVersion = Convert.ToInt32(version.Replace(".", ""));
            
            var packageId = path.Split('_').Last().Split('\\').First();
            var arcPath = $@"{path}\Arc.exe";
            
            return new { PackageId = packageId, Path = arcPath, Version = version, numberVerion = numberVersion };
        }).ToList();
        
        var arcPackagePaths = arcPackages.Select(path =>
        {
            var packageId = path.Split('_').Last().Split('\\').First();
            var arcPath = $@"{path}\LocalCache\Local\firestore\Arc";
            return new { PackageId = packageId, Path = arcPath };
        }).ToList();
        
        var latestVersion = arcExecutablePaths.Max(v => v.numberVerion);
        var latest = arcExecutablePaths.First(v => v.numberVerion == latestVersion);
        
        var packageId = latest.PackageId;
        var version = latest.Version;
        
        var problemPath = arcPackagePaths.FirstOrDefault(p => p.PackageId == packageId)?.Path;
        paths.Add("version", version);
        paths.Add("packageId", packageId);
        paths.Add("executablePath", latest.Path);

        if (Directory.Exists(problemPath)) paths.Add("problemPath", problemPath);
        return paths;
    }
    
    private static bool FixArcBrowser(string path)
    {
        try
        {
            Directory.Delete(path, true);
            return true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {e.Message}");
            Console.ResetColor();
            return false;
        }
    }
    
    private static bool RunArcBrowser(string path)
    {
        try
        {
            Process.Start(path);
            return true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {e.Message}");
            Console.ResetColor();
            return false;
        }
    }
    
    private static void CloseRequirementsWatcher()
    {
        var wqlStartQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace WHERE ProcessName = \"Arc.exe\"");
        _arcStartWatcher = new ManagementEventWatcher(wqlStartQuery);

        _arcStartWatcher.EventArrived += startWatch_EventArrived;
        _arcStartWatcher.Start();
    }

    private static void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
    {
        var processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
        var process = Process.GetProcessById(processId);
        var processName = process.ProcessName;
        
        if (processName != "Arc") return;
        
        var processTitle = process.MainWindowTitle;
        if (processTitle.Contains("Processor requirements not met"))
        {
            Thread.Sleep(700);
            SendKeys.SendWait("{ENTER}");
        }
        
        _arcStartWatcher.Stop();
        Environment.Exit(0);
    }
    
    private static Dictionary<string, string> LoadLanguage(string languageKey)
    {
        var languagePath = $@"languages\{languageKey}.ini";
        var language = new Dictionary<string, string>
        {
            { "ArcBrowserNotFound", "Arc Browser not found." },
            { "PressAnyKeyToExit", "Press any key to exit..." },
            { "FailedToFixArcBrowser", "Failed to fix Arc Browser." },
            { "TryingToRunArcBrowser", "Trying to run Arc Browser..." },
            { "FailedToRunArcBrowser", "Failed to run Arc Browser." },
            { "ArcBrowserIsRunning", "Arc Browser is running." },
            { "NewVersionAvailable1", "New version available: {{LATEST_VERSION}}. Current version: {{VERSION}}" },
            { "NewVersionAvailable2", "Correct operation of the program is not guaranteed." },
            { "NewVersionAvailable3", "You can download it from: {{URL}}" },
            { "NewVersionAvailable4", "Press enter if you want to continue..." }
        };
        
        if (!File.Exists(languagePath)) return language;
        
        var lines = File.ReadAllLines(languagePath);
        return lines
            .Select(line => line.Split('='))
            .Where(parts => parts.Length == 2)
            .Aggregate(language, (current, parts) =>
            {
                current[parts[0]] = parts[1];
                return current;
            });
    }

    private const string Version = "1.0.2";
    private static Dictionary<string, string> _language;
    private const string ProgramName = "ArcBrowserStarter";
    private static ManagementEventWatcher _arcStartWatcher;
}