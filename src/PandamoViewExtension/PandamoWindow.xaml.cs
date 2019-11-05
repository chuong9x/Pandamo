﻿using MahApps.Metro.Controls;
using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace DynamoPandas.PandamoViewExtension
{
    /// <summary>
    /// Interaction logic for PandamoWindow.xaml
    /// </summary>
    public partial class PandamoWindow : MetroWindow
    {
        PandamoWindowViewModel pandamoVm;
        public PandamoWindow(PandamoWindowViewModel vm)
        {
            pandamoVm = vm;
            InitializeComponent();
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            pandamoVm.StartServer();
        }

        private void KillServerButton_Click(object sender, RoutedEventArgs e)
        {
            pandamoVm.KillServer();
        }
    }
}
