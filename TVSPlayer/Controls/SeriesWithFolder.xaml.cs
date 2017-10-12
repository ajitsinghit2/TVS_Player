﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SeriesWithFolder.xaml
    /// </summary>
    public partial class SeriesWithFolder : UserControl {
        public SeriesWithFolder(int id) {
            InitializeComponent();
            this.id = id;
        }
        public int id;

        private void Detail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Storyboard open = FindResource("OpenDetails") as Storyboard;
            open.Completed += (se, ev) => { DetailsPart.Visibility = Visibility.Visible; };
            open.Begin(MainPart);
        }

        private void DetailsPart_MouseLeave(object sender, MouseEventArgs e) {
            DetailsPart.Visibility = Visibility.Hidden;
            Storyboard close = FindResource("CloseDetails") as Storyboard;
            close.Begin(MainPart);
        }
    }
}
