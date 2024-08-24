// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using DcsWaypointExporter.Enums;

namespace DcsWaypointExporter.Models
{
    public class PresetsLua
    {
        #region public class Mission
        public class Mission
        {
            #region  public class Waypoint 
            public class Waypoint
            {
                public Dictionary<string, dynamic> Entries { get; }


                public Waypoint(Dictionary<string, dynamic> entries)
                {
                    Entries = entries;
                }
            }
            #endregion

            public string Name { get; set; }
            public List<Waypoint> Waypoints { get; }

            public Mission(string name, List<Waypoint> waypoints)
            {
                Waypoints = waypoints;
                Name = name;
            }
        }
        #endregion

        public Dictionary<string, Mission> Missions { get; }
        public DcsMaps Map { get; }

        public PresetsLua(DcsMaps map, Dictionary<string, Mission> missions)
        {
            Map = map;
            Missions = missions;
        }
    }
}
