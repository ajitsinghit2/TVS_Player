﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Window
    {
        public Library()
        {
            InitializeComponent();
            Random r = new Random();
            for (byte i = 0; i < 10; i++)
            {
                Grid folder = new Grid();
                GenerateRectangle(out folder, r);
            }

        }
        private void GenerateRectangle(out Grid folder, Random r)
        {
            var xaml = XamlWriter.Save(BaseRectangle);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            folder = (Grid)XamlReader.Load(xmlReader);
            Rectangle rect = (Rectangle)folder.Children[0];
            Color genCol = Color.FromRgb((byte)(r.NextDouble() * 255), (byte)(r.NextDouble() * 255), (byte)(r.NextDouble() * 255));
            rect.Fill = new SolidColorBrush(genCol);
            if (genCol.B > 128 || genCol.R > 128 || genCol.B > 128)
            {
                ((TextBlock)folder.Children[1]).Foreground = new SolidColorBrush(Colors.Black);
            }
            List.Children.Add(folder);
        }
        private void Quit_Event(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}