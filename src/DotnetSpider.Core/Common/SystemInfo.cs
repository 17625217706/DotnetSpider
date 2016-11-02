﻿using System;
using System.Diagnostics;
using System.Net;
#if NET_CORE
using System.Runtime.InteropServices;
#else
using System.Management;
#endif

namespace DotnetSpider.Core.Common
{
	public class SystemInfo
	{
		public string Name { get; set; }
		public int CpuLoad { get; set; }
		public int CpuCount { get; set; }
		public int FreeMemory { get; set; }
		public int TotalMemory { get; set; }
		public string IpAddress { get; set; }
		public DateTime Timestamp { get; set; }
		public string Os { get; set; }

#if !NET_CORE
		private static readonly PerformanceCounter PcCpuLoad; //CPU计数器
#endif
		public static readonly int PhysicalMemory;
		public static readonly string HostName;
		public static readonly string Ip4Address;

		static SystemInfo()
		{
			HostName = Dns.GetHostName();

			// docker 化后只会有一个IP
			Ip4Address = Dns.GetHostAddressesAsync(HostName).Result[0].ToString();

#if !NET_CORE
			//初始化CPU计数器
			PcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total") { MachineName = "." };
			PcCpuLoad.NextValue();

			//获得物理内存
			ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
			ManagementObjectCollection moc = mc.GetInstances();
			foreach (var o in moc)
			{
				var mo = (ManagementObject)o;
				if (mo["TotalPhysicalMemory"] != null)
				{
					var physicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
					PhysicalMemory = (int)(physicalMemory / (1024 * 1024));
				}
			}
#else
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var memInfo = RunCommand("free", "-m").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				int totalMem = int.Parse(memInfo[6]);
				PhysicalMemory = totalMem;
			}
			else
			{
				// Didnot find dotnet core api now.
			}
#endif
		}

		public static SystemInfo GetSystemInfo()
		{
			SystemInfo systemInfo = new SystemInfo
			{
				CpuCount = Environment.ProcessorCount,
				TotalMemory = PhysicalMemory,
				Name = HostName,
				Timestamp = DateTime.Now,
				IpAddress = Ip4Address
			};


#if !NET_CORE
			long availablebytes = 0;

			ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
			foreach (var o in mos.GetInstances())
			{
				var mo = (ManagementObject)o;
				if (mo["FreePhysicalMemory"] != null)
				{
					availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
				}
			}

			systemInfo.CpuLoad = (int)(PcCpuLoad.NextValue());
			systemInfo.FreeMemory = (int)(availablebytes / (1024 * 1024));
			systemInfo.Os = "Windows";
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// cpu load 
				string loadAvg = RunCommand("cat", "/proc/loadavg");
				systemInfo.CpuLoad = (int)(float.Parse(loadAvg.Split(' ')[2]) * 100);

				var memInfo = RunCommand("free", "-m").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

				int usedMem = int.Parse(memInfo[7]);

				systemInfo.FreeMemory = (PhysicalMemory - usedMem);
				systemInfo.Os = "Linux";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// cpu load 
				string loadAvg = RunCommand("cat", "/proc/loadavg");
				systemInfo.CpuLoad = (int)(float.Parse(loadAvg.Split(' ')[2]) * 100);

				//todo:
				systemInfo.Os = "OSX";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Didnot find dotnet core api now.
				//PcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total") { MachineName = "." };
				//PcCpuLoad.NextValue();

				//todo:
				systemInfo.Os = "Windows";
			}
#endif
			return systemInfo;
		}

		public static string RunCommand(string command, string arguments)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments) {RedirectStandardOutput = true};
			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit(1500);
			return process.StandardOutput.ReadToEnd();
		}
	}
}
