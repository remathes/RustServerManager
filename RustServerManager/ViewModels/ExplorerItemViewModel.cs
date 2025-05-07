using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;

public class ExplorerItemViewModel : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public bool IsDirectory { get; set; }
    public PackIconKind IconKind => IsDirectory ? PackIconKind.Folder : PackIconKind.FileDocument;
    public string Type => IsDirectory ? "Folder" : Path.GetExtension(Name).ToUpper();
    public string Size => IsDirectory ? "" : new FileInfo(FullPath).Length.ToString("N0") + " bytes";
    public DateTime Modified => File.GetLastWriteTime(FullPath);

    public ObservableCollection<ExplorerItemViewModel> SubItems { get; set; } = new();

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged();
            if (value && SubItems.Count == 0)
                LoadSubDirectories();
        }
    }

    public void LoadSubDirectories()
    {
        try
        {
            SubItems.Clear();
            foreach (var dir in Directory.GetDirectories(FullPath))
            {
                SubItems.Add(new ExplorerItemViewModel
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    IsDirectory = true
                });
            }
        }
        catch { }
    }

    public ExplorerItemViewModel()
    {
        SubItems = new ObservableCollection<ExplorerItemViewModel>();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}