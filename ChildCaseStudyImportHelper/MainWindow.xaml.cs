using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace ChildCaseStudyImporter
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            this.DataContext = _viewModel;
        }

        private void ZipBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Zip Files|*.zip";

            bool? result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
               _viewModel.ZipFileNames = ofd.FileNames.ToList();
            }
        }

        private void CreateCSVButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateCSV();
        }

        private void OutputBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            var result = fbd.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                string filename = System.IO.Path.GetFileName(_viewModel.OutputCSVFileName);

                _viewModel.OutputCSVFileName = System.IO.Path.Combine(fbd.SelectedPath, filename);
            }
        }

        private void ZipFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            listBox.SelectedItem = null;
        }

        private void LocationsBrowseButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "CSV Files|*.csv";

            bool? result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                _viewModel.LocationCSVFileName = ofd.FileName;
            }
        }
    }
}
