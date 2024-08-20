// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace DcsWaypointExporter.Services
{
    public class MissionImportExport : IMissionImportExport
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        public Models.PresetsLua.Mission? Import(string? importedText)
        {
            if (string.IsNullOrWhiteSpace(importedText))
            {
                return null;
            }

            try
            {
                #region function: byte[] decodeBase64(string text)
                byte[] decodeBase64(string text) => Convert.FromBase64String(importedText);
                #endregion
                var compresedBytes = decodeBase64(importedText);

                #region function: string decompress(byte[] compressedBuffer)
                string decompressBuffer(byte[] compressedBuffer)
                {
                    using (MemoryStream decompressedStream = new())
                    {
                        using (var compressedStream = new MemoryStream(compressedBuffer))
                        {
                            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                            {
                                gzipStream.CopyTo(decompressedStream);
                            }
                        }

                        decompressedStream.Position = 0;
                        using (var reader = new StreamReader(decompressedStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                #endregion
                var allLines = decompressBuffer(compresedBytes);
                Debug.Write(allLines);

                #region function: Models.PresetsLua.Mission? parseJson(string jsonText)
                Models.PresetsLua.Mission? parseJson(string jsonText)
                {
                    using (var jsonDoc = JsonDocument.Parse(jsonText))
                    {
                        var jsonRoot = jsonDoc.RootElement;

                        // Zugriff auf einzelne Elemente
                        var missionName = jsonRoot.GetProperty("Name").GetString();
                        if (missionName is null)
                        {
                            s_log.Error("Unable to read 'Name' element.");
                            return null;
                        }

                        var jsonWaypoints = jsonRoot.GetProperty("Waypoints");

                        List<Models.PresetsLua.Mission.Waypoint> waypoints = new();
                        var jsonWaypointsElements = jsonWaypoints.EnumerateObject();
                        foreach (var jsonWaypointProperty in jsonWaypointsElements)
                        {
                            Dictionary<string, dynamic> entries = new();
                            var jsonEntryElements = jsonWaypointProperty.Value.EnumerateObject();
                            foreach (var jsonEntryProperty in jsonEntryElements)
                            {
                                if (jsonEntryProperty.Value.ValueKind == JsonValueKind.String)
                                {
                                    var s = (dynamic?)jsonEntryProperty.Value.GetString();
                                    if (s is null)
                                    {
                                        s_log.Error("Unable to convert json entry: {0}", jsonEntryProperty.Name);
                                        return null;
                                    }

                                    entries.Add(jsonEntryProperty.Name, (dynamic)s);
                                }
                                else if (jsonEntryProperty.Value.ValueKind == JsonValueKind.False)
                                {
                                    entries.Add(jsonEntryProperty.Name, (dynamic)jsonEntryProperty.Value.GetBoolean());
                                }
                                else if (jsonEntryProperty.Value.ValueKind == JsonValueKind.True)
                                {
                                    entries.Add(jsonEntryProperty.Name, (dynamic)jsonEntryProperty.Value.GetBoolean());
                                }
                                else if (jsonEntryProperty.Value.ValueKind == JsonValueKind.Number)
                                {
                                    entries.Add(jsonEntryProperty.Name, (dynamic)jsonEntryProperty.Value.GetDouble());
                                }
                            }

                            waypoints.Add(new Models.PresetsLua.Mission.Waypoint(entries));
                        }

                        return new Models.PresetsLua.Mission(missionName, waypoints);
                    }
                }
                #endregion
                return parseJson(allLines);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception.");
                return null;
            }
        }
        public string? Export(Models.PresetsLua.Mission mission)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    #region function: void writeJson(Stream stream)
                    void writeJson(Stream stream)
                    {
#if DEBUG
                        const bool indented = true;
#else
                        const bool indented = false;
#endif
                        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = indented }))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("Name", mission.Name);
                            writer.WriteStartObject("Waypoints");
                            var waypoints = mission.Waypoints;
                            for (var i = 0; i < waypoints.Count; i++)
                            {
                                var waypoint = waypoints[i];
                                writer.WriteStartObject(i.ToString());
                                foreach (var entry in waypoint.Entries)
                                {
                                    dynamic tt = entry.Value.GetType();
                                    if (entry.Value.GetType() == typeof(bool))
                                    {
                                        writer.WriteBoolean(entry.Key, entry.Value);
                                    }
                                    else if (entry.Value.GetType() == typeof(double))
                                    {
                                        writer.WriteNumber(entry.Key, (double)entry.Value);
                                    }
                                    else
                                    {
                                        writer.WriteString(entry.Key, entry.Value);
                                    }
                                }
                                writer.WriteEndObject();
                            }
                            writer.WriteEndObject();
                            writer.WriteEndObject();
                            writer.Flush();
                        }
                    }
                    #endregion
                    writeJson(memoryStream);

                    #region function: MemoryStream compressStream(Stream stream)
                    MemoryStream compressStream(Stream stream)
                    {
                        stream.Position = 0;
                        var compressedStream = new MemoryStream();
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
                        {
                            stream.CopyTo(gzipStream);
                        }
                        return compressedStream;
                    }
                    #endregion
                    var compressedStream = compressStream(memoryStream);

                    #region function: string encodeBase64(MemoryStream stream)
                    string encodeBase64(MemoryStream stream)
                    {
                        compressedStream.Position = 0;
                        return Convert.ToBase64String(stream.ToArray());
                    }
                    #endregion
                    return encodeBase64(compressedStream);
                }
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception.");
                return null;
            }
        }
    }
}
