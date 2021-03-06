﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVS.API;

namespace TVSPlayer {
    static class TorrentDatabase {

        /// <summary>
        /// Adds torrent to databae
        /// </summary>
        /// <param name="torrent">Torrent</param>
        public static void Save(Torrent torrent) {
            Series series = torrent.Series;
            Episode episode = torrent.Episode;
            var list = Load();
            torrent.seriesId = torrent.Series.id;
            torrent.episodeId = torrent.Episode.id;
            torrent.Series = null;
            torrent.Episode = null;
            list.Add(torrent);
            string text = JsonConvert.SerializeObject(list);
            torrent.Series = series;
            torrent.Episode = episode;
            Database.WriteToFile(Helper.data + "Torrents.tvsp", text);
        }

        /// <summary>
        /// Returns all torrents in database
        /// </summary>
        /// <returns></returns>
        public static List<Torrent> Load() {
            string original = Database.ReadFromFile(Helper.data + "Torrents.tvsp");
            List<Torrent> list = new List<Torrent>();
            if (!String.IsNullOrEmpty(original)) {
                JArray jo = JArray.Parse(original);
                list = jo.ToObject<List<Torrent>>();
            }
            foreach (var item in list) {
                item.Series = Database.GetSeries(item.seriesId);
                item.Episode = Database.GetEpisode(item.seriesId, item.episodeId);
            }
            return list;
        }

        /// <summary>
        /// Edits database by magnet and new torrent
        /// </summary>
        /// <param name="magnet">any magnet</param>
        /// <param name="newTorrent">new torrent that will replace old one</param>
        public static void Edit(string magnet, Torrent newTorrent) {
            List<Torrent> eps = Load();
            try {
                eps.Remove(eps.Single(se => se.Magnet == magnet));
                Series series = newTorrent.Series;
                Episode episode = newTorrent.Episode;
                newTorrent.Series = null;
                newTorrent.Episode = null;
                eps.Add(newTorrent);
                string text = JsonConvert.SerializeObject(eps);
                newTorrent.Series = series;
                newTorrent.Episode = episode;
                Database.WriteToFile(Helper.data + "Torrents.tvsp", text);
            } catch (Exception) { }
        }

        /// <summary>
        /// Removes torrent from database by magnet
        /// </summary>
        /// <param name="magnet">any magnet</param>
        public static void Remove(string magnet) {
            List<Torrent> eps = Load();
            try {
                eps.Remove(eps.Single(se => se.Magnet == magnet));
                string text = JsonConvert.SerializeObject(eps);
                Database.WriteToFile(Helper.data + "Torrents.tvsp", text);
            } catch (Exception) { }
        }

        /// <summary>
        /// Cheks if torrent exist in database by magnet
        /// </summary>
        /// <param name="magnet">any magnet</param>
        /// <returns></returns>
        public static bool Exists(string magnet) {
            var list = Load();
            foreach (var item in list) {
                if (item.Magnet == magnet) {
                    return true;
                }
            }
            return false;
        }
    }
}
