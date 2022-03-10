using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;



namespace PlatformyTechnologiczne
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnExitItemMenuClick(object sender, RoutedEventArgs e)
        {
            //execute parent method
            this.Close();
        }

        private void OnOpenItemMenuClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result.Equals(System.Windows.Forms.DialogResult.OK)) LoadTreeView(dialog);
        }


        private void LoadTreeView(FolderBrowserDialog dialog)
        {
            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView)this.FindName("directoryTree");
            string rootDirectory = dialog.SelectedPath;
            string root = Path.GetFileName(rootDirectory);

            TreeViewItem rootItem = new TreeViewItem { Header = root, Tag = rootDirectory };
            tree.Items.Add(rootItem);

            string[] directories = Directory.GetDirectories(rootDirectory);
            string[] files = Directory.GetFiles(rootDirectory);

            for (int i = 0; i < directories.Length; i++)
            {
                string directoryName = Path.GetFileName(directories[i]);
                string directoryPath = directories[i];

                TreeViewItem item = new TreeViewItem
                {
                    Header = directoryName,
                    Tag = directoryPath
                };

                rootItem.Items.Add(item);
                LoadFiles(item, directoryPath);
            }

            TreeViewItem[] items = new TreeViewItem[files.Length];
            for (int i = 0; i < items.Length; i++)
            {
                string fileName = Path.GetFileName(files[i]);
                string directoryPath = files[i];

                TreeViewItem item = new TreeViewItem
                {
                    Header = fileName,
                    Tag = directoryPath
                };

                rootItem.Items.Add(item);
            }

        }

        private void LoadFiles(TreeViewItem parent,string path)
        {
            if (!Directory.Exists(path)) return;
            
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            if (files.Length == 0 && directories.Length == 0) return;


            for (int i = 0; i < directories.Length; i++)
            {
                string directoryName = Path.GetFileName(directories[i]);
                string directoryPath = directories[i];

                TreeViewItem item = new TreeViewItem
                {
                    Header = directoryName,
                    Tag = directoryPath
                };

                parent.Items.Add(item);
                LoadFiles(item, directoryPath);
            }

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileName(files[i]);
                string directoryPath = files[i];

                TreeViewItem item = new TreeViewItem
                {
                    Header = fileName,
                    Tag = directoryPath
                };

                parent.Items.Add(item);
            }


        }
    }
}
