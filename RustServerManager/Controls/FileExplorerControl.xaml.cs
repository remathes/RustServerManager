using MaterialDesignThemes.Wpf;
using RustServerManager.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RustServerManager.Controls
{
    public partial class FileExplorerControl : UserControl
    {
        public FileExplorerViewModel ViewModel { get; } = new FileExplorerViewModel();

        public FileExplorerControl()
        {
            InitializeComponent();
            DataContext = ViewModel;

            FolderTreeView.ItemsSource = ViewModel.RootItems;
            FileDataGrid.ItemsSource = ViewModel.CurrentItems;
            Breadcrumb.ItemsSource = ViewModel.BreadcrumbSegments;

            FolderTreeView.SelectedItemChanged += (s, e) =>
            {
                if (e.NewValue is ExplorerItemViewModel item && item.IsDirectory)
                    ViewModel.CurrentPath = item.FullPath;
            };

            FileDataGrid.MouseDoubleClick += (s, e) =>
            {
                if (FileDataGrid.SelectedItem is ExplorerItemViewModel item && item.IsDirectory)
                    ViewModel.CurrentPath = item.FullPath;
            };

            FileDataGrid.SelectionChanged += (s, e) =>
            {
                if (FileDataGrid.SelectedItem is ExplorerItemViewModel item)
                    ViewModel.SelectedItem = item;
            };
        }

        private Point _startPoint;

        private void FileDataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsMouseOverScrollbar((DependencyObject)e.OriginalSource))
                    return; // 💥 Don't drag from scrollbar

                _startPoint = e.GetPosition(null);
                var selectedItem = FileDataGrid.SelectedItem as ExplorerItemViewModel;
                if (selectedItem != null)
                {
                    DataObject dragData = new DataObject("fileItem", selectedItem);
                    DragDrop.DoDragDrop(FileDataGrid, dragData, DragDropEffects.Move);
                }
            }
        }

        private void FileDataGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent("fileItem") ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void FileDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("fileItem"))
            {
                var droppedItem = e.Data.GetData("fileItem") as ExplorerItemViewModel;
                var targetFolder = ViewModel.CurrentPath;

                if (droppedItem != null && targetFolder != droppedItem.FullPath)
                {
                    try
                    {
                        string targetPath = Path.Combine(targetFolder, Path.GetFileName(droppedItem.FullPath));

                        if (droppedItem.IsDirectory)
                        {
                            Directory.Move(droppedItem.FullPath, targetPath);
                        }
                        else
                        {
                            File.Move(droppedItem.FullPath, targetPath);
                        }

                        ViewModel.UpdateCurrentItems();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Move failed: " + ex.Message);
                    }
                }
            }
        }

        private bool IsMouseOverScrollbar(DependencyObject source)
        {
            while (source != null)
            {
                if (source is ScrollBar) return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        private void FolderTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (IsMouseOverScrollbar((DependencyObject)e.OriginalSource))
                return; // 💥 Don't drag from scrollbar

            DependencyObject source = (DependencyObject)e.OriginalSource;

            // 🚫 Ignore clicks on the expander toggle triangle
            while (source != null)
            {
                if (source is ToggleButton) return;
                source = VisualTreeHelper.GetParent(source);
            }

            if (FolderTreeView.SelectedItem is ExplorerItemViewModel selectedItem)
            {
                DataObject dragData = new DataObject("fileItem", selectedItem);
                DragDrop.DoDragDrop(FolderTreeView, dragData, DragDropEffects.Move);
            }
        }

        private void FolderTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("fileItem"))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private async void FolderTreeView_Drop(object sender, DragEventArgs e)
        {
            if (DialogHost.IsDialogOpen("MainDialog"))
            {
                // Optionally show a toast/snackbar instead
                ShowSnackbar("Please close the current dialog first.");
                return;
            }
            if (!e.Data.GetDataPresent("fileItem")) return;

            var draggedItem = e.Data.GetData("fileItem") as ExplorerItemViewModel;
            var target = GetNearestContainer(e.OriginalSource as DependencyObject);
            if (draggedItem == null || target?.DataContext is not ExplorerItemViewModel targetFolder || !targetFolder.IsDirectory)
                return;

            if (draggedItem.FullPath == targetFolder.FullPath ||
                draggedItem.FullPath.StartsWith(targetFolder.FullPath + Path.DirectorySeparatorChar))
            {
                ShowSnackbar("You cannot move a folder into itself or its subfolder.");
                return;
            }

            var dialog = new ConfirmMoveDialog
            {
                DataContext = new ConfirmMoveDialogViewModel
                {
                    Message = $"Move '{draggedItem.Name}' to '{targetFolder.Name}'?"
                }
            };

            var result = await DialogHost.Show(dialog, "MainDialog");

            if (!(result is bool confirmed && confirmed))
                return;

            try
            {
                string destPath = Path.Combine(targetFolder.FullPath, Path.GetFileName(draggedItem.FullPath));
                if (draggedItem.IsDirectory)
                    Directory.Move(draggedItem.FullPath, destPath);
                else
                    File.Move(draggedItem.FullPath, destPath);

                targetFolder.LoadSubDirectories();
                if (draggedItem.FullPath == ViewModel.CurrentPath)
                    ViewModel.UpdateCurrentItems();
            }
            catch (Exception ex)
            {
                ShowSnackbar("Move failed: " + ex.Message);
            }
        }

        public void ShowSnackbar(string message)
        {
            if (Application.Current.MainWindow is MainWindow mw &&
                mw.FindName("MainSnackbar") is Snackbar snackbar)
            {
                snackbar.MessageQueue?.Enqueue(message);
            }
        }

        // Helper to find TreeViewItem
        private TreeViewItem GetNearestContainer(DependencyObject obj)
        {
            while (obj != null && obj is not TreeViewItem)
                obj = VisualTreeHelper.GetParent(obj);
            return obj as TreeViewItem;
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ExplorerItemViewModel item && item.IsDirectory)
            {
                item.IsExpanded = true;            // Auto-expand
                item.LoadSubDirectories();         // Load subfolders
                ViewModel.CurrentPath = item.FullPath;  // Navigate right pane
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
menuItem.DataContext is ExplorerItemViewModel file)
            {
                MessageBox.Show($"Editing: {file.FullPath}");

                if (!File.Exists(file.FullPath))
                {
                    MessageBox.Show("File not found.");
                    return;
                }

                Process.Start("notepad.exe", $"\"{file.FullPath}\"");
            }

        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is ExplorerItemViewModel file)
            {
                var input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter new name:", "Rename File", file.Name);

                if (string.IsNullOrWhiteSpace(input) || input == file.Name) return;

                try
                {
                    var newPath = Path.Combine(Path.GetDirectoryName(file.FullPath), input);
                    File.Move(file.FullPath, newPath);
                    MessageBox.Show("Renamed successfully!");

                    ViewModel.UpdateCurrentItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Rename failed: " + ex.Message);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is ExplorerItemViewModel file)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete '{file.Name}'?",
                                              "Confirm Delete",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    if (file.IsDirectory)
                        Directory.Delete(file.FullPath, true);
                    else
                        File.Delete(file.FullPath);

                    MessageBox.Show("Deleted.");
                    ViewModel.UpdateCurrentItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete failed: " + ex.Message);
                }
            }
        }
    }
}