using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Maunts.Properties;

namespace Maunts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        #region Data

        private readonly MauntsLookup _mauntsLookup = new MauntsLookup();
        private readonly DataTable _rowDetailsTable = new DataTable();
        private BossesWindow _bossesWindow;

        private List<Character> _oldCharacters;

        #endregion

        #region Load/Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _mauntsLookup;
        }

        /// <summary>
        /// Occurs when the window is loaded.
        /// Starts the lookup-process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _mauntsLookup.DoneProcessingAllCharacters += Maunts_DoneProcessingAllCharacters;
            _mauntsLookup.LookupError += Maunts_LookupError;

            _rowDetailsTable.Columns.Add("Name", typeof(string));
            _rowDetailsTable.Columns.Add("Normal Kills", typeof(string));
            _rowDetailsTable.Columns.Add("Heroic Kills", typeof(string));
            _rowDetailsTable.Columns.Add("%", typeof(string));

            Run();
        }

        #endregion

        #region General Methods

        /// <summary>
        /// Starts the mount lookup.
        /// </summary>
        private void Run()
        {
            ActivateRunMode();
            _mauntsLookup.RunLookup();
        }

        private void ActivateRunMode()
        {
            CharacterProgressBar.Visibility = Visibility.Visible;
            CharacterProgressBar.IsIndeterminate = true;
            charactersDataGrid.ItemsSource = null;
            Helpers.EnableDisableAllControlInCollection<Button>(MauntsGrid, enable: false);
            Helpers.EnableDisableAllControlInCollection<TextBox>(MauntsGrid, enable: false);
        }

        private void ActivateNormalMode()
        {
            CharacterProgressBar.IsIndeterminate = false;
            CharacterProgressBar.Visibility = Visibility.Hidden;
            charactersDataGrid.ItemsSource = _mauntsLookup.Characters;
            Helpers.EnableDisableAllControlInCollection<Button>(MauntsGrid, enable: true);
            Helpers.EnableDisableAllControlInCollection<TextBox>(MauntsGrid, enable: true);
        }

        /// <summary>
        /// Displays character info in given row.
        /// </summary>
        /// <param name="rowIndex">The index for the row to display information in.</param>
        private void AddSelectedRowToRowDetails(int rowIndex)
        {
            _rowDetailsTable.Rows.Clear();
            _mauntsLookup.Characters[rowIndex].SortBosses();
            foreach (var boss in _mauntsLookup.Characters[rowIndex].Bosses)
            {
                var rng = Helpers.GetRNGForBoss(boss);
                _rowDetailsTable.Rows.Add(boss.Name, boss.NormalKills, boss.HeroicKills, rng);
            }
        }

        private void AddNewCharacter()
        {
            if (string.IsNullOrWhiteSpace(CharacterNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(CharacterNameTextBox.Text))
            {
                return;
            }

            var nameMatch = Regex.IsMatch(CharacterNameTextBox.Text, @"[^\w]+");
            var realmMatch = Regex.IsMatch(RealmTextBox.Text, @"[^[a-zA-Z0-9_]+( [a-zA-Z0-9_]+)*$");
            if (nameMatch)
            {
                MessageBox.Show("Name can only contain letters.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (realmMatch)
            {
                MessageBox.Show("Realm can only contain letters.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            charactersDataGrid.ItemsSource = null;
            var characterName = CharacterNameTextBox.Text;
            var realm = RealmTextBox.Text;

            _mauntsLookup.AddNewCharacter(characterName, realm);
            charactersDataGrid.ItemsSource = _mauntsLookup.Characters;
        }

        private void DeleteSelectedCharacters()
        {
            //Deep copy of character list.
            _oldCharacters = _mauntsLookup.Characters.ConvertAll(character => new Character(character));

            foreach (Character character in charactersDataGrid.SelectedItems)
            {
                _mauntsLookup.RemoveCharacter(character);
            }
            charactersDataGrid.ItemsSource = null;
            charactersDataGrid.ItemsSource = _mauntsLookup.Characters;
            UndoButton.Visibility = Visibility.Visible;
        }

        private void UndoDelete()
        {
            charactersDataGrid.ItemsSource = null;
            _mauntsLookup.Characters = _oldCharacters;
            _mauntsLookup.SaveCurrentCharacters();
            charactersDataGrid.ItemsSource = _mauntsLookup.Characters;

            UndoButton.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Datagrid Events

        /// <summary>
        /// Occurs when the character datagrid is clicked.
        /// Displays selected character info in its row details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void characterDataGrid_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
        {
            AddSelectedRowToRowDetails(e.Row.GetIndex());

            var innerDataGrid = e.DetailsElement as DataGrid;
            if (innerDataGrid != null)
            {
                innerDataGrid.CanUserAddRows = charactersDataGrid.CanUserDeleteRows;
                innerDataGrid.CanUserDeleteRows = charactersDataGrid.CanUserDeleteRows;
                innerDataGrid.CanUserResizeRows = charactersDataGrid.CanUserResizeRows;
                innerDataGrid.IsReadOnly = charactersDataGrid.IsReadOnly;
                innerDataGrid.ItemsSource = ((IListSource)_rowDetailsTable).GetList();
            }
        }

        private void charactersDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && charactersDataGrid.SelectedItems.Count > 0)
            {
                DeleteSelectedCharacters();
            }
        }

        #endregion

        #region Lookup Events

        /// <summary>
        /// Occurs when the async character lookup is completly done.
        /// Displays the result in the character datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maunts_DoneProcessingAllCharacters(object sender, EventArgs e)
        {
            if (_mauntsLookup.Characters.Count > 0)
            {
                _mauntsLookup.CalculateTotalKills();
                AddSelectedRowToRowDetails(0);
            }

            ActivateNormalMode();

            charactersDataGrid.RowDetailsVisibilityChanged += characterDataGrid_RowDetailsVisibilityChanged;
        }

        private void Maunts_LookupError(object sender, EventArgs e)
        {
            ActivateNormalMode();

            var lookupException = (e as DownloadStringCompletedEventArgs).Error as WebException;

            if (lookupException.Response == null)
            {
                MessageBox.Show($"Error:\n{lookupException.Message}", "Lookup Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            var name = lookupException.Response.ResponseUri.Segments[4];
            var realm = lookupException.Response.ResponseUri.Segments[3];
            MessageBox.Show($"Error while looking up character {name}-{realm}", "Lookup Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        #endregion

        #region Button Events

        private void AddRemoveBossesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Helpers.IsWindowOpen<BossesWindow>("BossWindow"))
            {
                _bossesWindow = new BossesWindow(_mauntsLookup) { Owner = this };
                _bossesWindow.Show();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) => AddNewCharacter();
        private void RefreshButton_Click(object sender, RoutedEventArgs e) => Run();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteSelectedCharacters();
        private void UndoButton_Click(object sender, RoutedEventArgs e) => UndoDelete();
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e) => Settings.Default.Save();

        /// <summary>
        /// Occurs when the textboxes got focus.
        /// Clears the textboxes and unsubscribes from the event, so it won't fire again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Text = string.Empty;
            textBox.GotFocus -= TextBox_GotFocus;
        }


        #region IDisposable methods

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _rowDetailsTable.Dispose();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
