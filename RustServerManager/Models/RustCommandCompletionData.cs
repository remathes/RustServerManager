using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;

namespace RustServerManager.Models
{
    public class RustCommandCompletionData : ICompletionData
    {
        public RustCommandCompletionData(string text, string description)
        {
            Text = text;
            Description = description;
        }

        public System.Windows.Media.ImageSource? Image => null; // optional icons

        public string Text { get; private set; }
        public object Content => Text;
        public object Description { get; private set; }
        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // 🛠 Find the real start of the partial word, not just replace tiny segment
            var document = textArea.Document;
            var startOffset = FindWordStart(document, completionSegment.Offset);
            var endOffset = completionSegment.EndOffset;

            document.Replace(startOffset, endOffset - startOffset, Text);
        }

        private int FindWordStart(TextDocument document, int offset)
        {
            while (offset > 0)
            {
                char c = document.GetCharAt(offset - 1);
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_')
                    break;
                offset--;
            }
            return offset;
        }
    }
}