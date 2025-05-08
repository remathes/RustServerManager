using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RustServerManager.Controls
{
    public class CommandAutoCompleteProvider
    {
        private readonly List<string> _validCommands;

        public CommandAutoCompleteProvider(IEnumerable<string> commands)
        {
            _validCommands = commands?.ToList() ?? new();
        }

        public CompletionWindow ShowCompletion(TextEditor editor, string newlyTypedChar)
        {
            var completionWindow = new CompletionWindow(editor.TextArea);

            var typedText = GetCurrentWord(editor, newlyTypedChar).Trim();

            var filteredCommands = _validCommands
                .Where(c => c.StartsWith(typedText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c)
                .ToList();

            foreach (var cmd in filteredCommands)
            {
                completionWindow.CompletionList.CompletionData.Add(new CompletionData(cmd));
            }

            if (!filteredCommands.Any())
                return null;

            completionWindow.Show();
            return completionWindow;
        }

        private string GetCurrentWord(TextEditor editor, string newlyTypedChar)
        {
            var caretOffset = editor.CaretOffset;
            if (caretOffset == 0)
                return newlyTypedChar; // If empty, the first typed char is the word

            int startOffset = caretOffset - 1;
            while (startOffset > 0)
            {
                char ch = editor.Document.GetCharAt(startOffset);
                if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_')
                    break;
                startOffset--;
            }

            var word = editor.Document.GetText(startOffset + 1, caretOffset - startOffset - 1);
            return word + newlyTypedChar; // <<< append the new char manually
        }
    }
}