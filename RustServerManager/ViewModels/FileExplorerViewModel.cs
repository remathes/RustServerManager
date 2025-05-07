using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System;
using System.Windows.Input;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Diagnostics;

public class FileExplorerViewModel : INotifyPropertyChanged
{

    public ObservableCollection<ExplorerItemViewModel> RootItems { get; set; } = new();
    public ObservableCollection<ExplorerItemViewModel> CurrentItems { get; set; } = new();
    public ObservableCollection<BreadcrumbSegment> BreadcrumbSegments { get; set; } = new();

    private string _currentPath;
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            OnPropertyChanged();
            UpdateCurrentItems();
            UpdateBreadcrumb();
        }
    }

    public class BreadcrumbSegment
    {
        public string Name { get; set; }
        public ICommand NavigateCommand { get; set; }
    }

    public FileExplorerViewModel()
    {
        LoadDrives();
    }

    public void LoadDrives()
    {
        RootItems.Clear();
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            RootItems.Add(new ExplorerItemViewModel
            {
                Name = drive.Name,
                FullPath = drive.RootDirectory.FullName,
                IsDirectory = true
            });
        }
    }

    public void UpdateCurrentItems()
    {
        if (!Directory.Exists(CurrentPath)) return;

        CurrentItems.Clear();

        // List Folders
        try
        {
            foreach (var dir in Directory.GetDirectories(CurrentPath))
            {
                try
                {
                    CurrentItems.Add(new ExplorerItemViewModel
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir,
                        IsDirectory = true
                    });
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        catch
        {
            // Skip entire folder listing if top-level access fails
        }

        // List Files
        try
        {
            foreach (var file in Directory.GetFiles(CurrentPath))
            {
                try
                {
                    CurrentItems.Add(new ExplorerItemViewModel
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    });
                }
                catch
                {
                    
                }
            }
        }
        catch
        {
            // Skip entire file listing if access fails
        }
    }

    public void UpdateBreadcrumb()
    {
        BreadcrumbSegments.Clear();
        var parts = CurrentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        string buildPath = "";

        foreach (var part in parts)
        {
            buildPath = Path.Combine(buildPath, part);
            string pathCopy = buildPath;

            BreadcrumbSegments.Add(new BreadcrumbSegment
            {
                Name = part,
                NavigateCommand = new RelayCommand(() => CurrentPath = pathCopy)
            });
        }
    }
    private ExplorerItemViewModel _clipboardItem;
    private bool _isCopyOperation = false;

    public ICommand CopyCommand => new RelayCommand(CopySelectedItem, () => SelectedItem != null);
    public ICommand PasteCommand => new RelayCommand(PasteItem, () => _clipboardItem != null);

    public ExplorerItemViewModel SelectedItem { get; set; } // Set from DataGrid selection

    private void CopySelectedItem()
    {
        _clipboardItem = SelectedItem;
        _isCopyOperation = true;
    }

    private void PasteItem()
    {
        if (_clipboardItem == null || string.IsNullOrWhiteSpace(CurrentPath))
            return;

        try
        {
            var sourcePath = _clipboardItem.FullPath;
            var destPath = Path.Combine(CurrentPath, Path.GetFileName(sourcePath));

            if (_clipboardItem.IsDirectory)
            {
                CopyDirectory(sourcePath, destPath);
            }
            else
            {
                File.Copy(sourcePath, destPath, overwrite: true);
            }

            UpdateCurrentItems();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Paste failed: " + ex.Message);
        }
    }

    private void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var newDestDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, newDestDir);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) =>
            _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter) =>
            _execute((T)parameter);

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}