using Accord.Audio;
using Accord.DirectSound;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using HeyRed.Mime;
using System.DrawingCore;

namespace Server
{
    public static partial class Program
    {
        public static void GetImage(string reference, WebSocketService service)
        {
#pragma warning disable CS0219 // La variable 'data' est assignée, mais sa valeur n'est jamais utilisée
            byte[] data = null;
#pragma warning restore CS0219 // La variable 'data' est assignée, mais sa valeur n'est jamais utilisée
            string datas = "";
            string filePath = imagesDir + reference;
            bool error = false;
            if (ImagesList.ContainsKey(reference) == true)
            {
                if (File.Exists(filePath) == true)
                {
                    FileInfo fi = new FileInfo(filePath);
                    if (fi.LastWriteTime.ToFileTimeUtc() > (long)ImagesList[reference]["lastTime"]) { ImagesList.Remove(reference); }
                }
                else { error = true; }
            }

            if (error == false)
            {
                if (ImagesList.ContainsKey(reference) == false)
                {
                    if (File.Exists(filePath) == true)
                    {
                        FileInfo fi = new FileInfo(filePath);
                        using (Image image = Image.FromFile(filePath))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();
                                string base64String = Convert.ToBase64String(imageBytes);
                                ImagesList[reference] = new Dictionary<string, object>();
                                ImagesList[reference]["data"] = base64String;
                                ImagesList[reference]["lastTime"] = fi.LastWriteTime.ToFileTimeUtc();
                                ImagesList[reference]["MimeType"] = MimeTypesMap.GetMimeType(filePath);
                                datas = "{\"function\":\"RetGetImage\", \"reference\":\"" + reference + "\", \"result\":\"data:" + ImagesList[reference]["MimeType"] + ";base64," + base64String + "\"}";
                            }
                        }
                    }
                    else { error = true; }
                }
                else
                {
                    datas = "{\"function\":\"RetGetImage\", \"reference\":\"" + reference + "\", \"result\":\"data:" + ImagesList[reference]["MimeType"] + ";base64," + ImagesList[reference]["data"] + "\"}";
                }
            }

            if (error == true) { datas = "{\"function\":\"RetGetImage\", \"reference\":\"" + reference + "\", \"result\":\"ERROR\"}"; }
            service.SendMessage(datas);
        }

    }
}
