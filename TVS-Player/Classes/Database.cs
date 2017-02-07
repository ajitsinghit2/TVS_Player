﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace TVS_Player {
    public static class DatabaseAPI {
        public static Database database = new Database();

        public static void readDb() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            try {
                string json = File.ReadAllText(path);
                database = JsonConvert.DeserializeObject<Database>(json);
            } catch {
                database = new Database();
            }
        }
        public static void resetDb(string dbLoc, bool save) {
            database = new Database();
            if (save) {
                saveDB();
            }
        }

        public static bool CheckIfExists(string id) {
            foreach (SelectedShows ss in database.Shows) {
                if (ss.idSel == id) {
                    return true;
                }
            }
            return false;
        }

        public static void addShowToDb(string id, string showname, bool save) {
            SelectedShows newShow = new SelectedShows(id, showname);
            if (!CheckIfExists(id)) {
                database.Shows.Add(newShow);
            } else {
                DialogResult dialogResult = MessageBox.Show("TV Show is already in database do you want to rewrite it?", "TV Shows already exists", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) {
                    removeShowFromDb(id, true);
                    database.Shows.Add(newShow);
                }
            }          
            if (save) {
                saveDB();
            }
        }
        public static void removeShowFromDb(string id, bool save) {
            database.Shows.Remove(FindShowByID(id));
            if (save) {
                saveDB();
            }
        }
        public static void saveDB() {
            string json = JsonConvert.SerializeObject(database);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, json);
        }
        public static SelectedShows FindShowByID(string ID) {
            SelectedShows foundShow = null;
            foreach (SelectedShows ss in database.Shows) {
                if (ss.idSel == ID) {
                    foundShow = ss;
                }
            }
            return foundShow;
        }
        public static int GetSeasons(int id) {
            return DatabaseEpisodes.readDb(id).Max(e => e.season);
        }   


    }
    public class Database {
        public string libraryLocation;
        public List<SelectedShows> Shows;

        public Database() {
            libraryLocation = String.Empty;
            Shows = new List<SelectedShows>();
        }
    }
    public class DatabaseEpisodes {
        public static void createDB(int id, List<Episode> l) {
            string path = Helpers.path + id + "\\Detail.json";
            string json = JsonConvert.SerializeObject(l);
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, json);
        }
        public static void addEPToDb(int id,Episode e) {
            List <Episode> list = readDb(id);
            list.Add(e);
            createDB(id, list);           
        }
        public static int GetEpPerSeason(int id, int season) {
            List<Episode> list = readDb(id);
            int count = 0;
            foreach (Episode e in list) {
                if (e.season == season) {
                    count++;
                }
            }
            return count;
        }

        public static string GetSeasonRelease(int id, int season) {
            List<Episode> list = readDb(id);
            return list.FindAll(s => s.season == season).Min(e => e.release);
        }

        public static List<Episode> readDb(int id) {
            List<Episode> e = new List<Episode>();
            string path = Helpers.path + id + "\\Detail.json";
            JArray jo = new JArray();
            try {
                string json = File.ReadAllText(path);
                jo = JArray.Parse(json);
            } catch {
            }
            foreach (JToken jt in jo) {
                e.Add(jt.ToObject<Episode>());             
            }
            return e;
        }

    }
    public class SelectedShows {
        public string idSel;
        public string nameSel;
        public string posterFilename;
        public SelectedShows(string id, string showname, string poster = null) {
            this.idSel = id;
            this.nameSel = showname;
            this.posterFilename = poster;
        }
    }
    public class Episode {
        public string name;
        public int season;
        public int episode;
        public int id;
        public string release;
        public bool downloaded;
        public List<string> locations; 
        public Episode(string name, int season, int episode, int id, string releaseDate ,bool downloaded, List<string> l) {
            this.name = name;
            this.season = season;
            this.episode = episode;
            release = releaseDate;
            this.id = id;
            this.downloaded = downloaded;
            locations = l;
        }        
    }
}