//c:\Windows\Microsoft.Net\Framework\v4.0.30319\cs

namespace ppe
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management;
    using System.Text;
    using VirusTotalNet;
    using VirusTotalNet.Results;

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
            foreach (string f in Directory.EnumerateFiles(directory, "*"))
                PECheck(f);
            foreach (var d in Directory.GetDirectories(directory))
            {
                foreach (var f in Directory.GetFiles(d))
                    PECheck(f);
                Scan(d);
            }
        }

        static void PECheck(String file)
        {
            byte[] buffer = null;
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            long numBytes = new FileInfo(file).Length;
            buffer = reader.ReadBytes(5);

            var encoding = new ASCIIEncoding();
            var header = encoding.GetString(buffer);

            if (buffer[0] == 0x4D && buffer[1] == 0x5A)
                if (buffer[2] == 0x50)
                {
                    Console.WriteLine("{0} is a portable executable file. Check now? (y/n)", file);

                }
                else
                {
                    Console.WriteLine("{0} is an executable file. Check now? (y/n)", file);
                    if (Console.ReadLine() == "y" || Console.ReadLine() == "yes")
                        Submit(file);
                }
        }

        static async void Submit(String file)
        {
            VirusTotal vt = new VirusTotal("Insert Api Key Here");
            FileStream f = File.Open(file, FileMode.Open, FileAccess.Read);
            byte[] fileBytes = null;
            f.Read(fileBytes, 0, (int)f.Length);
            FileReport report = await vt.GetFileReportAsync(fileBytes);
            Console.WriteLine("Scan ID: " + report.ScanId);
            Console.WriteLine("Message: " + report.VerboseMsg);
        }
    }
}