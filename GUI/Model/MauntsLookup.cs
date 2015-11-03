using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Maunts.Json;
using Maunts.Properties;
using Newtonsoft.Json;

namespace Maunts
{
    /// <summary>
    /// "Base"/Main class for Maunts. 
    /// Contains methods for looking up players
    ///  and checking the amount of times they've defeated a boss.
    /// </summary>
    public class MauntsLookup
    {
        #region Data

        /// <summary>
        /// Base url to battle net API.
        /// </summary>
        private readonly string[] _baseUrl =
        {
            "http://eu.battle.net/api/wow/character/",
            "/",
            @"?fields=progression"
        };

        /// <summary>
        /// Retuns an URI-object pointing to a characters armory API.
        /// </summary>
        /// <param name="character">The character to setup an uri for.</param>
        /// <returns></returns>
        private Uri GetCharacterUri(Character character) =>
            new Uri(_baseUrl[0] + character.Realm + _baseUrl[1] + character.Name + _baseUrl[2]);

        /// <summary>
        /// List of all characters to lookup.
        /// </summary>
        public List<Character> Characters { get; set; } = new List<Character>();

        /// <summary>
        /// List of all bossIDs to lookup for each player.
        /// </summary>
        public List<Boss> BossIds { get; set; } = new List<Boss>();

        /// <summary>
        /// The number of currently processed characters.
        /// Used to determine when the async lookup is done.
        /// </summary>
        private int _processedCharacters;

        /// <summary>
        /// List containing webclients. Each player lookup gets its own webclient.
        /// </summary>
        private readonly List<WebClient> _webClients = new List<WebClient>();

        /// <summary>
        /// Event for signaling when the lookup is done processing all characters.
        /// </summary>
        public EventHandler DoneProcessingAllCharacters;

        /// <summary>
        /// Event for signaling an error during lookup.
        /// </summary>
        public EventHandler LookupError;

        #endregion

        #region General Maunts Methods

        /// <summary>
        /// The main function that runs the mount lookup.
        /// </summary>
        public void RunLookup()
        {
            Settings.Default.Reset();
            Settings.Default.Reload();

            _processedCharacters = 0;
            _webClients.Clear();
            Characters.Clear();

            ReadBossesFromSettings();
            ReadCharactersFromSettings();
            LookupCharacters();

            if (Characters.Count == 0)
            {
                DoneProcessingAllCharacters?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Looks up every character in the Character-list.
        /// </summary>
        private void LookupCharacters()
        {
            foreach (var character in Characters)
            {
                var uri = GetCharacterUri(character);
                ReadApiAsync(uri);
            }
        }

        /// <summary>
        /// Calculates the total amount of kills for each boss.
        /// Stores the total amount in a new object called "Total":
        /// </summary>
        public void CalculateTotalKills()
        {
            if (Characters.Count == 0)
            {
                return;
            }

            var total = new Character(Characters[0], copyKills: false)
            {
                Name = "Total", Realm = string.Empty
            };

            foreach (var character in Characters)
            {
                for (var i = 0; i < character.Bosses.Count; i++)
                {
                    total.Bosses[i].NormalKills += character.Bosses[i].NormalKills;
                    total.Bosses[i].HeroicKills += character.Bosses[i].HeroicKills;
                }
            }
            Characters.Add(total);
            Characters.Sort();
        }

        /// <summary>
        /// Processes a boss and stores info in a Character object.
        /// </summary>
        /// <param name="boss">The Boss object containg the amount of kills.</param>
        /// <param name="character">The Character object to save kill progress to.</param>
        private void ProcessKill(Json.Boss boss, Character character)
        {
            var bossStats = new Boss(boss.name, boss.id)
            {
                NormalKills = boss.normalKills,
                HeroicKills = boss.heroicKills ?? 0
            };
            character.Bosses.Add(bossStats);
        }

        #endregion

        #region User Settings Methods

        /// <summary>
        /// Reads in and saves bosses from user settings.
        /// This lets the user save&load settings between sessions.
        /// (Settings means characters&bosses in this context).
        /// </summary>
        public void ReadBossesFromSettings()
        {
            foreach (var boss in Helpers.ReadSettings(Settings.Default.Bosses).Where(x => x.Contains('#')))
            {
                AddNewBoss(boss.Split('#')[1], Convert.ToInt32(boss.Split('#')[0]));
            }
        }

        /// <summary>
        /// Reads in and saves characters from user settings.
        /// This lets the user save&load settings between sessions.
        /// (Settings means characters&bosses in this context).
        /// </summary>
        public void ReadCharactersFromSettings()
        {
            foreach (var character in Helpers.ReadSettings(Settings.Default.Characters).Where(x => x.Contains('(')))
            {
                var realm = character.Split(' ')[1].Split('(')[1].Split(')')[0];
                var name = character.Split(' ')[0];

                AddNewCharacter(name, realm);
            }
        }

        /// <summary>
        /// Saves the current characters from the character list to user settings.
        /// Skips the "Total"-object.
        /// </summary>
        public void SaveCurrentCharacters()
        {
            foreach (var character in Characters.Where(a => !a.Name.Equals("Total")))
            {
                var characterString = Helpers.GetCharacterString(character);
                if (!Settings.Default.Characters.Contains(characterString))
                {
                    Settings.Default.Characters.Add(characterString);
                }
            }
        }

        /// <summary>
        /// Saves the current bosses from the boss list to user settings.
        /// </summary>
        public void SaveCurrentBosses()
        {
            foreach (var boss in BossIds)
            {
                var bossString = Helpers.GetBossString(boss);
                if (!Settings.Default.Bosses.Contains(bossString))
                {
                    Settings.Default.Bosses.Add(bossString);
                }
            }
        }

        #endregion

        #region WebClient Methods

        /// <summary>
        /// Starts and asynchronously reads the sent uri.
        /// </summary>
        /// <param name="uri">The uri to look up.</param>
        public void ReadApiAsync(Uri uri)
        {
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.DownloadStringAsync(uri);
                webClient.DownloadStringCompleted += Webclient_DownloadStringCompleted;
                _webClients.Add(webClient);
            }
        }

        /// <summary>
        /// Occurs when the webclient is done downloading a string.
        /// It checks for errors and then handles read character data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Webclient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                foreach (var webClient in _webClients)
                {
                    webClient.CancelAsync();
                    webClient.DownloadStringCompleted -= Webclient_DownloadStringCompleted;
                }
                LookupError?.Invoke(this, e);
                return;
            }
            var characterProgress = JsonConvert.DeserializeObject<RaidProgress>(e.Result);
            var realm = characterProgress.realm.Replace(" ", "-");
            var character =
                Characters.Single(c => c.Equals(new Character(characterProgress.name, realm)));

