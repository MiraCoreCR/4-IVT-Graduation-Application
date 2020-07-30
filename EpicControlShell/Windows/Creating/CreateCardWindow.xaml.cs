using EpicControlShell.Resources.Entities;
using System;
using System.Windows;

namespace EpicControlShell.Windows.Creating
{
    /// <summary>
    /// Логика взаимодействия для CreateCardWindow.xaml
    /// </summary>
    public partial class CreateCardWindow : Window
    {
        int EpicId;

        bool isDataCorrect = false, isDateEmpty = true;

        DateTime dateTime;

        public CreateCardWindow(int EpicId, int status)
        {
            InitializeComponent();
            this.EpicId = EpicId;

            Status_ComboBox.SelectedIndex = status;
        }

        private void CreateNewCardButton_Click(object sender, RoutedEventArgs e)
        {
            CheckData();

            if (isDataCorrect)
            {
                try
                {
                    using (var context = new EpicDbContext())
                    {
                        if (isDateEmpty)
                        {
                            Card card = new Card()
                            {
                                Title = CardTitle.Text,
                                Status = Status_ComboBox.SelectedIndex,
                                Description = Description.Text,
                                DeadLine = null,
                                EpicId = this.EpicId
                            };
                            context.Cards.Add(card);
                            context.SaveChanges();
                        }
                        
                        else
                        {
                            Card card = new Card()
                            {
                                Title = CardTitle.Text,
                                Status = Status_ComboBox.SelectedIndex,
                                Description = Description.Text,
                                DeadLine = dateTime,
                                EpicId = this.EpicId
                            };
                            context.Cards.Add(card);
                            context.SaveChanges();
                        }

                        MessageBox.Show("Card succesfully added", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    this.DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CheckData()
        {
            if (CardTitle.Text != "" && Description.Text != "")
            {
                if (CardTitle.Text.Length > 100)
                    MessageBox.Show("Title length must not exceed 100 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                else if (Description.Text.Length > 500)
                    MessageBox.Show("Description length must not exceed 500 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                else if (DeadLine.Text != "")
                {
                    try
                    {
                        dateTime = Convert.ToDateTime(DeadLine.SelectedDate);

                        if (dateTime < DateTime.Today)
                        {
                            MessageBox.Show("Deadline should not be overdue", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        else
                        {
                            isDataCorrect = true;
                            isDateEmpty = false;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Incorrect date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                else
                    isDataCorrect = true;
            }

            else
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
