using ICSharpCode.AvalonEdit.CodeCompletion;
using RustServerManager.Controls;
using RustServerManager.Models;
using RustServerManager.Services;
using RustServerManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace RustServerManager.Views
{
    /// <summary>
    /// Interaction logic for RustServerConsoleWindow.xaml
    /// </summary>
    public partial class RustServerConsoleWindow : Window
    {
        private readonly RconClient _rconClient;
        private CancellationTokenSource? _connectRetryCts;
        private bool _connected = false;
        //private RconClient? _rconClient;
        private List<CommandSnippet> _snippets = new();
        private CommandAutoCompleteProvider _commandAutoCompleteProvider;
        private CompletionWindow _completionWindow;
        private readonly string _hostname;
        private readonly int _rconPort;
        private readonly string _rconPassword;
        private readonly string _logFilePath;
        private RustLogRconWatcher _logWatcher;
        private string Identity;

        public RustServerConsoleWindow(string hostname, int rconPort, string rconPassword, string identity,string logFilePath)
        {
            InitializeComponent();
            if (_rconClient == null)
            {
                _rconClient = new RconClient(identity);
                Identity = identity;
                _hostname = hostname;
                _rconPort = rconPort;
                _rconPassword = rconPassword;
                _logFilePath = logFilePath;
            }
            var list = File.ReadAllLines("QuickCommands.txt").Select(line => line.Split("===")[0].Trim()).Where(cmd => !string.IsNullOrEmpty(cmd)).ToList();
            var quickCommands = list.OrderBy(a => a);
            _commandAutoCompleteProvider = new CommandAutoCompleteProvider(quickCommands);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            SnippetsListBox.Items.Clear();
            LoadSnippets();
            SnippetsListBox.ItemsSource = _snippets;
            CommandInputEditor.TextArea.TextEntering += TextArea_TextEntering;
            CommandInputEditor.TextArea.TextEntered += TextArea_TextEntered;
            CommandInputEditor.TextArea.TextEntered += CommandInputEditor_TextArea_TextEntered;
            _ = TryConnectLoopAsync();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            _logWatcher = new RustLogRconWatcher(
                ServerLogBox, // your RichTextBox in this window
                async () =>
                {
                    var response = await _rconClient.SendCommandAsync("serverbridge.logtail 10");
                    return response ?? string.Empty; // fixed the .Output issue
                },
                  Identity,
                5000
            );
            _logWatcher.Start();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _logWatcher?.Stop();
            _logWatcher = null;
        }

        private void StartPulse()
        {
            Dispatcher.Invoke(() =>
            {
                var sb = (Storyboard)FindResource("PulseStoryboard");
                sb.Begin();
            });
        }

        private void StopPulse()
        {
            Dispatcher.Invoke(() =>
            {
                var sb = (Storyboard)FindResource("PulseStoryboard");
                sb.Stop();

                var resetSb = (Storyboard)FindResource("StopPulseStoryboard");
                resetSb.Begin();
            });
        }

        private void SetConnected(bool connected)
        {
            _connected = connected;

            Dispatcher.Invoke(() =>
            {
                if (connected)
                {
                    ConnectionIndicator.Fill = (Brush)Brushes.LimeGreen;
                    ConnectionStatusText.Text = $"Connected to {_hostname}:{_rconPort}";
                }
                else
                {
                    ConnectionIndicator.Fill = (Brush)Brushes.Crimson;
                    ConnectionStatusText.Text = $"Waiting to connect to server...";
                }
            });
        }
        private async Task TryConnectLoopAsync()
        {
            StartPulse();

            _connectRetryCts?.Cancel(); // Cancel previous loop if any
            _connectRetryCts = new CancellationTokenSource();
            var token = _connectRetryCts.Token;

            SetConnected(false);
            WatchLog.IsEnabled = false;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    bool connected = await _rconClient.EnsureConnectedAsync(_hostname, (ushort)_rconPort, _rconPassword);

                    if (connected)
                    {
                        StopPulse();
                        SetConnected(true);
                        WatchLog.IsEnabled = true;

                        System.Diagnostics.Debug.WriteLine($"[RCON Terminal] Connected to {_hostname}:{_rconPort}");
                        break; // ✅ Exit loop once connected
                    }

                    System.Diagnostics.Debug.WriteLine("[RCON Terminal] Server not ready, retrying in 5s...");
                    await Task.Delay(5000, token); // ⏳ Wait before retry
                }
                catch (TaskCanceledException)
                {
                    break; // Graceful cancel
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RCON Terminal] Error connecting: {ex.Message}");
                    await Task.Delay(5000, token); // Wait again after error
                }
            }
        }

        private async void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (_rconClient != null && _rconClient.IsConnected)
            {
                var commandText = CommandInputEditor.Text.Trim();
                if (!string.IsNullOrWhiteSpace(commandText))
                {
                    await _rconClient.SendCommandAsync(commandText);
                    CommandInputEditor.Clear();
                }
            }
        }

        private void CommandInputEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '.' && e.Text[0] != '/')
                return;

            string currentWord = GetCurrentWord(CommandInputEditor).Trim();
            if (string.IsNullOrEmpty(currentWord))
                return;

            // Always open popup if not open yet
            if (_completionWindow == null)
            {
                _completionWindow = new CompletionWindow(CommandInputEditor.TextArea);
                _completionWindow.Width = 300;
                _completionWindow.Closed += (o, args) => _completionWindow = null;
            }

            var data = _completionWindow.CompletionList.CompletionData;
            data.Clear(); // <== very important: Clear previous items

            foreach (var snippet in _snippets)
            {
                if (snippet.Command.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                {
                    data.Add(new RustCommandCompletionData(snippet.Command, snippet.Description));
                }
            }

            if (data.Count == 0)
            {
                _completionWindow.Close();
                _completionWindow = null;
            }
            else
            {
                if (!_completionWindow.IsVisible)
                {
                    _completionWindow.Show(); // Show only once when needed
                }
                // else: already open, just update contents
            }
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ServerLogBox.Document.Blocks.Clear();
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Only react if you typed a meaningful character
            if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '.' && e.Text[0] != '/')
                return;

            string currentWord = GetCurrentWord(CommandInputEditor).Trim();

            if (string.IsNullOrEmpty(currentWord))
                return;

            if (_completionWindow == null)
            {
                _completionWindow = new CompletionWindow(CommandInputEditor.TextArea);
                _completionWindow.Width = 300;
                _completionWindow.Closed += (o, args) => _completionWindow = null;
            }

            var data = _completionWindow.CompletionList.CompletionData;
            data.Clear();

            foreach (var snippet in _snippets)
            {
                if (snippet.Command.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                {
                    data.Add(new RustCommandCompletionData(snippet.Command, snippet.Description));
                }
            }

            if (data.Count == 0)
            {
                _completionWindow?.Close();
                _completionWindow = null;
            }
            else
            {
                if (_completionWindow != null && !_completionWindow.IsVisible)
                {
                    _completionWindow.Show();
                }
            }
        }

        private string GetCurrentWord(ICSharpCode.AvalonEdit.TextEditor editor)
        {
            int caretOffset = editor.CaretOffset;
            if (caretOffset == 0)
                return string.Empty;

            var document = editor.Document;
            int startOffset = caretOffset;

            while (startOffset > 0)
            {
                char c = document.GetCharAt(startOffset - 1);
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_')
                    break;
                startOffset--;
            }

            return document.GetText(startOffset, caretOffset - startOffset);
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (_completionWindow == null)
                return;

            // If user types something non-letter and non-digit, close completion
            if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '.')
            {
                _completionWindow.CompletionList.RequestInsertion(e); // okay to insert THEN close
            }
        }

        private void SnippetsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SnippetsListBox.SelectedItem is CommandSnippet snippet)
            {
                var caretOffset = CommandInputEditor.CaretOffset;
                CommandInputEditor.Document.Insert(caretOffset, snippet.Command);
                CommandInputEditor.Focus();
                CommandInputEditor.CaretOffset = caretOffset + snippet.Command.Length;
            }
        }
        private void LoadSnippets()
        {
            try
            {
                var lines = File.ReadAllLines("QuickCommands.txt").ToList();
                var sorted = lines.OrderBy(line => line.ToString());

                foreach (var line in sorted)
                {
                    var cleanline = line.Trim(); // <<< ADD THIS here
                    var parts = cleanline.Split("===", StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        _snippets.Add(new CommandSnippet
                        {
                            Command = parts[0].Trim(),
                            Description = parts[1].Trim()
                        });
                    }
                }

                SnippetsListBox.ItemsSource = _snippets;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load snippets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class CommandSnippet
        {
            public string Command { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"{Command} - ({Description})";
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logWatcher?.Stop();
            await _rconClient.DisconnectAsync();
            _connectRetryCts?.Cancel();

        }
    }
}
