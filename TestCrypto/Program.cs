using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace TestCrypto
{
    class Program
    {
        private static bool unload = false;
        static void Main(string[] args)
        {
            //PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();

            //string[] categoryNames = new string[categories.Length];
            //int objX;
            //for (objX = 0; objX < categories.Length; objX++)
            //{
            //    categoryNames[objX] = categories[objX].CategoryName;
            //}
            //Array.Sort(categoryNames);

            //for (objX = 0; objX < categories.Length; objX++)
            //{
            //    Console.WriteLine("{0,4} - {1}", objX + 1, categoryNames[objX]);
            //}

            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter freeRamCounter = new PerformanceCounter("Memory", "Available Bytes");
            PerformanceCounter UsedRamCounter = new PerformanceCounter("Memory", "Committed Bytes");
            PerformanceCounter CachedRamCounter = new PerformanceCounter("Memory", "Cache Bytes");
            cpuCounter.NextValue();

            Crytography.Init();
            Console.WriteLine("Hello World!");
            Console.WriteLine("Public Key = "+ Crytography.GetPublicKeyString());
            string input = "";
            float memAvaillable = 0;
            float memUsed = 0;
            float memCached = 0;
            float memTotal = 0;
            while (unload == false)
            {
                memAvaillable = freeRamCounter.NextValue();
                memUsed = UsedRamCounter.NextValue();
                memCached = CachedRamCounter.NextValue();
                memTotal = memAvaillable + memUsed + memCached;
                Console.WriteLine("Cpu Usage = " + cpuCounter.NextValue() + "%");
                Console.WriteLine("Memory Available = " + (freeRamCounter.NextValue() / 1024 / 1024) + "Mb");
                Console.WriteLine("Memory Used = " + (UsedRamCounter.NextValue() / 1024 / 1024) + "Mb");
                Console.WriteLine("Memory Cached = " + (CachedRamCounter.NextValue() / 1024 / 1024) + "Mb");
                Console.WriteLine("Memory Total = " + (memTotal / 1024 / 1024) + "Mb");
                Console.WriteLine("Input:");
                input = Console.ReadLine();
                string retin = Crytography.Encrypt(input);
                Console.WriteLine("Encrypted:" + retin);
                Console.WriteLine("Decrypted:" + Crytography.Decrypt(retin));
                Console.WriteLine("- - - - - - - - - - - - - - - -");
            }
        }

        private static string Encrypt(string input, string key)
        {
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(input);
            string b64 = Convert.ToBase64String(toEncryptArray, 0, toEncryptArray.Length);

            toEncryptArray = UTF8Encoding.UTF8.GetBytes(b64+key);
            b64 = Convert.ToBase64String(toEncryptArray, 0, toEncryptArray.Length);

            toEncryptArray = UTF8Encoding.UTF8.GetBytes(key + b64);
            b64 = Convert.ToBase64String(toEncryptArray, 0, toEncryptArray.Length);
            return b64;
        }

        private static string Decrypt(string input, string key)
        {
            byte[] data = Convert.FromBase64String(input);
            string decodedString = Encoding.UTF8.GetString(data);
            decodedString = decodedString.Replace(key, "");

            data = Convert.FromBase64String(decodedString);
            decodedString = Encoding.UTF8.GetString(data);
            decodedString = decodedString.Replace(key, "");

            data = Convert.FromBase64String(decodedString);
            decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }
    }
}
