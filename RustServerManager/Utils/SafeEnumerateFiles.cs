using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustServerManager.Utils
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class SafeEnumerateFiles
    {
        public static Task<List<string>> EnumerateFilesSafeAsync(string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return Task.Run(() =>
            {
                var filesList = new List<string>();
                var pending = new Stack<string>();
                pending.Push(path);

                while (pending.Count > 0)
                {
                    var currentDir = pending.Pop();

                    try
                    {
                        foreach (var file in Directory.GetFiles(currentDir, searchPattern))
                        {
                            filesList.Add(file);
                        }
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (PathTooLongException) { continue; }

                    if (searchOption == SearchOption.AllDirectories)
                    {
                        try
                        {
                            foreach (var subDir in Directory.GetDirectories(currentDir))
                            {
                                pending.Push(subDir);
                            }
                        }
                        catch (UnauthorizedAccessException) { continue; }
                        catch (PathTooLongException) { continue; }
                    }
                }

                return filesList;
            });
        }
    }
}
