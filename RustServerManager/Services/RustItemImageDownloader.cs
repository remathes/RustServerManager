// RustItemImageDownloader.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RustServerManager.Services
{
    public class RustItemDefinition
    {
        public string shortname { get; set; }
    }

    public class RustItemImageDownloader
    {
        private readonly string _itemsFolder;
        private readonly string _iconsFolder;
        private readonly HttpClient _http = new();

        public RustItemImageDownloader(string itemsFolder, string iconsFolder)
        {
            _itemsFolder = itemsFolder;
            _iconsFolder = iconsFolder;
            if (!Directory.Exists(itemsFolder))
            {
                Debug.Write($"Directory not found {itemsFolder}");
                return;
            }    
                
            if(!Directory.Exists(iconsFolder))
                Directory.CreateDirectory(_iconsFolder);
        }

        public async Task DownloadAllAsync()
        {
            var shortnames = GetShortnamesFromJsonFiles(_itemsFolder);

            foreach (var shortname in shortnames)
            {
                string url = $"https://rustlabs.com/img/items/{shortname}.png";
                string dest = Path.Combine(_iconsFolder, $"{shortname}.png");

                if (File.Exists(dest))
                    continue;

                try
                {
                    var data = await _http.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(dest, data);
                    Debug.WriteLine($"Downloaded: {shortname}.png");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed: {shortname} — {ex.Message}");
                }
            }
        }

        private List<string> GetShortnamesFromJsonFiles(string folder)
        {
            var list = new List<string>();
            foreach (var file in Directory.GetFiles(folder, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("shortname", out var sn))
                    {
                        var name = sn.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                            list.Add(name);
                    }
                }
                catch
                {
                    Debug.WriteLine($"Invalid or unreadable: {file}");
                }
            }
            return list;
        }
    }
}
