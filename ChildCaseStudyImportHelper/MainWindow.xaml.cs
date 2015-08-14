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
			if (_viewModel.FoundLocationCSVFile == false)
			{
				MessageBoxResult dlgResult = MessageBox.Show("Do you want to continue without the LocationID file? (not recommended)", "No Locations!", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
				if (dlgResult == MessageBoxResult.No)
				{
					this.Close();
				}
			}
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
			if (_viewModel.FixTempChildID)
			{
				if (MessageBox.Show("Are you sure you want to modify the Temporary Child ID?", "Fix Duplicate Temporary Child ID", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
				{
					_viewModel.FixTempChildID = false;
				}
				else
				{
					if (_viewModel.FixTempChildIDSuffix == null || _viewModel.FixTempChildIDSuffix.Length == 0)
					{
						MessageBox.Show("Please enter the suffix you would like to add to the Temporary Child ID to make it unique");
					}
					else
					{
						_viewModel.CreateCSV();
					}

				}
			}
			else
			{
				_viewModel.CreateCSV();
			}
            
        }

        private void OutputBrowseButtonClick(object sender, RoutedEventArgs e)
        {
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.CheckFileExists = false;
            ofd.Multiselect = false;
			ofd.Filter = "";
			ofd.FileName = System.IO.Path.GetFileName(_viewModel.OutputCSVFileName);
			ofd.InitialDirectory = System.IO.Path.GetDirectoryName (_viewModel.OutputCSVFileName);  
			bool? result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
				_viewModel.OutputCSVFileName = ofd.FileName;
            }

			/*
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            var result = fbd.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                string filename = System.IO.Path.GetFileName(_viewModel.OutputCSVFileName);

                _viewModel.OutputCSVFileName = System.IO.Path.Combine(fbd.SelectedPath, filename);
            }
			 */
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
