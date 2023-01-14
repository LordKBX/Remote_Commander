using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using static Source_PerformanceCounters.Source;

namespace Source_PerformanceCounters
{
    public class ExtType { 
        public static readonly string Type = "Source";
        public static readonly string Name = "PerformanceCounters";
        //return type json = json stringified
        public static readonly Dictionary<string, string> Interfaces = new Dictionary<string, string>() { 
            //{ "Init", "void" }, 
            { "GetCpu", "float" }, 
            { "Memory", "json" } 
        };

    }

    public class Source
    {
        /// Alternate Wrapper Method
        /// <summary>Retrieves information about the system's current usage of both physical and virtual memory.</summary>
        /// <param name="msex">Memory information structure that will be populated by this method</param>
        /// <returns>True if the wrapped native call was successfull, false if not.  Use
        /// <see cref="Marshal.GetLastWin32Error"/> for additional error information</returns>
        public static bool GlobalMemoryStatus(ref MEMORYSTATUSEX msex)
        {
            msex.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

            return (_GlobalMemoryStatusEx(ref msex));
        }

        //Used to use ref with comment below
        // but ref doesn't work.(Use of [In, Out] instead of ref
        //causes access violation exception on windows xp
        //comment: most probably caused by MEMORYSTATUSEX being declared as a class
        //(at least at pinvoke.net). On Win7, ref and struct work.
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        // Alternate Version Using "ref," And Works With Alternate Code Below.
        // Also See Alternate Version Of [MEMORYSTATUSEX] Structure With
        // Fields Documented.
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
        static extern bool _GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        //private static PerformanceCounter cpuCounter;
        //private static PerformanceCounter freeRamCounter;
        //private static PerformanceCounter UsedRamCounter;
        //private static PerformanceCounter CachedRamCounter;

        //public static void Init() { 
            //cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //freeRamCounter = new PerformanceCounter("Memory", "Available Bytes");
            //UsedRamCounter = new PerformanceCounter("Memory", "Committed Bytes");
            //CachedRamCounter = new PerformanceCounter("Memory", "Cache Bytes");
        //}

        public static float GetCpu()
        {
            try
            {
                //Console.WriteLine("CPU = "+ cpuCounter.NextValue() + "%");
                //return cpuCounter.NextValue();
                
                return ProcessorLogic.CPUusagePercent();
            }
            catch (Exception err) {
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }
            return 0;
        }

        public static string Memory()
        {
            ulong memAvaillable = 0;
            ulong memUsed = 0;
            ulong memTotal = 0;
            //float memCached = 0;

            try
            {
                NativeMethods oMemoryInfo = new NativeMethods();
                memTotal = oMemoryInfo.msex.ullTotalVirtual;
                memAvaillable = oMemoryInfo.msex.ullAvailVirtual;
                memUsed = memTotal - memAvaillable;

                //memAvaillable = freeRamCounter.NextValue();
                //memUsed = UsedRamCounter.NextValue();
                //memCached = CachedRamCounter.NextValue();
                //memTotal = memAvaillable + memUsed + memCached;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }
            //return "{\"free\":"+ memAvaillable + ", \"used\":" + memUsed + ", \"cached\":" + memCached + ", \"total\":" + memTotal + ", \"unit\": \"Bytes\" }";
            return "{\"free\":"+ Convert.ToString((long)memAvaillable, 10) + ", \"used\":" + Convert.ToString((long)memUsed, 10) + ", \"total\":" + Convert.ToString((long)memTotal, 10) + ", \"unit\": \"Bytes\" }";
        }
    }

    /// <summary>
    /// contains information about the current state of both physical and virtual memory, including extended memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        /// <summary>
        /// Size of the structure, in bytes. You must set this member before calling GlobalMemoryStatusEx.
        /// </summary>
        public uint dwLength;

        /// <summary>
        /// Number between 0 and 100 that specifies the approximate percentage of physical memory that is in use (0 indicates no memory use and 100 indicates full memory use).
        /// </summary>
        public uint dwMemoryLoad;

        /// <summary>
        /// Total size of physical memory, in bytes.
        /// </summary>
        public ulong ullTotalPhys;

        /// <summary>
        /// Size of physical memory available, in bytes.
        /// </summary>
        public ulong ullAvailPhys;

        /// <summary>
        /// Size of the committed memory limit, in bytes. This is physical memory plus the size of the page file, minus a small overhead.
        /// </summary>
        public ulong ullTotalPageFile;

        /// <summary>
        /// Size of available memory to commit, in bytes. The limit is ullTotalPageFile.
        /// </summary>
        public ulong ullAvailPageFile;

        /// <summary>
        /// Total size of the user mode portion of the virtual address space of the calling process, in bytes.
        /// </summary>
        public ulong ullTotalVirtual;

        /// <summary>
        /// Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process, in bytes.
        /// </summary>
        public ulong ullAvailVirtual;

        /// <summary>
        /// Size of unreserved and uncommitted memory in the extended portion of the virtual address space of the calling process, in bytes.
        /// </summary>
        public ulong ullAvailExtendedVirtual;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MEMORYSTATUSEX"/> class.
        /// </summary>
        public MEMORYSTATUSEX() { this.dwLength = (uint)Marshal.SizeOf(typeof(NativeMethods.MEMORYSTATUSEX)); }
    }

    [CLSCompliant(false)]
    public class NativeMethods
    {
        public MEMORYSTATUSEX msex;
        private uint _MemoryLoad;
        const int MEMORY_TIGHT_CONST = 80;

        public bool isMemoryTight()
        {
            if (_MemoryLoad > MEMORY_TIGHT_CONST)
                return true;
            else
                return false;
        }

        public uint MemoryLoad
        {
            get { return _MemoryLoad; }
            internal set { _MemoryLoad = value; }
        }

        public NativeMethods()
        {

            msex = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(msex))
            {

                _MemoryLoad = msex.dwMemoryLoad;
                //etc.. Repeat for other structure members        

            }
            else
                // Use a more appropriate Exception Type. 'Exception' should almost never be thrown
                throw new Exception("Unable to initalize the GlobalMemoryStatusEx API");
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    }

    static class ProcessorLogic
    {
        // calc cpu
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

        static bool bUsedOnce = false;
        static ulong uOldIdle = 0;
        static ulong uOldKrnl = 0;
        static ulong uOldUsr = 0;

        public static float CPUusagePercent()
        {
            float nRes = -1;

            if (GetSystemTimes(out FILETIME ftIdle, out FILETIME ftKrnl, out FILETIME ftUsr))
            {
                ulong uIdle = ((ulong)ftIdle.dwHighDateTime << 32) | (uint)ftIdle.dwLowDateTime;
                ulong uKrnl = ((ulong)ftKrnl.dwHighDateTime << 32) | (uint)ftKrnl.dwLowDateTime;
                ulong uUsr = ((ulong)ftUsr.dwHighDateTime << 32) | (uint)ftUsr.dwLowDateTime;

                if (bUsedOnce)
                {
                    ulong uDiffIdle = uIdle - uOldIdle;
                    ulong uDiffKrnl = uKrnl - uOldKrnl;
                    ulong uDiffUsr = uUsr - uOldUsr;

                    if ((uDiffKrnl + uDiffUsr) != 0)
                    { //Calculate percentage
                        nRes = ((uDiffKrnl + uDiffUsr - uDiffIdle) * 100 / (uDiffKrnl + uDiffUsr));
                    }
                }

                bUsedOnce = true;
                uOldIdle = uIdle;
                uOldKrnl = uKrnl;
                uOldUsr = uUsr;
            }

            return nRes;
        }
    }
}
