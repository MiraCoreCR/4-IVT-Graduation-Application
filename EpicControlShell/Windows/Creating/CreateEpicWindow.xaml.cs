using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EpicControlShell.Windows.Creating
{
    /// <summary>
    /// Логика взаимодействия для CreateEpicWindow.xaml
    /// </summary>
    public partial class CreateEpicWindow : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        private int ProjectId;
        private bool isDataCorrect = false;
        DateTime startDate, deadLine;

        public CreateEpicWindow(int ProjectId)
        {
            InitializeComponent();
            this.ProjectId = ProjectId;

            try
            {
                var project = context.Projects.Where(p => p.Id == ProjectId).First();

                ProjectTitle.Text = project.Title;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillComboBox()
        {
            NextEpic_ComboBox.Items.Clear();

            var epics = context.Epics.Where(e => e.DeadLine > EpicDeadLine.SelectedDate).ToList();

            foreach (var epic in epics)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem()
                {
                    Content = epic.Title,
                    Tag = epic.Id
                };

                NextEpic_ComboBox.Items.Add(comboBoxItem);
            }

            ComboBoxItem NowhereComboBoxItem = new ComboBoxItem()
            {
                Content = "Nowhere",
                Tag = null
            };
            NextEpic_ComboBox.Items.Add(NowhereComboBoxItem);
        }

        private void CreateNewEpicButton_Click(object sender, RoutedEventArgs e)
        {
            CheckData();

            try
            {
                if (isDataCorrect)
                {
                    var epic = new Epic()
                    {
                        ProjectId = this.ProjectId,
                        Project = context.Projects.Where(p => p.Id == ProjectId).First(),
                        Title = EpicTitle.Text,
                        Description = EpicDescription.Text,
                        StartDay = startDate,
                        DeadLine = deadLine
                    };

                    context.Epics.Add(epic);
                    context.SaveChanges();

                    MessageBox.Show("Epic succesfully added", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EpicDeadLine_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FillComboBox();
        }

        private void CheckData()
        {
            if (EpicTitle.Text != "" && EpicDescription.Text != "")
            {
                if (EpicTitle.Text.Length > 100)
                    MessageBox.Show("Title length must not exceed 100 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                else if (EpicTitle.Text.Length > 500)
                    MessageBox.Show("Title length must not exceed 500 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                else if (EpicStartDate.Text != "" && EpicDeadLine.Text != "")
                {
                    try
                    {
                        startDate = Convert.ToDateTime(EpicStartDate.Text);
                        deadLine = Convert.ToDateTime(EpicDeadLine.Text);

                        if (deadLine < DateTime.Today)
                        {
                            MessageBox.Show("Deadline should not be overdue", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        else
                            isDataCorrect = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Incorrect date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                else
                    MessageBox.Show("Select All Dates!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            else
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
