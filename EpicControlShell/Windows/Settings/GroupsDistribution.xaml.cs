using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EpicControlShell.Windows.Settings
{
    public partial class GroupsDistribution : Window
    {
        private int GroupId;

        List<User> users;
        PermissionGroup group;

        EpicDbContext context = DatabaseProvider.GetInstance();

        public GroupsDistribution(int GroupId)
        {
            InitializeComponent();
            this.GroupId = GroupId;

            ShowStartInfoOnThisWindow();
            FillCurrentDatagrid();
        }

        private void ShowStartInfoOnThisWindow()
        {
            try
            {
                var groups = context.PermissionGroups.ToList();                                 // Получаем все группы 

                foreach (var group in groups)
                {
                    if (Convert.ToInt32(group.Id) == GroupId)                                   // Если Id группы совпадает с Id выбранной группы
                    {                                                                           // Заполнить заголовок и права доступа
                        this.group = group;
                        GroupTitle.Text = group.Title;
                        CanViewAllProjects.IsChecked = group.CanViewAllProjects;
                        CanManageProjects.IsChecked = group.CanManageProject;
                        CanManageEpics.IsChecked = group.CanManageEpic;
                        CanCommentEpics.IsChecked = group.CanCommentEpic;
                        CanManageGroups.IsChecked = group.CanManageGroup;
                        CanManageCards.IsChecked = group.CanManageCard;
                        CanMarkAndCommentCards.IsChecked = group.CanMarkAndCommentCard;
                    }

                    else                                                                        // Иначе - заполнить ComboBox названиями групп 
                    {
                        ComboBoxItem comboBoxItem = new ComboBoxItem()
                        {
                            Content = group.Title,
                            Tag = group.Id
                        };

                        SelectedGroup_ComboBox.Items.Add(comboBoxItem);
                    }
                }

                users = context.Users.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region OtherGroup
        private void SelectedGroup_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Извеменение левой группы
        {
            SelectedGroup_DataGrid.ItemsSource = null;
            SelectedGroup_DataGrid.ItemsSource = users.Where(u => u.GroupId == Convert.ToInt32(((ComboBoxItem)SelectedGroup_ComboBox.SelectedItem).Tag)).ToList();
        }

        private void SelectedGroup_DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) // Двойной клик по пользователю в левой группе 
        {
            User user = (User)SelectedGroup_DataGrid.SelectedItem;
            user.GroupId = GroupId;

            FillCurrentDatagrid();
            SelectedGroup_ComboBox_SelectionChanged(null, null);
        }
        #endregion

        #region CurrentGroup
        private void FillCurrentDatagrid() // Заполнить список пользователей в правой группе 
        {
            CurrentGroup_DataGrid.ItemsSource = null;
            CurrentGroup_DataGrid.ItemsSource = users.Where(u => u.GroupId == GroupId).ToList();
        }

        private void CurrentGroup_DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) // Двойной клик по пользователю в правой группе 
        {
            if (SelectedGroup_ComboBox.SelectedItem != null)
            {
                User user = (User)CurrentGroup_DataGrid.SelectedItem;
                user.GroupId = Convert.ToInt32(((ComboBoxItem)SelectedGroup_ComboBox.SelectedItem).Tag);

                FillCurrentDatagrid();
                SelectedGroup_ComboBox_SelectionChanged(null, null);
            }

            else
            {
                MessageBox.Show("Choose any group at first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region AcceptOrDeleteThisGroup
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                group.Title = GroupTitle.Text;
                group.CanViewAllProjects = Convert.ToBoolean(CanViewAllProjects.IsChecked);
                group.CanManageProject = Convert.ToBoolean(CanManageProjects.IsChecked);
                group.CanManageEpic = Convert.ToBoolean(CanManageEpics.IsChecked);
                group.CanCommentEpic = Convert.ToBoolean(CanCommentEpics.IsChecked);
                group.CanManageGroup = Convert.ToBoolean(CanManageGroups.IsChecked);
                group.CanManageCard = Convert.ToBoolean(CanManageCards.IsChecked);
                group.CanMarkAndCommentCard = Convert.ToBoolean(CanMarkAndCommentCards.IsChecked);

                context.SaveChanges();

                MessageBox.Show("Succesfull", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete this?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (group.Title != "User")
                    {
                        var groupWithNameUser = context.PermissionGroups.Where(g => g.Title == "User").First();

                        foreach (var user in users)
                        {
                            if (user.GroupId == GroupId)
                                user.GroupId = groupWithNameUser.Id;
                        }

                        context.PermissionGroups.Remove(group);
                        context.SaveChanges();

                        this.DialogResult = true;
                    }

                    else
                        MessageBox.Show("Group with title \"User\" cannot be deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
