﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Shows.xaml
    /// </summary>
    public partial class Shows : Page {
        public Shows(){
            InitializeComponent();
            Action load;
            load = () => LoadShows();
            Thread t = new Thread(load.Invoke);
            t.Name = "Populate library";
            t.Start();
        }

        public void LoadShows() {
            for (int i = 0 ; i < DatabaseAPI.database.Shows.Count;i++) {
                Dispatcher.Invoke(new Action(() => {
                    ShowRectangle folder = new ShowRectangle(DatabaseAPI.database.Shows[i]);
                    GenerateRectangle(out folder, DatabaseAPI.database.Shows[i]);
                }), DispatcherPriority.Send);
            }
        }

        private void GenerateRectangle(out ShowRectangle folder,SelectedShows ss){
            folder = new ShowRectangle(ss);
            folder.library = this;
            List.Children.Add(folder);
        }

        public void RemoveRectangle(ShowRectangle show) {
            List.Children.Remove(show);
        }

        private void SearchInLibrary_Event(object sender, TextChangedEventArgs e) {
            if (List != null) {
                foreach (ShowRectangle sr in List.Children) {
                    if (sr.ShowName == null) {
                        sr.GenerateName();
                    }
                    if (Regex.IsMatch(SearchBox.Text, @"^[a-zA-Z0-9_ .-]*$", RegexOptions.None, new TimeSpan(0, 0, 1))) {
                        Match m = Regex.Match(sr.ShowName, SearchBox.Text, RegexOptions.IgnoreCase);
                        if (m.Success) {
                            sr.SearchEnable();
                        } else {
                            sr.SearchDisable();
                        }
                    } else {
                        sr.SearchEnable();
                    }
                }
            }
        }

        private void EraseSearchText_Event(object sender, MouseButtonEventArgs e) {
            SearchBox.Text = String.Empty;
        }

        private void ResetText_Event(object sender, RoutedEventArgs e) {
            if (string.Compare(SearchBox.Text, "Search show") == 0) {
                SearchBox.Text = string.Empty;
                SearchBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
    }
}
