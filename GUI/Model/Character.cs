using System;
using System.Collections.Generic;
using System.Linq;

namespace Maunts
{
    /// <summary>
    /// Class containing information about a character.
    /// </summary>
    public class Character : IComparable<Character>, IEquatable<Character>
    {
        #region Data

        public string Name { get; set; }
        public string Realm { get; set; }
        public int Achis { get; set; }
        public List<Boss> Bosses { get; set; } = new List<Boss>();

        #endregion

        #region Constructors

        public Character(string name, string realm)
        {
            Name = name;
            Realm = realm;
        }

        public Character(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="otherCharacter">The other Character to copy from.</param>
        /// <param name="copyKills">Whether it should copy kills or not.</param>
        public Character(Character otherCharacter, bool copyKills = true)
        {
            Name = otherCharacter.Name;
            Realm = otherCharacter.Realm;
            Achis = otherCharacter.Achis;
            Bosses = otherCharacter.Bosses.ConvertAll(boss => new Boss(boss, copyKills));
        }

        #endregion

        /// <summary>
        /// Sorts the boss-list based on the amount of kills.
        /// </summary>
        public void SortBosses()
        {
            if (Bosses.Count > 0)
            {
                Bosses = Bosses.OrderByDescending((boss => boss.NormalKills + boss.HeroicKills)).ToList();
            }
        }

        /// <summary>
        /// Used by IComparale and compares two Character-objects.
        /// </summary>
        /// <param name="other">The other Character object to compare with.</param>
        /// <returns></returns>
        public int CompareTo(Character other)
        {
            if (other == null || Bosses.Count == 0)
            {
                return 1;
            }
            other.SortBosses();
            SortBosses();

            var kills = Bosses[0].NormalKills + Bosses[0].HeroicKills;
            var otherKills = other.Bosses[0].NormalKills + other.Bosses[0].HeroicKills;
            if (kills == otherKills)
            {
                return 1;
            }
            return kills > otherKills ? -1 : 1;
        }

        /// <summary>
        /// Used by IEquatable and determines if two Character-objects are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Character other)
        {
            return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase) &&
                   string.Equals(Realm, other.Realm, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
