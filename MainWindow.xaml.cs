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

        private readonly string treeDirectoryName = "DirectoryTree";
        private readonly string textBlockName = "FileTextBox";

        public MainWindow()
        {
            InitializeComponent();
            
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

        private void LoadTreeView(FolderBrowserDialog dialog)
        {
            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView) FindName(treeDirectoryName);
            tree.Items.Clear();
            tree.UpdateLayout();

            string rootDirectory = dialog.SelectedPath;
            string root = Path.GetFileName(rootDirectory);

            TreeViewItem rootItem = CreateDirectoryTreeViewItem(root, rootDirectory, tree.Items);
            tree.Items.Add(rootItem);

            string[] directories = Directory.GetDirectories(rootDirectory);
            string[] files = Directory.GetFiles(rootDirectory);

            for (int i = 0; i < directories.Length; i++)
            {
                string directoryName = Path.GetFileName(directories[i]);
                string directoryPath = directories[i];

                TreeViewItem item = CreateDirectoryTreeViewItem(directoryName, directoryPath, rootItem.Items);
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

                TreeViewItem item = CreateDirectoryTreeViewItem(directoryName, directoryPath, parent.Items);

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

        private TreeViewItem CreateDirectoryTreeViewItem(string name, string path, ItemCollection items)
        {
            TreeViewItem treeItem = new TreeViewItem
            {
                Header = name,
                Tag = path
            };

            treeItem.Selected += OnSelectedTreeItem;

            ContextMenu menu = new ContextMenu();

            MenuItem createFolderFileMenuItem = new MenuItem { Header = "Create", Tag = new MenuItemMetaInfo(items, treeItem) };
            createFolderFileMenuItem.Click += OnDirectoryCreateFolderFileClick;

            MenuItem menuItemDelete = new MenuItem { Header = "Delete", Tag = new MenuItemMetaInfo(items,treeItem) };
            menuItemDelete.Click += OnDirectoryDeleteMenuItemClick;

            menu.Items.Add(createFolderFileMenuItem);
            menu.Items.Add(menuItemDelete);

            treeItem.ContextMenu = menu;

            return treeItem;

        }

        private void OnSelectedTreeItem(object sender, RoutedEventArgs e)
        {
            if (sender is not TreeViewItem item) return;
            PropertiesTextBlock.Text = ""; // reset
            string filePath = (string) item.Tag;

            FileAttributes fileAttribute = File.GetAttributes(filePath);
            string rash = "";

            if (fileAttribute.HasFlag(FileAttributes.ReadOnly)) rash += "r";
            if (fileAttribute.HasFlag(FileAttributes.Archive)) rash += "a";
            if (fileAttribute.HasFlag(FileAttributes.System)) rash += "s";
            if (fileAttribute.HasFlag(FileAttributes.Hidden)) rash += "h";

            PropertiesTextBlock.Text = rash;
            e.Handled = true;
        }

        private void OnDirectoryCreateFolderFileClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            MenuItemMetaInfo meta = (MenuItemMetaInfo)item.Tag;
            CreateDirectoryOrFile(meta);
            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView) FindName(treeDirectoryName);
            tree.UpdateLayout();
        }

        private void CreateDirectoryOrFile(MenuItemMetaInfo meta)
        {
            CreateDialog dialog = new CreateDialog();
            dialog.ShowDialog();
            
            if (dialog.Cancel) return;

            try
            {
                if (dialog.DirectoryType.IsChecked != null ? (bool)dialog.DirectoryType.IsChecked : false)
                {
                    CreateDirectory(meta, dialog);
                } else if(dialog.FileType.IsChecked != null ? (bool) dialog.FileType.IsChecked : false) CreateFile(meta, dialog);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void CreateFile(MenuItemMetaInfo meta, CreateDialog dialog)
        {
            string currentDirectoryPath = (string)meta.TreeItem.Tag;
            if (!Directory.Exists(currentDirectoryPath)) return;

            string fileName = dialog.FileName.Text;
            string pathToFile = currentDirectoryPath + Path.DirectorySeparatorChar + fileName;

            if (File.Exists(pathToFile)) return;
            File.Create(pathToFile);
            SetFileAttributes(pathToFile, dialog);

            TreeViewItem item = CreateFileTreeViewItem(fileName, pathToFile, meta.TreeItem.Items);
            meta.TreeItem.Items.Add(item);
        }


        private void CreateDirectory(MenuItemMetaInfo meta, CreateDialog dialog)
        {
            string currentDirectoryPath = (string) meta.TreeItem.Tag;

            if (!Directory.Exists(currentDirectoryPath)) return;

            string folderName = dialog.FileName.Text;
            string newDirectoryPath = currentDirectoryPath + Path.DirectorySeparatorChar + folderName;

            Directory.CreateDirectory(newDirectoryPath);
            SetFileAttributes(newDirectoryPath, dialog);

            TreeViewItem item = CreateDirectoryTreeViewItem(folderName, newDirectoryPath, meta.TreeItem.Items);
            meta.TreeItem.Items.Add(item);
        }


        private void SetFileAttributes(string pathToFile, CreateDialog dialog)
        {
            if (dialog.ReadOnlyCheckbox.IsChecked != null ? (bool)dialog.ReadOnlyCheckbox.IsChecked : false)
            {
                File.SetAttributes(pathToFile,File.GetAttributes(pathToFile) | FileAttributes.ReadOnly);
            }

            if (dialog.ArchiveCheckbox.IsChecked != null ? (bool)dialog.ArchiveCheckbox.IsChecked : false)
            {
                File.SetAttributes(pathToFile, File.GetAttributes(pathToFile) | FileAttributes.Archive);
            }
           
            if (dialog.HiddenCheckbox.IsChecked != null ? (bool)dialog.HiddenCheckbox.IsChecked : false)
            {
                File.SetAttributes(pathToFile, File.GetAttributes(pathToFile) | FileAttributes.Hidden);
            }
           
            if (dialog.SystemCheckbox.IsChecked != null ? (bool)dialog.SystemCheckbox.IsChecked : false)
            {
                File.SetAttributes(pathToFile, File.GetAttributes(pathToFile) | FileAttributes.System);
            }
        }

        private void OnDirectoryDeleteMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            MenuItemMetaInfo meta = (MenuItemMetaInfo)item.Tag;
            string filePath = (string)meta.TreeItem.Tag;
            Directory.Delete(filePath);

            meta.ParentCollection.Remove(meta.TreeItem);

            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView) FindName(treeDirectoryName);
            tree.UpdateLayout();
        }

        private TreeViewItem CreateFileTreeViewItem(string name, string path, ItemCollection items)
        {
            TreeViewItem treeItem = new TreeViewItem
            {
                Header = name,
                Tag = path
            };


            treeItem.Selected += OnSelectedTreeItem;

            ContextMenu menu = new ContextMenu();

            string extension = Path.GetExtension(path);
            bool isTxt = extension == ".txt";

            if(isTxt)
            {
                MenuItem menuItemRead = new MenuItem { Header = "Open", Tag = new MenuItemMetaInfo(items, treeItem) };
                menuItemRead.Click += OnRouteMenuEventClick;
                menu.Items.Add(menuItemRead);
            }

            MenuItem deleteItemMenu = new MenuItem { Header = "Delete", Tag = new MenuItemMetaInfo(items,treeItem) };
            deleteItemMenu.Click += OnDeleteFileMenuItemClick;

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

            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView) FindName(treeDirectoryName);
            tree.UpdateLayout();
        }


        private void OnRouteMenuEventClick(object sender,EventArgs args)
        {
            if (sender is not MenuItem item) return;

            MenuItemMetaInfo meta = (MenuItemMetaInfo) item.Tag;
            string filePath = (string) meta.TreeItem.Tag;

            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox) FindName(textBlockName);
            box.Text = ""; // reset

            using (var textReader = File.OpenText(filePath))
            {
                string text = textReader.ReadToEnd();
                box.Text = text;
            }
        }

        

    }
}
