
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace RustServerManager.Utils
{
    public class RustLogRconWatcher
    {
        private readonly RichTextBox _output;
        private readonly Dispatcher _dispatcher;
        private readonly Func<Task<string>> _fetchLogsAsync;
        private readonly HashSet<string> _seenLines = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly int _pollIntervalMs;
        private readonly RustMySqlLogger _logger;

        public RustLogRconWatcher(RichTextBox output, Func<Task<string>> fetchLogsAsync, string serverIdentity, int pollIntervalMs = 2000)
        {
            _output = output;
            _dispatcher = output.Dispatcher;
            _fetchLogsAsync = fetchLogsAsync;
            _pollIntervalMs = pollIntervalMs;
            _logger = new RustMySqlLogger(serverIdentity);
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _fetchLogsAsync();
                        var lines = result.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        var newLines = lines.Where(line => !_seenLines.Contains(line)).ToList();
                        foreach (var line in newLines)
                            _seenLines.Add(line);

                        if (_seenLines.Count > 1000)
                            _seenLines.Clear();

                        if (newLines.Count > 0)
                        {
                            _ = _dispatcher.InvokeAsync(() =>
                            {
                                foreach (var line in newLines)
                                    AppendLineToOutput(line);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogAsync(
                            logType: "RCON",
                            errorType: "PollLoop",
                            severity: "Error",
                            source: nameof(RustLogRconWatcher),
                            message: ex.Message,
                            details: ex.ToString());

                        _ = _dispatcher.InvokeAsync(() =>
                        {
                            AppendLineToOutput("[Watcher Error] " + ex.Message);
                        });
                    }

                    await Task.Delay(_pollIntervalMs);
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private void AppendLineToOutput(string line)
        {
            var color = GetColorForLine(line);
            var run = new Run(line + "\n") { Foreground = color };
            var paragraph = new Paragraph(run) { Margin = new System.Windows.Thickness(0) };
            _output.Document.Blocks.Add(paragraph);
            _output.ScrollToEnd();

            while (_output.Document.Blocks.Count > 1000)
                _output.Document.Blocks.Remove(_output.Document.Blocks.FirstBlock);
        }

        private static Brush GetColorForLine(string line)
        {
            string lower = line.ToLowerInvariant();

            if (lower.Contains("error")) return Brushes.Red;
            if (lower.Contains("warn")) return Brushes.Yellow;
            if (lower.Contains("connected") && lower.Contains("joined")) return Brushes.LimeGreen;
            if (lower.Contains("disconnected")) return Brushes.Red;
            if (lower.Contains("loaded")) return Brushes.DodgerBlue;
            if (lower.Contains("unloaded")) return Brushes.MediumPurple;
            if (lower.Contains("save")) return Brushes.LightBlue;
            if (lower.Contains("object")) return Brushes.Red;
            if (lower.Contains("oxide")) return Brushes.Orange;
            if (lower.Contains("rcon")) return Brushes.CadetBlue;

            return Brushes.White;
        }
    }
}
