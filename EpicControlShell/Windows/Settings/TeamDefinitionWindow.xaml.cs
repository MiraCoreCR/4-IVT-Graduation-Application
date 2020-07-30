using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace EpicControlShell.Windows.Settings
{
    public partial class TeamDefinitionWindow : Window
    {
        private int ProjectId;

        EpicDbContext context = DatabaseProvider.GetInstance();

        List<UserInProject> usersInProject;
        List<User> users;
        List<PermissionGroup> groups;

        public TeamDefinitionWindow(int ProjectId)
        {
            InitializeComponent();

            this.ProjectId = ProjectId;

            PrepareToTeamDefinition();

            FillGridsAndComboBoxOnStart();
        }

        private void PrepareToTeamDefinition()
        {
            try
            {
                usersInProject = context.UserInProjects.Where(up => up.ProjectId == ProjectId).ToList();
                groups = context.PermissionGroups.ToList();

                var project = context.Projects.Where(p => p.Id == ProjectId).First();
                ProjectTitle.Text = project.Title;

                users = context.Users.ToList();

                foreach (var userInProject in usersInProject)
                {
                    var temp = context.Users.Where(u => u.Id == userInProject.UserId).First();
                    users.Remove(temp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }

        private void FillGridsAndComboBoxOnStart()
        {
            try
            {
                FillDataGridAndComboBoxSelectedGroup();
                UpdateAlreadyInProject_DataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillDataGridAndComboBoxSelectedGroup()
        {
            foreach (var group in groups)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem()
                {
                    Content = group.Title,
                    Tag = group.Id
                };

                SelectedGroup_ComboBox.Items.Add(comboBoxItem);
            }
        }

        #region AlreadyInProject
        private void UpdateAlreadyInProject_DataGrid()
        {
            AlreadyInProject_DataGrid.ItemsSource = null;
            AlreadyInProject_DataGrid.ItemsSource = usersInProject;
        }

        private void AlreadyInProject_DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedGroup_ComboBox.SelectedItem != null)
            {
                UserInProject userInProject = (UserInProject)AlreadyInProject_DataGrid.SelectedItem;
                usersInProject.Remove(userInProject);
                context.UserInProjects.Remove(userInProject);

                var user = context.Users.Where(u => u.Id == userInProject.UserId).First();
                if (users == null)
                {
                    users.Insert(0, user);
                }

                else
                {
                    users.Add(user);
                }

                SelectedGroup_ComboBox_SelectionChanged(null, null);
                UpdateAlreadyInProject_DataGrid();
            }

            else
                MessageBox.Show("Choose any group at first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                context.SaveChanges();

                MessageBox.Show("Succesfull", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region SelectedGroup
        private void SelectedGroup_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedGroup_DataGrid.ItemsSource = null;
            SelectedGroup_DataGrid.ItemsSource = users.Where(u => u.GroupId == Convert.ToInt32(((ComboBoxItem)SelectedGroup_ComboBox.SelectedItem).Tag)).ToList();
        }

        private void SelectedGroup_DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            User user = (User)SelectedGroup_DataGrid.SelectedItem;
            users.Remove(user);

            UserInProject userInProject = new UserInProject()
            {
                UserId = user.Id,
                User = user,
                ProjectId = this.ProjectId
            };
            usersInProject.Add(userInProject);
            context.UserInProjects.Add(userInProject);

            SelectedGroup_ComboBox_SelectionChanged(null, null);
            UpdateAlreadyInProject_DataGrid();
        }
        #endregion
    }
}
