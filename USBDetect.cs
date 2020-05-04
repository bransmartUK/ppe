//c:\Windows\Microsoft.Net\Framework\v4.0.30319\cs

namespace ppe
{
    using System;
    using System.IO;
    using System.Management;
    using System.Collections.Generic;

    public static class Program
    {
        public static void Main(String[] args)
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += (s, e) =>
            {
                string driveName = (string)e.NewEvent.GetPropertyValue("DriveName");
                Console.WriteLine("Inserted drive: {0}", driveName);
                var drive = new DriveInfo(driveName);
                Scan(drive.RootDirectory.ToString());
            };
            watcher.Query = query;
            watcher.Start();
            watcher.WaitForNextEvent();

            Console.Read();
        }

        static void Scan(String directory)
        {
            // TODO: Check file for PE header
            // see if we can run it isolated
            // check for capabilities
            Console.WriteLine("Files in {0}", directory);
            foreach (string f in Directory.EnumerateFiles(directory, "*"))
                Console.WriteLine(f);
            foreach (var d in Directory.GetDirectories(directory))
            {
                Console.WriteLine("Files in {0}:", d);
                foreach (var f in Directory.GetFiles(d))
                    Console.WriteLine(f);
                Scan(d);
            }
        }
    }
}