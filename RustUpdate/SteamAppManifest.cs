using System;
using System.Collections.Generic;
using System.IO;

namespace RustUpdate
{
    public class SteamAppManifest
    {
        public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();

        public SteamAppManifest(string filePath)
        {
            Load(filePath);
        }

        private void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Appmanifest file not found", filePath);
            }

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Skip lines that start with "{" or "}"
                        if (line.Trim().StartsWith("{") || line.Trim().StartsWith("}"))
                        {
                            continue;
                        }

                        // Find the key and value
                        int quoteIndex = line.IndexOf("\"");
                        if (quoteIndex != -1)
                        {
                            int keyEnd = line.IndexOf("\"", quoteIndex + 1);
                            if (keyEnd != -1)
                            {
                                string key = line.Substring(quoteIndex + 1, keyEnd - quoteIndex - 1);
                                int valueStart = line.IndexOf("\"", keyEnd + 1);
                                if (valueStart != -1)
                                {
                                    int valueEnd = line.LastIndexOf("\"");
                                    if (valueEnd != -1)
                                    {
                                        string value = line.Substring(valueStart + 1, valueEnd - valueStart - 1);
                                        if (!Data.ContainsKey(key))
                                        {
                                            Data.Add(key, value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error loading appmanifest file: {e.Message}", e);
            }
        }
    }
}