            foreach (var boss in from bossId in BossIds
                                 from raid in characterProgress.progression.raids
                                 from boss in raid.bosses
                                 where boss.id == bossId.Id
                                 select boss)
            {

                ProcessKill(boss, character);
            }

            if (++_processedCharacters == Characters.Count)
            {
                DoneProcessingAllCharacters?.Invoke(this, new EventArgs());
            }
        }

        #endregion

        #region Add & Remove

        /// <summary>
        /// Adds a unique new boss to both the boss list and to user settings.
        /// </summary>
        /// <param name="name">The boss name.</param>
        /// <param name="id">The boss id.</param>
        public void AddNewBoss(string name, int id)
        {
            var boss = new Boss(name, id);
            if (!BossIds.Contains(boss))
            {
                BossIds.Add(boss);
                var bossString = Helpers.GetBossString(boss);
                if (!Settings.Default.Bosses.Contains(bossString))
                {
                    Settings.Default.Bosses.Add(bossString);
                }
            }
        }

        /// <summary>
        /// Adds a unique new character to both the character list and to user settings.
        /// </summary>
        /// <param name="name">The character name.</param>
        /// <param name="realm">The character realm.</param>
        public void AddNewCharacter(string name, string realm)
        {
            var character = new Character(name, realm.Replace(' ', '-'));
            if (!Characters.Contains(character))
            {
                Characters.Add(character);
                var characterString = Helpers.GetCharacterString(character);
                if (!Settings.Default.Characters.Contains(characterString))
                {
                    Settings.Default.Characters.Add(characterString);
                }
            }
        }

        /// <summary>
        /// Removes given boss from both the boss list and user settings.
        /// </summary>
        /// <param name="boss"></param>
        public void RemoveBoss(Boss boss)
        {
            BossIds.Remove(boss);
            Settings.Default.Bosses.Remove(Helpers.GetBossString(boss));
        }

        /// <summary>
        /// Removes given character from both the character list and user settings.
        /// </summary>
        /// <param name="character"></param>
        public void RemoveCharacter(Character character)
        {
            Characters.Remove(character);
            Settings.Default.Characters.Remove(Helpers.GetCharacterString(character));
        }

        #endregion
    }
}
