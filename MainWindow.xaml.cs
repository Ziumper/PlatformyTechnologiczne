using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;



namespace PlatformyTechnologiczne
{
    public class MenuItemMetaInfo
    {
        public MenuItemMetaInfo(ItemCollection parentCollection, TreeViewItem treeItem)
        {
            this.parentCollection = parentCollection;
            this.treeItem = treeItem;
        }

        private ItemCollection parentCollection;
        private TreeViewItem treeItem;

        public ItemCollection ParentCollection => parentCollection;
        public TreeViewItem TreeItem => treeItem;
        
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadTreeView(FolderBrowserDialog dialog)
        {
            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView)this.FindName("DirectoryTree");
            tree.UpdateLayout();

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

                TreeViewItem item = CreateFileTreeViewItem(fileName, directoryPath,rootItem.Items);
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

                TreeViewItem item = CreateFileTreeViewItem(fileName, directoryPath,parent.Items);

                parent.Items.Add(item);
            }
        }

        private TreeViewItem CreateFileTreeViewItem(string name, string path, ItemCollection items)
        {
            TreeViewItem treeItem = new TreeViewItem
            {
                Header = name,
                Tag = path
            };

            ContextMenu menu = new ContextMenu();
            
            var menuItemResult = new MenuItem { Header = "Read", Tag = new MenuItemMetaInfo(items,treeItem) };
            menuItemResult.Click += OnRouteMenuEventClick;


            MenuItem deleteItemMenu = new MenuItem { Header = "Delete", Tag = new MenuItemMetaInfo(items,treeItem) };
            deleteItemMenu.Click += OnDeleteFileMenuItemClick;

            menu.Items.Add(menuItemResult);
            menu.Items.Add(deleteItemMenu);

            treeItem.ContextMenu = menu;

            return treeItem;
        }

        private void OnDeleteFileMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            MenuItemMetaInfo meta = (MenuItemMetaInfo)item.Tag;
            string filePath = (string)meta.TreeItem.Tag;
            File.Delete(filePath);

            meta.ParentCollection.Remove(meta.TreeItem);

            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView)this.FindName("DirectoryTree");
            tree.UpdateLayout();
        }


        private void OnRouteMenuEventClick(object sender,EventArgs args)
        {
            if (sender is not MenuItem item) return;

            MenuItemMetaInfo meta = (MenuItemMetaInfo) item.Tag;
            string filePath = (string) meta.TreeItem.Tag;

            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)this.FindName("FileTextBox");
            box.Text = ""; // reset

            using (var textReader = File.OpenText(filePath))
            {
                string text = textReader.ReadToEnd();
                box.Text = text;
            }
        }


        private void OnExitItemMenuClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnOpenItemMenuClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result.Equals(System.Windows.Forms.DialogResult.OK)) LoadTreeView(dialog);
        }

    }
}
