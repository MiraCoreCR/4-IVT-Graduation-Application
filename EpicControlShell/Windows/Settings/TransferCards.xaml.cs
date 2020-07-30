using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System.Linq;
using System.Windows;

namespace EpicControlShell.Windows.Settings
{
    public partial class TransferCards : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        Card card;

        public TransferCards(int CardId)
        {
            InitializeComponent();

            FillEpicsDataGrid(CardId);
        }

        private void FillEpicsDataGrid(int CardId)
        {
            card = context.Cards.Where(c => c.Id == CardId).First();
            var epics = card.Epic.Project.Epics;

            EpicsDataGrid.ItemsSource = epics.Where(e => e.Id != card.EpicId).ToList();
        }

        private void EpicsDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedEpic = (Epic)EpicsDataGrid.SelectedItem;

                card.EpicId = selectedEpic.Id;
                card.Epic = selectedEpic;

                context.SaveChanges();

                MessageBox.Show("Epic successfully changed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
