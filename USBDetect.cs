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
            var usbDevices = GetUSBDevices();
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += (s, e) =>
            {
                string driveName = (string)e.NewEvent.GetPropertyValue("DriveName");
                Console.WriteLine("Inserted drive: {0}", driveName);
                var drive = new DriveInfo(driveName);
                var files = Directory.EnumerateFiles(drive.RootDirectory.ToString(), "*");
                foreach(string f in files)
                    Console.WriteLine("File in {0}: {1}", drive, f);
            };
            watcher.Query = query;
            watcher.Start();
            watcher.WaitForNextEvent();
            
            // foreach (var d in allDrives)
            // {
            //     Console.WriteLine("Drive {0}", d.Name);
            //     Console.WriteLine("Drive Type {0}", d.DriveType);
            //     if (d.IsReady == true)
            //     {
            //         Console.WriteLine("  Volume label: {0}", d.VolumeLabel);
            //         Console.WriteLine("  File system: {0}", d.DriveFormat);
            //         Console.WriteLine(
            //             "  Available space to current user:{0, 15} bytes",
            //             d.AvailableFreeSpace);

            //         Console.WriteLine(
            //             "  Total available space:          {0, 15} bytes",
            //             d.TotalFreeSpace);

            //         Console.WriteLine(
            //             "  Total size of drive:            {0, 15} bytes ",
            //             d.TotalSize);
            //     }
            // }
            Console.Read();
        }

        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
        }
        class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
        }
    }
}