using EpicControlShell.Resources.Entities;
using EpicControlShell.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;

namespace EpicControlShell.Windows.Settings
{
    /// <summary>
    /// Логика взаимодействия для PurposeWindow.xaml
    /// </summary>
    public partial class PurposeWindow : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        int ProjectId, EpicOrCardId, ExecutorId;

        PurposeType purposeType;

        List<UserInProject> usersInProject;
        List<Executor> cardExecutors;

        enum PurposeType
        {
            Project,
            Epic,
            Card
        }

        public PurposeWindow(string sender, int EpicOrCardId, int ProjectId, int ExecutorId)
        {
            InitializeComponent();

            if (sender == "Epic")
            {
                purposeType = PurposeType.Epic;
            }

            else if (sender == "Project")
            {
                purposeType = PurposeType.Project;
            }

            else if (sender == "Card")
            {
                purposeType = PurposeType.Card;
                cardExecutors = context.Executors.Where(ce => ce.CardId == EpicOrCardId).ToList();

                if (ExecutorId == 0)
                {
                    DeleteButton.Visibility = Visibility.Hidden;
                    DeleteButton.IsEnabled = false;
                }
            }

            this.EpicOrCardId = EpicOrCardId;
            this.ProjectId = ProjectId;
            this.ExecutorId = ExecutorId;

            FillDataGrid();
        }

        private void AlreadyInProject_DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var userInPtoject = (UserInProject)AlreadyInProject_DataGrid.SelectedItem;

            if (purposeType == PurposeType.Project)
            {
                var project = context.Projects.Where(p => p.Id == ProjectId).First();

                project.MasterId = userInPtoject.UserId;

                MessageBox.Show("Project Master successfully set", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }

            else if (purposeType == PurposeType.Epic)
            {
                var epic = context.Epics.Where(ep => ep.Id == EpicOrCardId).First();

                epic.MasterId = userInPtoject.UserId;

                MessageBox.Show("Epic Master successfully set", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }

            else
            {
                if (ExecutorId != 0)
                {
                    ReplaceExecutor(userInPtoject.UserId);
                }
                else
                {
                    AddExecutor(userInPtoject.UserId);
                }

                MessageBox.Show("Card Executor successfully set", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }

            context.SaveChanges();
        }

        private void AddExecutor(int UserId)
        {
            if (cardExecutors.Count < 5)
            {
                Executor executor = new Executor()
                {
                    UserId = UserId,
                    CardId = EpicOrCardId
                };

                var changeExecutor = new ExecutorChange()
                {
                    CardId = EpicOrCardId,
                    OldExecutorId = null,
                    NewExecutorId = UserId,
                    Reason = ReasonTextBox.Text,
                    Date = DateTime.Today
                };
                context.ExecutorsChanges.Add(changeExecutor);

                context.Executors.Add(executor);
            }
            else
            {
                MessageBox.Show("There can't be more than five performers", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReplaceExecutor(int UserId)
        {
            var changeExecutor = new ExecutorChange()
            {
                CardId = EpicOrCardId,
                OldExecutorId = ExecutorId,
                NewExecutorId = UserId,
                Reason = ReasonTextBox.Text,
                Date = DateTime.Today
            };
            context.ExecutorsChanges.Add(changeExecutor);

            var cardExecutor = context.Executors.Where(ce => ce.UserId == ExecutorId).First();

            cardExecutor.UserId = UserId;
        }

        private void FillDataGrid()
        {
            usersInProject = context.UserInProjects.Where(u => u.ProjectId == ProjectId).ToList();
            AlreadyInProject_DataGrid.ItemsSource = usersInProject;
        }

        private void DeleteMaster_Click(object sender, RoutedEventArgs e)
        {
            if (purposeType == PurposeType.Epic)
            {
                var epic = context.Epics.Where(ep => ep.Id == EpicOrCardId).First();

                epic.MasterId = null;

                MessageBox.Show("Epic Master successfully delete", "Information", MessageBoxButton.OK, MessageBoxImage.Information);   
            }

            else if (purposeType == PurposeType.Project)
            {
                var project = context.Projects.Where(ep => ep.Id == ProjectId).First();

                project.MasterId = null;

                MessageBox.Show("Project Master successfully delete", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            else
            {
                var executor = context.Executors.Where(ce => ce.UserId == ExecutorId).First();

                context.Executors.Remove(executor);

                var changeExecutor = new ExecutorChange()
                {
                    CardId = EpicOrCardId,
                    OldExecutorId = ExecutorId,
                    NewExecutorId = null,
                    Reason = ReasonTextBox.Text,
                    Date = DateTime.Today
                };
                context.ExecutorsChanges.Add(changeExecutor);

                MessageBox.Show("Card Executor successfully delete", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.DialogResult = true;
            context.SaveChanges();
        }
    }
}
