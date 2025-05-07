using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace RustServerManager.Utils
{
    public class RustLogWatcher
    {
        private readonly RichTextBox _output;
        private readonly Dispatcher _dispatcher;
        private readonly string _folderPath;
        private readonly int _maxLines;
        private FileSystemWatcher _watcher;
        private CancellationTokenSource _cts;
        private string _currentLogFile;
        private long _lastPosition = 0;

        public RustLogWatcher(RichTextBox output, string folderPath, int maxLines = 1000)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _dispatcher = output.Dispatcher;
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            _maxLines = maxLines;

            StartWatching();
        }

        private void StartWatching()
        {
            if (!Directory.Exists(_folderPath))
                throw new DirectoryNotFoundException($"Log folder not found: {_folderPath}");

            _watcher = new FileSystemWatcher(_folderPath, "*.log")
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _watcher.Created += (_, _) => SwitchToNewestLog();
            _watcher.Changed += (_, _) => SwitchToNewestLog();

            SwitchToNewestLog(); // Initial attach
        }

        private void SwitchToNewestLog()
        {
            var newestLog = Directory.GetFiles(_folderPath, "*.log")
                .OrderByDescending(File.GetLastWriteTime)
                .FirstOrDefault();

            if (newestLog != null && newestLog != _currentLogFile)
            {
                _currentLogFile = newestLog;
                _lastPosition = 0;
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                StartReadingLog(_currentLogFile, _cts.Token);
            }
        }

        private void StartReadingLog(string filePath, CancellationToken token)
        {
            Task.Run(async () =>
            {
                var queue = new ConcurrentQueue<string>();

                // Pre-seek to end to avoid full initial read
                using var initStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                initStream.Seek(0, SeekOrigin.End);
                _lastPosition = initStream.Position;

                while (!token.IsCancellationRequested)
                {
                    if (!File.Exists(filePath)) break;

                    try
                    {
                        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        stream.Seek(_lastPosition, SeekOrigin.Begin);
                        using var reader = new StreamReader(stream);

                        var newText = await reader.ReadToEndAsync();
                        _lastPosition = stream.Position;

                        if (!string.IsNullOrWhiteSpace(newText))
                        {
                            var lines = newText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in lines)
                                queue.Enqueue(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        queue.Enqueue($"[Watcher Error] {ex.Message}");
                    }

                    await Task.Delay(200);

                    if (!queue.IsEmpty)
                    {
                        _ = _dispatcher.InvokeAsync(() =>
                        {
                            while (queue.TryDequeue(out var line))
                            {
                                AppendLineToOutput(line);
                            }
                        });
                    }
                }
            }, token);
        }

        private void AppendLineToOutput(string line)
        {
            if (line.Contains("An existing connection was forcibly closed by the remote host") || line.Contains("Disconnected: Exception"))
            {
                return; // ❌ skip adding this line
            }
            var color = GetColorForLine(line);
            string timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
            var run = new Run(timestamp + line + "\n") { Foreground = color };

            var paragraph = new Paragraph(run) { Margin = new Thickness(0) };
            _output.Document.Blocks.Add(paragraph);
            _output.ScrollToEnd();

            while (_output.Document.Blocks.Count > _maxLines)
            {
                _output.Document.Blocks.Remove(_output.Document.Blocks.FirstBlock);
            }
        }

        private static Brush GetColorForLine(string line)
        {
            string lower = line.ToLowerInvariant();

            if (lower.Contains("error"))
                return Brushes.Red;
            if (lower.Contains("warn"))
                return Brushes.Yellow;
            if (lower.Contains("connected") && lower.Contains("joined"))
                return Brushes.LimeGreen;
            if (lower.Contains("disconnected"))
                return Brushes.Red;
            if (lower.Contains("loaded"))
                return Brushes.DodgerBlue;
            if (lower.Contains("unloaded"))
                return Brushes.MediumPurple;
            if (lower.Contains("save"))
                return Brushes.LightBlue;
            if (lower.Contains("object"))
                return Brushes.Red;
            if (lower.Contains("oxide"))
                return Brushes.Orange;
            if (lower.Contains("rcon"))
                return Brushes.CadetBlue;

            return Brushes.White;
        }

        public void Stop()
        {
            _watcher?.Dispose();
            _cts?.Cancel();
        }
    }
}