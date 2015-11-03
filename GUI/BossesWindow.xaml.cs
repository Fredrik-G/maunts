using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Maunts
{
    /// <summary>
    /// Interaction logic for BossesWindow.xaml
    /// </summary>
    public partial class BossesWindow : Window
    {
        #region Data

        private readonly MauntsLookup _mauntsLookup;
        private List<Boss> _oldBosses;

        #endregion

        #region Load/Constructors

        public BossesWindow(MauntsLookup mauntsLookup)
        {
            _mauntsLookup = mauntsLookup;
            InitializeComponent();
        }

        private void BossWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Left = Owner.Left + Owner.ActualWidth - 15;
            Top = Owner.Top;
            bossesDataGrid.ItemsSource = _mauntsLookup.BossIds;
        }

        #endregion

        #region General Methods

        /// <summary>
        /// Adds a new boss based on the textboxes to the boss list.
        /// </summary>
        private void AddNewBoss()
        {
            if (BossNameTextBox.Text == string.Empty ||
                BossIdTextBox.Text == string.Empty)                           
            {
                return;
            }
            var nameMatch = Regex.IsMatch(BossNameTextBox.Text, @"[^\w]+");
            var idMatch = Regex.IsMatch(BossIdTextBox.Text, @"[^\d]+");
            if(nameMatch)
            {
                MessageBox.Show("Name can only contain letters.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (idMatch)
            {
                MessageBox.Show("ID can only contain numbers.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            bossesDataGrid.ItemsSource = null;
            var bossName = BossNameTextBox.Text;
            var bossId = Convert.ToInt32(BossIdTextBox.Text);
            _mauntsLookup.AddNewBoss(bossName, bossId);
            bossesDataGrid.ItemsSource = _mauntsLookup.BossIds;
        }

        /// <summary>
        /// Deletes the selected bosses.
        /// Also saves the old bosses for giving the user the possibility to undo.
        /// </summary>
        private void DeleteSelectedBosses()
        {
            _oldBosses = _mauntsLookup.BossIds.ConvertAll(boss => new Boss(boss));
            foreach (Boss boss in bossesDataGrid.SelectedItems)
            {
                _mauntsLookup.RemoveBoss(boss);
            }
            bossesDataGrid.ItemsSource = null;
            bossesDataGrid.ItemsSource = _mauntsLookup.BossIds;
            UndoButton.Visibility = Visibility.Visible;
        }

        private void UndoDelete()
        {
            bossesDataGrid.ItemsSource = null;
            _mauntsLookup.BossIds = _oldBosses;
            _mauntsLookup.SaveCurrentBosses();
            bossesDataGrid.ItemsSource = _mauntsLookup.BossIds;
            UndoButton.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Window Events

        /// <summary>
        /// Occurs when the textbox is focused.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Text = string.Empty;
            textBox.GotFocus -= TextBox_GotFocus;
        }

        private void bossesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Delete && bossesDataGrid.SelectedItems.Count > 0)
            {
                DeleteSelectedBosses();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) => AddNewBoss();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteSelectedBosses();
        private void UndoButton_Click(object sender, RoutedEventArgs e) => UndoDelete();

        /// <summary>
        /// Occurs when a key is pressed inside a name textbox.
        /// Check if pressed key is allowed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Regex.IsMatch(e.Key.ToString(), @"[\d]"))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Occurs when a key is pressed inside a number textbox.
        /// Check if pressed key is allowed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Regex.IsMatch(e.Key.ToString(), @"[\d]"))
            {
                e.Handled = true;
            }
        }

        #endregion
    }
}
