using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EpicControlShell.Windows.Settings
{
    public partial class ArchivalMaterialsWindow : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        private int ProjectId;

        List<Card> cards;

        public ArchivalMaterialsWindow(int ProjectId)
        {
            InitializeComponent();

            this.ProjectId = ProjectId;

            LoadArchivalCardsInDataGrid();
        }

        private void LoadArchivalCardsInDataGrid()
        {
            if (ArchivalCards_DataGrid.ItemsSource == null)
            {
                try
                {
                    cards = context.Cards.ToList();

                    ArchivalCards_DataGrid.ItemsSource = cards.Where(c => c.Epic.ProjectId == this.ProjectId).Where(c => c.Archival == true).ToList();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else
            {
                ArchivalCards_DataGrid.ItemsSource = null;
                ArchivalCards_DataGrid.ItemsSource = cards.Where(c => c.Epic.ProjectId == this.ProjectId).Where(c => c.Archival == true).ToList();
            }
        }

        private void ArchivalCards_DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedCard = (Card)ArchivalCards_DataGrid.SelectedItem;

            cards.Where(c => c.Id == selectedCard.Id).FirstOrDefault().Archival = false;

            LoadArchivalCardsInDataGrid();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            context.SaveChanges();

            this.DialogResult = true;
        }

        private void DeleteArchivalCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (ArchivalCards_DataGrid.SelectedItem != null)
            {
                var dialogResult = MessageBox.Show("Do you really want to delete this card?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    var selectedCard = (Card)ArchivalCards_DataGrid.SelectedItem;

                    context.Cards.Remove(cards.Where(c => c.Id == selectedCard.Id).FirstOrDefault());

                    LoadArchivalCardsInDataGrid();
                }
            }

            else
                MessageBox.Show("Select card firstly", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
