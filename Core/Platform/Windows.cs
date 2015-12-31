﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Symbiote.Core.Platform
{
    public class Windows : IPlatform
    {
        private static Logger logger;

        public PlatformManager.PlatformType Type { get; private set; }
        public string Version { get; private set; }
        public ISystemInformation Info { get; private set; }

        public Windows()
        {
            logger = LogManager.GetCurrentClassLogger();

            Type = PlatformManager.PlatformType.Windows;
            Version = Environment.OSVersion.VersionString;
            Info = new WindowsSystemInformation();
        }

        public List<string> GetDirectoryList(string root)
        {
            List<string> list = new List<string>();
            try
            {
                list = Directory.EnumerateDirectories(root).ToList<string>();
            }
            catch (IOException ex)
            {
                logger.Error(ex, "Error listing directories for root path '" + root + "'");
            }
            return list;
        }

        public List<string> GetFileList(string root, string searchPattern)
        {
            List<string> list = new List<string>();
            try
            {
                list = Directory.EnumerateFiles(root, searchPattern).ToList<string>();
            }
            catch (IOException ex)
            {
                logger.Error(ex, "Error listing files for directory '" + root + "' using search pattern '" + searchPattern);
            }
            return list;
        }
    }

    public class WindowsSystemInformation: ISystemInformation
    {
        public double CPUTime { get; private set; }
        public double MemoryUsage { get; private set; }
        public List<IDiskInformation> Disks { get; private set; }
        public List<INetworkAdapterInformation> NetworkAdapters { get; private set; }

        public WindowsSystemInformation()
        {
            Disks = new List<IDiskInformation>();
            Refresh();
        }

        public void Refresh()
        {
            CPUTime = getCPUCounter();
            MemoryUsage = getAvailableRAM();
            Disks.Clear();
            
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
               if (drive.IsReady)
                    Disks.Add(new WindowsDiskInformation(drive));
            }

        }

        public double getCPUCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(250);
            return cpuCounter.NextValue();
        }

        public double getAvailableRAM()
        {
            return new PerformanceCounter("Memory", "Available MBytes").NextValue();
        }
    }

    public class WindowsDiskInformation : IDiskInformation
    {
        private static Logger logger;

        public string Name { get; private set; }
        public string Path { get; private set; }
        public PlatformManager.DiskType Type { get; private set; }
        public long Capacity { get; private set; }
        public long UsedSpace { get; private set; }
        public long FreeSpace { get; private set; }
        public double PercentFree { get; private set; }
        public double PercentUsed { get; private set; }

        public WindowsDiskInformation(DriveInfo drive)
        {
            logger = LogManager.GetCurrentClassLogger();
            try
            {
                if (!drive.IsReady)
                    throw new System.IO.IOException("Drive " + drive.Name + " is not ready; unable to retrieve information");
                else
                {
                    this.Name = drive.Name;
                    this.Path = drive.RootDirectory.ToString();
                    this.Type = (PlatformManager.DiskType)Enum.Parse(typeof(PlatformManager.DiskType), drive.DriveType.ToString());
                    this.Capacity = drive.TotalSize;
                    this.FreeSpace = drive.TotalFreeSpace;
                    this.UsedSpace = Capacity - FreeSpace;
                    this.PercentFree = FreeSpace / (double)Capacity;
                    this.PercentUsed = UsedSpace / (double)Capacity;
                }
            }
            catch (System.IO.IOException ex)
            {
                logger.Error(ex, "Error listing drive info");
            }
        }
    }
}
