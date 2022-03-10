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
            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView)this.FindName("DirectoryTree");
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

                TreeViewItem item = CreateFileTreeViewItem(fileName, directoryPath);
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

                TreeViewItem item = CreateFileTreeViewItem(fileName, directoryPath);

                parent.Items.Add(item);
            }
        }

        private TreeViewItem CreateFileTreeViewItem(string fileName, string filepath)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = fileName,
                Tag = filepath,
                
            };

            item.ContextMenu = CreateContextMenu(filepath);
            return item;
        }

        private ContextMenu CreateContextMenu(string filePath)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem readItem = CreateReadMenuFileItem(filePath);
            menu.Items.Add(readItem);

            MenuItem delteItem = CreateDeleteMenuFileItem(filePath);
            menu.Items.Add(delteItem);

            return menu;
        }

        private MenuItem CreateDeleteMenuFileItem(string filePath)
        {
            MenuItem item = new MenuItem { Header = "Delete file", Tag = filePath };
            item.Click += OnDeleteFileMenuItemClick;
            return item;
        }

        private void OnDeleteFileMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            string path = (string) item.Tag;
            File.Delete(path);
        }

        private MenuItem CreateReadMenuFileItem(string filePath)
        {
            var menuItemResult = new MenuItem { Header = "Read File", Tag = filePath };
            menuItemResult.Click += OnRouteMenuEventClick;
            return menuItemResult;
        }

        private void OnRouteMenuEventClick(object sender,EventArgs args)
        {
            if (sender is not MenuItem item) return;

            string filePath = (string)item.Tag;

            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)this.FindName("FileTextBox");
            box.Text = ""; // reset

            using (var textReader = File.OpenText(filePath))
            {
                string text = textReader.ReadToEnd();
                box.Text = text;
            }
        }
    }
}
