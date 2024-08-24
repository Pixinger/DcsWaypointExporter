// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Net.Http;
using System.Text.Json;

namespace DcsWaypointExporter.Services
{
    public class UpdateValidationService : IUpdateValidationService
    {
        #region nLog instance (s_log)
        private static NLog.Logger s_log { get; } = NLog.LogManager.GetCurrentClassLogger();
        #endregion


        public async Task<bool> HasUpdateAsync()
        {
            try
            {
                const string owner = "Pixinger";
                const string repo = "DcsWaypointExporter";
                const string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";

                using (var client = new HttpClient())
                {
                    s_log.Info("Requesting latest release from GitHub.");

                    // Benutzer-Agent setzen (wird von der GitHub API gefordert)
                    client.DefaultRequestHeaders.Add("User-Agent", repo);

                    var response = await client.GetStringAsync(url);
                    if (response is null)
                    {
                        s_log.Info("ERROR(debug): GetStringAsync(url) failed.");
                        return false;
                    }

                    // JSON-Daten parsen und den "tag_name" extrahieren
                    var jsonDoc = JsonDocument.Parse(response);
                    var remoteProductVersion = jsonDoc.RootElement.GetProperty("tag_name").GetString();
                    if (remoteProductVersion is null)
                    {
                        s_log.Info("ERROR(debug): jsonDoc.RootElement.GetProperty(\"tag_name\") failed.");
                        return false;
                    }

                    #region function: static Version getAssemblyVersion()
                    static Version getAssemblyVersion()
                    {
                        try
                        {
                            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                            if (assembly is null)
                            {
                                return new Version();
                            }

                            var name = assembly.GetName();
                            if (name is null)
                            {
                                return new Version();
                            }

                            var version = name.Version;
                            if (version is null)
                            {
                                return new Version();
                            }

                            return (Version)version.Clone();
                        }
                        catch
                        {
                            return new Version();
                        }
                    }
                    #endregion
                    var localVersion = getAssemblyVersion();
                    if (localVersion is null)
                    {
                        s_log.Info("ERROR(debug): getAssemblyVersion() failed.");
                        return false;
                    }
                    #region getAssemblyProductVersion ??
                    //#region static string? getAssemblyProductVersion()
                    //static string? getAssemblyProductVersion()
                    //{
                    //    var assemblyPath = Assembly.GetEntryAssembly()?.Location;
                    //    if (assemblyPath is null)
                    //    {
                    //        return null;
                    //    }

                    //    var filename = new FileInfo(assemblyPath).FullName;
                    //    if (!System.IO.File.Exists(filename))
                    //    {
                    //        return null;
                    //    }

                    //    try
                    //    {
                    //        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filename);
                    //        return fileVersionInfo.ProductVersion;
                    //    }
                    //    catch
                    //    {
                    //        return null;
                    //    }
                    //}
                    //#endregion
                    //var locaProductVersion = getAssemblyProductVersion();
                    //if (locaProductVersion is null)
                    //{
                    //    s_log.Info("ERROR(debug): getAssemblyProductVersion() failed.");
                    //    return false;
                    //}
                    #endregion

                    var hasUpdate = "v" + localVersion.ToString() != remoteProductVersion;
                    s_log.Info("HasUpdate: {0}", hasUpdate);
                    return hasUpdate;
                }
            }
            catch (Exception ex)
            {
                s_log.Error(ex, "Unexpected exception.");
                return false;
            }
        }
    }
}
