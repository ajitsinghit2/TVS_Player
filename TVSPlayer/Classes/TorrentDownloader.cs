﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ragnar;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using Ragnar;
using System.Threading;
using TVS.Notification;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using TVS.API;
using static TVS.API.Episode;
using System.Windows;

namespace TVSPlayer {
    public class TorrentDownloader {

        public static List<TorrentDownloader> torrents = new List<TorrentDownloader>();

        public TorrentDownloader(Torrent torrent) {
            TorrentSource = torrent;
        }

        public TorrentStatus Status { get; set; }
        public Torrent TorrentSource { get; set; }
        public TorrentHandle Handle { get; set; }
        public Session TorrentSession { get; set; }
        public bool ShowNotificationWhenFinished { get; set; } = true;

        private TorrentDownloader DownloadLocal(bool sequential) {
            TorrentSession = new Session();
            TorrentSession.ListenOn(6881, 6889);
            var torrentParams = new AddTorrentParams();
            torrentParams.SavePath = GetDownloadPath();
            torrentParams.Url = TorrentSource.Magnet;
            Handle = TorrentSession.AddTorrent(torrentParams);
            Handle.SequentialDownload = TorrentSource.IsSequential = sequential;
            Status = Handle.QueryStatus();
            torrents.Add(this);
            TorrentDatabase.Save(TorrentSource);
            return this;
        }

        public async Task<TorrentDownloader> Stream(bool showStream = true) {
            var downloader = await Task.Run(() => {
                return DownloadLocal(true);
            });
#pragma warning disable CS4014
            Task.Run(() => {
                while (Handle != null && !downloader.Status.IsSeeding) {
                    Trace.WriteLine(downloader.Status.DownloadRate + ", " + downloader.Status.AllTimeDownload);
                    downloader.Status = downloader.Handle.QueryStatus();
                    Thread.Sleep(1000);
                }
            });
#pragma warning restore CS4014
            MainWindow.AddPage(new TorrentStreamer(downloader));
            return downloader;
        }

        public async Task<TorrentDownloader> Download() {
            var torrs = torrents;
            torrs = torrs.Where(x => x.TorrentSource.Magnet == TorrentSource.Magnet).ToList();
            if (torrs.Count == 0) {
                var downloader = await Task.Run(() => {
                    return DownloadLocal(false);
                });
#pragma warning disable CS4014
                Task.Run(() => {
                    while (Handle != null &&!downloader.Status.IsSeeding) {
                        Trace.WriteLine(downloader.Status.DownloadRate + ", " + downloader.Status.AllTimeDownload);
                        downloader.Status = downloader.Handle.QueryStatus();
                        Thread.Sleep(1000);
                    }
                    if (downloader.Status != null) {
                        TorrentSource.Name = Handle.TorrentFile.Name;
                        Episode ep = Database.GetEpisode(TorrentSource.Series.id, TorrentSource.Episode.id);
                        Renamer.MoveAfterDownload(this);
                        TorrentSource.FinishedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        TorrentSource.HasFinished = true;
                        TorrentDatabase.Edit(TorrentSource.Magnet, TorrentSource);
                        torrents.Remove(downloader);
                        if (ShowNotificationWhenFinished) {
                            NotificationSender sender = new NotificationSender("Download finished", Helper.GenerateName(TorrentSource.Series, TorrentSource.Episode));
                            sender.ClickedEvent += (s, ev) => {
                                Application.Current.Dispatcher.Invoke(() => {
                                    PlayFile(TorrentSource.Series, ep);
                                }, DispatcherPriority.Send);
                            };
                            sender.Show();
                        }
                    }
                });
#pragma warning restore CS4014

                return downloader;
            } else {
                await MessageBox.Show("Torrent is already downloading");
                return null;
            }
        }

        public bool IsPaused { get; set; } = false;
        public async void Pause() {
            IsPaused = true;
            await Task.Run(() => {
                while (IsPaused) {
                    Handle.Pause();
                    Thread.Sleep(100);
                }
            });
        }

        public void Resume() {
            IsPaused = false;
        }

        public async void Remove(bool deleteFiles) {
            MainWindow.AddPage(new PleaseWait());
            string path = Status.SavePath + "\\" + Status.Name;
            string magnet = TorrentSource.Magnet;
            await Task.Run(() => {
                TorrentSession.Dispose();
                Status.Dispose();
                Handle.Dispose();
                Handle = null;
                Status = null;
                TorrentSession = null;
            });
            torrents.Remove(this);
            TorrentDatabase.Remove(magnet);
            if (deleteFiles) {
                if (Directory.Exists(path)) {
                    Directory.Delete(path, true);
                } else if (File.Exists(path)) {
                    File.Delete(path);
                }
            }
            MainWindow.RemovePage();
        }

        public async void PlayFile(Series series, Episode episode) {
            List<Episode.ScannedFile> list = new List<Episode.ScannedFile>();
            foreach (var item in episode.files) {
                if (item.Type == Episode.ScannedFile.FileType.Video) {
                    list.Add(item);
                }
            }
            List<FileInfo> infoList = new List<FileInfo>();
            foreach (var item in list) {
                infoList.Add(new FileInfo(item.NewName));
            }
            FileInfo info = infoList.OrderByDescending(ex => ex.Length).FirstOrDefault();
            if (info != null) {
                ScannedFile sf = list.Where(x => x.NewName == info.FullName).FirstOrDefault();
                //Used to release as many resources as possible to give all rendering power to video playback
                MainWindow.RemoveAllPages();
                MainWindow.SetPage(new BlankPage());
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                await Task.Run(() => {
                    Thread.Sleep(500);
                });
                MainWindow.AddPage(new LocalPlayer(series, episode, sf));
            }
        }

        private string GetDownloadPath() {
            if (!String.IsNullOrEmpty(Settings.DownloadCacheLocation)) {
                return Settings.DownloadCacheLocation;
            } else {
                string downloads = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();      
                var dialog = new VistaFolderBrowserDialog();
                dialog.SelectedPath = downloads + "\\Select Path";
                if ((bool)dialog.ShowDialog()) {
                    Settings.DownloadCacheLocation = dialog.SelectedPath;
                    return dialog.SelectedPath;
                } else {
                    return downloads;
                }
            }
        }

        public async static void ContinueUnfinished() {
            var torrents = TorrentDatabase.Load();
            torrents = torrents.Where(x => x.HasFinished == false).ToList();
            foreach (var item in torrents) {
                TorrentDatabase.Remove(item.Magnet);
                TorrentDownloader downloader = new TorrentDownloader(item);
                if (item.IsSequential) {
                    await downloader.Stream(false);
                } else {
                    await downloader.Download();
                }
            }
        }

    }
}