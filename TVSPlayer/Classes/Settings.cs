﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TVSPlayer {
    static class Settings {
        private static string library;
        private static bool autodownload;
        private static string cachelocation;
        private static string scanone;
        private static string scantwo;
        private static string scanthree;
        private static bool theme;
        public static string Library { get { return library; } set { library = value; SaveSettings(); } }
        public static bool AutoDownload { get { return autodownload; } set { autodownload = value; SaveSettings(); } }
        public static string DownloadCacheLocation { get { return cachelocation; } set { cachelocation = value; SaveSettings(); } }
        public static string FirstScanLocation { get { return scanone; } set { scanone = value; SaveSettings(); } }
        public static string SecondScanLocation { get { return scantwo; } set { scantwo = value; SaveSettings(); } }
        public static string ThirdScanLocation { get { return scanthree; } set { scanthree = value; SaveSettings(); } }
        public static bool Theme  { get { return theme; } set { theme = value; SaveSettings(); } }

        public static void SaveSettings() {
            Type type = typeof(Settings);
            string filename = Helper.data + "Settings.tvsp";
            // Library = "test";
            // Test = Library;
            CreateDir();
            if (!File.Exists(filename)) {
                File.Create(filename).Dispose();
            }
            do {
                try {
                    FieldInfo[] properties = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
                    object[,] a = new object[properties.Length, 2];
                    int i = 0;
                    foreach (FieldInfo field in properties) {
                        a[i, 0] = field.Name;
                        a[i, 1] = field.GetValue(null);
                        i++;
                    };
                    string json = JsonConvert.SerializeObject(a);
                    StreamWriter sw = new StreamWriter(filename);
                    sw.Write(json);
                    sw.Close();
                    return;
                    //Load(json);
                } catch (IOException e) {
                    Thread.Sleep(10);
                }
            } while (true);
        }

        private static void CreateDir() {
            string filename = Helper.data + "Settings.tvsp";
            if (!Directory.Exists(Path.GetDirectoryName(filename))){
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
        }

        public static void Load() {
            Type type = typeof(Settings);
            string filename = Helper.data + "Settings.tvsp";
            // Library = "";
            // Test = Library;
            CreateDir();
            if (!File.Exists(filename)) {
                File.Create(filename).Dispose();
            }
            do {
                try {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                    object[,] a;
                    StreamReader sr = new StreamReader(filename);
                    string json = sr.ReadToEnd();
                    sr.Close();
                    if (!String.IsNullOrEmpty(json)) {
                        JArray ja = JArray.Parse(json);
                        a = ja.ToObject<object[,]>();
                        if (a.GetLength(0) != fields.Length) { }
                        int i = 0;
                        foreach (FieldInfo field in fields) {
                            if (field.Name == (a[i, 0] as string)) {
                                field.SetValue(null, a[i, 1]);
                            }
                            i++;
                        };
                    }
                    return;
                } catch (IOException e) {
                    Thread.Sleep(15);
                }
            } while (true);
        }
    }
}