// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.DependencyInjection;
using DcsWaypointExporter.Enums;
using DcsWaypointExporter.Extensions;
using DcsWaypointExporter.Models;
using static DcsWaypointExporter.Models.PresetsLua.Mission;

namespace DcsWaypointExporter.Services
{
    public class PresetsLuaSerializer : IPresetsLuaSerializer
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region private static readonly List<string> _expectedKeys
        private static readonly List<string> _expectedKeys = new List<string>([
            "speed_locked",
            "type",
            "action",
            "ETA_locked",
            "x",
            "y",
            "name",
            "alt",
            "alt_type",
            "ETA"]);
        #endregion
        private const int EXPECTED_VALUES_COUNT = 10;

        public bool SerializeToFile_XUnit(PresetsLua presets, string filename) => SerializeToFile(presets, filename);
        public bool SerializeToFile(PresetsLua presets, DcsMaps file)
        {
            try
            {
                var filename = Ioc.Default.GetRequiredService<ISettingsService>().GetFullFilename(file);
                if (filename is null)
                {
                    s_log.Error("Unable to get filename for file {0}", file);
                    return false;
                }

                var allText = SerializeToString(presets);
                if (allText is null)
                {
                    s_log.Error("Unable to serialize file ({0}/{1}).", file, filename);
                    return false;
                }

                try
                {
                    File.WriteAllText(filename, allText, new UTF8Encoding(false));
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Unable to write file ({0}/{1}).", file, filename);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception");
                return false;
            }
        }
        public bool SerializeToFile(PresetsLua presets, string filename)
        {
            try
            {
                var allText = SerializeToString(presets);
                if (allText is null)
                {
                    s_log.Error("Unable to serialize file ({0}).", filename);
                    return false;
                }

                try
                {
                    File.WriteAllText(filename, allText, new UTF8Encoding(false));
                }
                catch (Exception ex)
                {
                    s_log.Error(ex, "Unable to write file ({0}).", filename);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception");
                return false;
            }
        }
        public string? SerializeToString(PresetsLua presets)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("presets = \n");
                sb.Append("{\n");
                foreach (var mission in presets.Missions)
                {
                    var name = mission.Key;
                    var waypoints = mission.Value.Waypoints;

                    sb.AppendIndent(1);
                    sb.Append(@"[""");
                    sb.Append(name);
                    sb.Append("\"] = \n");

                    sb.AppendIndent(1);
                    sb.Append("{\n");

                    #region function: void writeWaypoints(List<Waypoint> waypoints)
                    void writeWaypoints(List<Waypoint> waypoints)
                    {
                        for (var i = 1; i < waypoints.Count + 1; i++)
                        {
                            var waypoint = waypoints[i - 1];
                            sb.AppendIndent(2);
                            sb.Append(@"[");
                            sb.Append(i.ToString());
                            sb.Append("] = \n");

                            sb.AppendIndent(2);
                            sb.Append("{\n");

                            #region function: void writeWaypoint(Waypoint waypoint)
                            void writeWaypoint(Waypoint waypoint)
                            {
                                #region function: void writeBool(dynamic value)
                                void writeBool(dynamic value)
                                {
                                    if (value)
                                    {
                                        sb.Append("true");
                                    }
                                    else
                                    {
                                        sb.Append("false");
                                    }
                                }
                                #endregion
                                #region function: void writeString(string value)
                                void writeString(string value) => sb.Append($"\"{value}\"");
                                #endregion
                                #region function: void writeDouble(double value)
                                void writeDouble(double value) => sb.Append($"{value.ToString(CultureInfo.InvariantCulture.NumberFormat)}");
                                #endregion

                                foreach (var entry in waypoint.Entries)
                                {
                                    sb.AppendIndent(3);
                                    sb.Append(@"[""");
                                    sb.Append(entry.Key);
                                    sb.Append(@"""] = ");

                                    dynamic tt = entry.Value.GetType();
                                    if (entry.Value.GetType() == typeof(bool))
                                    {
                                        writeBool((bool)entry.Value);
                                    }
                                    else if (entry.Value.GetType() == typeof(double))
                                    {
                                        writeDouble((double)entry.Value);
                                    }
                                    else
                                    {
                                        writeString((string)entry.Value);
                                    }

                                    sb.Append(",\n");
                                }
                            }
                            #endregion
                            writeWaypoint(waypoint);

                            sb.AppendIndent(2);
                            sb.Append("}, -- end of [");
                            sb.Append(i.ToString());
                            sb.Append("]\n");
                        }
                    }
                    #endregion
                    writeWaypoints(waypoints);

                    sb.AppendIndent(1);
                    sb.Append("}, -- end of [\"");
                    sb.Append(mission.Key);
                    sb.Append("\"]\n");

                }
                sb.Append("} -- end of presets\n");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception.");
                return null;
            }
        }

        public PresetsLua? DeserializeFromFile_XUnit(DcsMaps map, string filename) => DeserializeFromFile(map, filename, true);
        public PresetsLua? DeserializeFromFile(DcsMaps map, bool noMessageBox)
        {
            try
            {
                var filename = Ioc.Default.GetRequiredService<ISettingsService>().GetFullFilename(map);
                if (filename is null)
                {
                    s_log.Error("Unable to get filename for file {0}", map);
                    return null;
                }

                return DeserializeFromFile(map, filename, noMessageBox);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception");
                return null;
            }
        }
        private PresetsLua? DeserializeFromFile(DcsMaps map, string filename, bool noMessageBox)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    s_log.Debug("File not found ({0}). Create a new one.", filename);
                    return new PresetsLua(map, new Dictionary<string, PresetsLua.Mission>());
                }

                var allText = File.ReadAllText(filename, new UTF8Encoding(false));
                if (allText is null)
                {
                    s_log.Error("Unable to read file ({0}).", filename);
                    return null;
                }

                return DeserializeFromString(map, allText, false);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception");
                return null;
            }
        }
        public PresetsLua? DeserializeFromString(DcsMaps map, string allText, bool noMessageBox)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(allText))
                {
                    return null;
                }

                // -- PRESETS --
                const string presetsExpression = @"(presets\s*=\s*{)(.*(} -- end of presets))";
                var match1 = Regex.Match(allText, presetsExpression, RegexOptions.Singleline);
                if (!match1.Success)
                {
                    return null;
                }

                // -- MISSIONS --
                var missionsText = match1.Groups[2].Value;

                const string missionsExpression = @"\[\""(.*?)\""\]\s*=\s*{(.*?)}, -- end of \[\"".*?\""\]";
                var matches2 = Regex.Matches(missionsText, missionsExpression, RegexOptions.Singleline);

                Dictionary<string, PresetsLua.Mission> missions = new();
                foreach (Match matchMissions in matches2)
                {
                    if (matchMissions.Success)
                    {
                        var missionName = matchMissions.Groups[1].Value;
                        var missionData = matchMissions.Groups[2].Value;

                        // -- WAYPOINTS --
                        const string waypointsExpression = @"\s*\[(\d+)\]\s*=\s*{(.*?)}, -- end of \[\d+\]";
                        var matches3 = Regex.Matches(missionData, waypointsExpression, RegexOptions.Singleline);

                        List<PresetsLua.Mission.Waypoint> waypoints = new();
                        foreach (Match matchWaypoint in matches3)
                        {
                            if (matchWaypoint.Success)
                            {
                                var waypoint = matchWaypoint.Groups[1].Value;
                                var waypointData = matchWaypoint.Groups[2].Value;

                                // -- ENTRIES --
                                const string entriesExpression = @"\[\""(.*?)\""\]\s*=\s*((\"".*?\"")|([^\"",]+)),?";
                                var matches4 = Regex.Matches(waypointData, entriesExpression, RegexOptions.Singleline);

                                var entries = new Dictionary<string, dynamic>();
                                foreach (Match matchEntries in matches4)
                                {
                                    if (matchEntries.Success)
                                    {
                                        var entryKey = matchEntries.Groups[1].Value;
                                        var entryValue = matchEntries.Groups[2].Value;

                                        // Integrity validation
                                        if (!_expectedKeys.Contains(entryKey))
                                        {
                                            if (!noMessageBox)
                                            {
                                                System.Windows.MessageBox.Show(CustomResources.Language.EdChangedFileFormat, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                            }

                                            return null;
                                        }

                                        #region function: static dynamic? getDynamic(string value)
                                        static dynamic? getDynamic(string value)
                                        {
                                            try
                                            {
                                                if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    return true;
                                                }

                                                if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    return false;
                                                }

                                                if (value.StartsWith('"') && value.EndsWith('"'))
                                                {
                                                    return value.Substring(1, value.Length - 2);
                                                }

                                                return Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                                            }
                                            catch (Exception ex)
                                            {
                                                s_log.Error(ex, "Unable to convert Entry-Value '{0}' from string to dynamic.", value);
                                                return null;
                                            }
                                        }
                                        #endregion
                                        var dynamicValue = getDynamic(entryValue);
                                        Debug.Assert(dynamicValue is not null);
                                        entries.Add(entryKey, dynamicValue);
                                    }
                                }

                                // Integrity validation
                                if (entries.Count != EXPECTED_VALUES_COUNT)
                                {
                                    if (!noMessageBox)
                                    {
                                        System.Windows.MessageBox.Show(CustomResources.Language.EdChangedFileFormat, CustomResources.Language.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                    }

                                    return null;
                                }

                                waypoints.Add(new PresetsLua.Mission.Waypoint(entries));
                            }
                        }

                        missions.Add(missionName, new PresetsLua.Mission(missionName, waypoints));
                    }
                }

                return new PresetsLua(map, missions);
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception while deserializing DcsPresets.");
                return null;
            }
        }

    }
}
