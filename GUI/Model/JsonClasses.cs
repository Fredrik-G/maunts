using System.Collections.Generic;


namespace Maunts.Json
{
    /// <summary>
    /// Contains all objects needed for parsing 
    /// the character progression api.
    /// Classes generated using http://json2csharp.com
    /// This is the main json-class containing all progress information.
    /// </summary>
    public class RaidProgress
    {
        public string name { get; set; }
        public string realm { get; set; }
        public int achievementPoints { get; set; }
        public int faction { get; set; }
        public Progression progression { get; set; }
        public int totalHonorableKills { get; set; }
    }

    public class Progression
    {
        public List<Raid> raids { get; set; }
    }

    public class Raid
    {
        public string name { get; set; }
        public int lfr { get; set; }
        public int normal { get; set; }
        public int heroic { get; set; }
        public int mythic { get; set; }
        public int id { get; set; }
        public List<Boss> bosses { get; set; }
    }

    public class Boss
    {
        public int id { get; set; }
        public string name { get; set; }
        public int normalKills { get; set; }
        public int? heroicKills { get; set; }
        public int? lfrKills { get; set; }
        public int? mythicKills { get; set; }
    }
}
