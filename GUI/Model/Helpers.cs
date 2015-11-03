using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Maunts
{
    /// <summary>
    /// Provides various general helpers-methods.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets the procentage of people who received a mount based on character's kill amount.
        /// Formula is 1-(1-0.01)^total_kills, where 0.01 (1%) is the default drop rate.
        /// (RNG = Random Number Generator, aka this checks how lucky you are.)
        /// </summary>
        /// <param name="boss"></param>
        /// <returns></returns>
        public static double GetRNGForBoss(Boss boss)
        {
            const double dropRate = 0.01;
            return Math.Round(((1 - Math.Pow(1 - dropRate, boss.NormalKills + boss.HeroicKills)) * 100), 1);
        }

        /// <summary>
        /// Reads settings from a given StringCollection and saves valid items to a new list.
        /// </summary>
        /// <param name="strings">The setting strings to read/convert.</param>
        /// <returns>List containing valid setting-lines.</returns>
        public static List<string> ReadSettings(StringCollection strings)
        {
            return strings.Cast<string>().Where(line => !line.StartsWith("#") && !string.IsNullOrWhiteSpace(line)).ToList();
        }

        /// <summary>
        /// Returns the character string used in storing a character in settings.
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static string GetCharacterString(Character character)
        {
            return $"{character.Name} ({character.Realm})";
        }

        /// <summary>
        /// Returns the boss string used in storing a boss in settings.
        /// </summary>
        /// <param name="boss"></param>
        /// <returns></returns>
        public static string GetBossString(Boss boss)
        {
            return $"{boss.Id}#{boss.Name}";
        }

        /// <summary>
        /// Gets the row index for the clicked row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>Returns the index or null if not found.</returns>
        public static int? GetClickedRowIndex(object sender, MouseButtonEventArgs e)
        {
            var asd = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject);
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
            return row == null ? null : ItemsControl.ItemsControlFromItemContainer(row)?.ItemContainerGenerator.IndexFromContainer(row);
        }

        /// <summary>
        /// Checks if given window is open.
        /// </summary>
        /// <typeparam name="T">The type of window to check for.</typeparam>
        /// <param name="name">The name of the window to check for.</param>
        /// <returns></returns>
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        /// <summary>
        /// Enables or disables child controls for given type in parent given collection.
        /// </summary>
        /// <typeparam name="T">Type to enable/disable</typeparam>
        /// <param name="collection">The parent collection for all child controls.</param>
        /// <param name="enable">Enable or disable the controls.</param>
        public static void EnableDisableAllControlInCollection<T>(Grid collection, bool enable = true) 
            where T : Control
        {
           foreach (var item in collection.Children.OfType<T>())
           {
                item.IsEnabled = enable;
           }
        }

    }
}

