// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using DcsWaypointExporter.Enums;
using DcsWaypointExporter.Models;

namespace DcsWaypointExporter.Services
{
    public class MissionImportExport : IMissionImportExport
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        public bool Import(string? importedText, out DcsMaps? map, [NotNullWhen(true)] out Models.PresetsLua.Mission? mission)
        {
            if (string.IsNullOrWhiteSpace(importedText))
            {
                map = null;
                mission = null;
                return false;
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

                #region function: bool parseJson(string jsonText, out DcsMaps? map, out Models.PresetsLua.Mission? mission)
                bool parseJson(string jsonText, out DcsMaps? map, out Models.PresetsLua.Mission? mission)
                {

                    using (var jsonDoc = JsonDocument.Parse(jsonText))
                    {
                        var jsonRoot = jsonDoc.RootElement;

                        // Try to read map if available.
                        var mapText = jsonRoot.GetProperty("Map").GetString();
                        if (mapText is not null)
                        {
                            try
                            {
                                map = Enum.Parse<DcsMaps>(mapText);
                            }
                            catch (Exception ex)
                            {
                                s_log.Error(ex, "Enum.Parse<DcsMaps>(mapText)");
                                map = null;
                            }
                        }
                        else
                        {
                            map = null;
                        }


                        // Zugriff auf einzelne Elemente
                        var missionName = jsonRoot.GetProperty("Name").GetString();
                        if (missionName is null)
                        {
                            s_log.Error("Unable to read 'Name' element.");
                            mission = null;
                            return false;
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
                                        mission = null;
                                        return false;
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

                        mission = new Models.PresetsLua.Mission(missionName, waypoints);
                        return true;
                    }
                }
                #endregion
                return parseJson(allLines, out map, out mission);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception.");
                map = null;
                mission = null;
                return false;
            }
        }
        public bool Export(DcsMaps map, Models.PresetsLua.Mission mission, [NotNullWhen(true)] out string? exportedText)
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
                            // Custom JSON OBJECT TO IDENTIFY THE MAP
                            writer.WriteString("Map", map.ToString());
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
                    exportedText = encodeBase64(compressedStream);
                    return !string.IsNullOrWhiteSpace(exportedText);
                }
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception.");
                exportedText = null;
                return false;
            }
        }

    }
}
