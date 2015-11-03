using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Maunts
{
    /// <summary>
    /// Class containing various boss information.
    /// </summary>
    public class Boss : IEquatable<Boss>
    {
        #region Data

        public string Name { get; set; }
        public int Id { get; set; }
        public int NormalKills { get; set; }
        public int HeroicKills { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        public Boss(string name, int id)
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="otherBoss">The other Boss to copy from.</param>
        /// <param name="copyKills">Whether it should copy kills or not.</param>
        public Boss(Boss otherBoss, bool copyKills = true)
        {
            Name = otherBoss.Name;
            Id = otherBoss.Id;
            if(copyKills)
            {
                NormalKills = otherBoss.NormalKills;
                HeroicKills = otherBoss.HeroicKills;
            }
        }

        #endregion

        /// <summary>
        /// Used by IEquatable and determines if two Boss-objects are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Boss other) => Id == other.Id;
    }
}
