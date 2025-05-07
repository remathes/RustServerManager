using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;

namespace RustServerManager.Controls
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(string text)
        {
            Text = text;
        }

        public ImageSource Image => null;
        public string Text { get; private set; }
        public object Content => Text;
        public object Description => $"Command: {Text}";
        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // 🧹 Clean trailing junk before inserting
            var wordStart = FindCurrentWordStart(textArea.Document, completionSegment.Offset);
            textArea.Document.Replace(wordStart, completionSegment.EndOffset - wordStart, Text);
        }

        private int FindCurrentWordStart(TextDocument document, int offset)
        {
            while (offset > 0)
            {
                char c = document.GetCharAt(offset - 1);
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_') // Rust commands may have . or _
                    break;
                offset--;
            }
            return offset;
        }
    }
}