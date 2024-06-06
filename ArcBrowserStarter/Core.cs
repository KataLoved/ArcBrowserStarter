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

         var bootstrap = new Bootstrapper();
         bootstrap.Initialize().Wait();
    }
}