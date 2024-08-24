// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Collections;
using System.Security.Cryptography;
using DcsWaypointExporter.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DcsWaypointExporter.Tests
{
    public class TestsMain : IClassFixture<DependencyInjectionFixture>
    {
        private const string CAUCASUS_LUA = @".\Data\Caucasus.lua";
        private const string CAUCASUS_LUA_OUT = @".\Data\CaucasusOut.lua";
        private const string CAUCASUS_ADDED_LUA = @".\Data\CaucasusAdded.lua";
        private const string CAUCASUS_MISSING_LUA = @".\Data\CaucasusMissing.lua";
        private const string CAUCASUS_SORTED_LUA = @".\Data\CaucasusSorted.lua";
        private const string CAUCASUS_SORTED_LUA_OUT = @".\Data\CaucasusSortedOut.lua";
        private const string CAUCASUS_UNSORTED_LUA = @".\Data\CaucasusUnsorted.lua";
        private const string CAUCASUS_UNSORTED_LUA_OUT = @".\Data\CaucasusUnsortedOut.lua";

        private readonly IServiceProvider _serviceProvider;

        public TestsMain(DependencyInjectionFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }

        /// <summary>
        /// Dieser Test lädt eine Preset.Lua von der Festplatte und speichert diese danach wieder.
        /// Anschließend wird ein Hash-Vergleich der beiden Dateien durchgeführt um zu sehen ob diese identisch sind.
        /// </summary>
        [Fact(DisplayName = "PresetLua: Deserialize -> Serialize cycle")]
        public void PresetLua_ReadWriteCycle()
        {
            // Delete generated files.
            DeleteFile(CAUCASUS_LUA_OUT);

            var serializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var instance = serializer.DeserializeFromFile_XUnit(DcsMaps.Caucasus, CAUCASUS_LUA);
            Assert.NotNull(instance);
            Assert.True(serializer.SerializeToFile_XUnit(instance, CAUCASUS_LUA_OUT));
            Assert.True(File.Exists(CAUCASUS_LUA_OUT));

            // Compare Files
            Assert.True(FilesAreIdentical(CAUCASUS_LUA, CAUCASUS_LUA_OUT));

            // Delete generated files.
            DeleteFile(CAUCASUS_LUA_OUT);
        }

        /// <summary>
        /// Dieser Test Exportiert alle Missionen aus der Beispiel Preset.Lua.
        /// Anschließen werden diese wieder alle importiert und mit den vorherigen Missionen verglichen.
        /// </summary>
        [Fact(DisplayName = "Mission: Import -> Export cycle")]
        public void MissionImportExportCycle()
        {
            var presetLuaSerializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var presetLua = presetLuaSerializer.DeserializeFromFile_XUnit(DcsMaps.Caucasus, CAUCASUS_LUA);
            Assert.NotNull(presetLua);

            var missionSerializer = _serviceProvider.GetRequiredService<Services.IMissionImportExport>();

            foreach (var missionPair in presetLua.Missions)
            {
                Assert.NotNull(missionPair.Value);
                Assert.Equal(missionPair.Key, missionPair.Value.Name);

                Assert.True(missionSerializer.Export(DcsMaps.Caucasus, missionPair.Value, out var missionText));
                Assert.True(missionSerializer.Import(missionText, out var mapImported, out var missionImported));
                Assert.Equal(mapImported, DcsMaps.Caucasus);

                Assert.NotNull(missionImported);
                Assert.StrictEqual(missionImported.Waypoints.Count, missionPair.Value.Waypoints.Count);
                for (var i = 0; i < missionImported.Waypoints.Count; i++)
                {
                    var left = missionImported.Waypoints[i];
                    var right = missionPair.Value.Waypoints[i];

                    foreach (var l in left.Entries)
                    {
                        Assert.True(right.Entries.ContainsKey(l.Key));
                        var r = right.Entries[l.Key];
                        Assert.StrictEqual(l.Value, r);
                    }
                }
            }
        }

        [Fact]
        public void CorruptImportData()
        {
            var allText = File.ReadAllText(CAUCASUS_LUA);
            var presetLuaSerializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var presetLua = presetLuaSerializer.DeserializeFromString(DcsMaps.Caucasus, allText);
            Assert.NotNull(presetLua);

            var missionSerializer = _serviceProvider.GetRequiredService<Services.IMissionImportExport>();
            Assert.True(missionSerializer.Export(DcsMaps.Caucasus, presetLua.Missions.ElementAt(0).Value, out var exportedText));

            const int COUNT = 20;
            for (var i = 0; i < COUNT; i++)
            {
                var position = Random.Shared.Next(0, exportedText.Length - 1);
                var badText = exportedText.Remove(position, 1);
                Assert.False(missionSerializer.Import(badText, out var map, out var mission));
                Assert.Null(map);
                Assert.Null(mission);
            }
        }

        [Fact]
        public void IntegrityVerification_Added()
        {
            var allText = File.ReadAllText(CAUCASUS_ADDED_LUA);
            var presetLuaSerializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var presetLua = presetLuaSerializer.DeserializeFromString(DcsMaps.Caucasus, allText, true);
            Assert.Null(presetLua);
        }

        [Fact]
        public void IntegrityVerification_Missing()
        {
            var allText = File.ReadAllText(CAUCASUS_MISSING_LUA);
            var presetLuaSerializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var presetLua = presetLuaSerializer.DeserializeFromString(DcsMaps.Caucasus, allText, true);
            Assert.Null(presetLua);
        }

        [Fact]
        public void IntegrityVerification_Sorted()
        {
            // Delete generated files.
            DeleteFile(CAUCASUS_SORTED_LUA_OUT);

            // Speichern
            var serializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var instance = serializer.DeserializeFromFile_XUnit(DcsMaps.Caucasus, CAUCASUS_SORTED_LUA);
            Assert.NotNull(instance);
            Assert.True(serializer.SerializeToFile_XUnit(instance, CAUCASUS_SORTED_LUA_OUT));
            Assert.True(File.Exists(CAUCASUS_SORTED_LUA_OUT));

            // Compare Files
            Assert.True(FilesAreIdentical(CAUCASUS_SORTED_LUA, CAUCASUS_SORTED_LUA_OUT));

            // Delete generated files.
            DeleteFile(CAUCASUS_SORTED_LUA_OUT);
        }

        [Fact]
        public void IntegrityVerification_Unsorted()
        {
            // Delete generated files.
            DeleteFile(CAUCASUS_UNSORTED_LUA_OUT);

            // Speichern
            var serializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var instance = serializer.DeserializeFromFile_XUnit(DcsMaps.Caucasus, CAUCASUS_UNSORTED_LUA);
            Assert.NotNull(instance);
            Assert.True(serializer.SerializeToFile_XUnit(instance, CAUCASUS_UNSORTED_LUA_OUT));
            Assert.True(File.Exists(CAUCASUS_UNSORTED_LUA_OUT));

            // Compare Files
            var iden = FilesAreIdentical(CAUCASUS_UNSORTED_LUA, CAUCASUS_UNSORTED_LUA_OUT);
            Assert.False(iden);

            // Delete generated files.
            DeleteFile(CAUCASUS_UNSORTED_LUA_OUT);
        }


        // ----------------------------------------------------------------------------------------------------
        ///  Helper Methods
        // ----------------------------------------------------------------------------------------------------
        private static byte[] ComputeHash(HashAlgorithm hashAlgorithm, string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }
        private static bool FilesAreIdentical(string path1, string path2)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash1 = ComputeHash(sha256, path1);
                var hash2 = ComputeHash(sha256, path2);

                return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
            }
        }
        private static void DeleteFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            catch
            {
            }
        }
    }
}
