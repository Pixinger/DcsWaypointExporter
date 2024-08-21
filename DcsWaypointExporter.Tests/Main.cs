// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Collections;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DcsWaypointExporter.Tests
{
    public class Main : IClassFixture<DependencyInjectionFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public Main(DependencyInjectionFixture fixture)
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
            const string filename1 = @".\Data\Caucasus.lua";
            const string filename2 = @".\Data\CaucasusOut.lua";
            try
            {
                if (File.Exists(filename2))
                    File.Delete(filename2);
            }
            catch
            {
                Assert.Fail(string.Format("Unable to delete file: {0}", filename2));
            }

            var serializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var instance = serializer.DeserializeFromFile_XUnit(filename1);
            Assert.NotNull(instance);
            Assert.True(serializer.SerializeToFile_XUnit(instance, filename2));
            Assert.True(File.Exists(filename2));

            // Compare Files
            #region function: static byte[] computeHash(HashAlgorithm hashAlgorithm, string filePath)
            static byte[] computeHash(HashAlgorithm hashAlgorithm, string filePath)
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return hashAlgorithm.ComputeHash(stream);
                }
            }
            #endregion
            #region function: static bool filesAreIdentical(string path1, string path2)
            static bool filesAreIdentical(string path1, string path2)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hash1 = computeHash(sha256, path1);
                    var hash2 = computeHash(sha256, path2);

                    return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
                }
            }
            #endregion
            Assert.True(filesAreIdentical(filename1, filename2));
        }

        /// <summary>
        /// Dieser Test Exportiert alle Missionen aus der Beispiel Preset.Lua.
        /// Anschließen werden diese wieder alle importiert und mit den vorherigen Missionen verglichen.
        /// </summary>
        [Fact(DisplayName = "Mission: Import -> Export cycle")]
        public void MissionImportExportCycle()
        {
            const string filename1 = @".\Data\Caucasus.lua";

            var presetLuaSerializer = _serviceProvider.GetRequiredService<Services.IPresetsLuaSerializer>();
            var presetLua = presetLuaSerializer.DeserializeFromFile_XUnit(filename1);
            Assert.NotNull(presetLua);

            var missionSerializer = _serviceProvider.GetRequiredService<Services.IMissionImportExport>();

            foreach (var missionPair in presetLua.Missions)
            {
                Assert.NotNull(missionPair.Value);
                Assert.Equal(missionPair.Key, missionPair.Value.Name);

                var missionText = missionSerializer.Export(missionPair.Value);
                var missionImported = missionSerializer.Import(missionText);

                Assert.NotNull(missionImported);
                Assert.StrictEqual(missionImported.Waypoints.Count, missionPair.Value.Waypoints.Count);
                for (int i = 0; i < missionImported.Waypoints.Count; i++)
                {
                    var left = missionImported.Waypoints[i];
                    var right = missionPair.Value.Waypoints[i];

                    foreach (KeyValuePair<string, dynamic> l in left.Entries)
                    {
                        Assert.True(right.Entries.ContainsKey(l.Key));
                        var r = right.Entries[l.Key];
                        Assert.StrictEqual(l.Value, r);
                    }
                }
            }
        }
    }
}
