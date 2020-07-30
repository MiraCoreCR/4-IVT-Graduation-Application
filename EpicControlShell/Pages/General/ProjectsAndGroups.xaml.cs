using EpicControlShell.Resources.Entities;
using EpicControlShell.Resources;
using EpicControlShell.Windows.Creating;
using EpicControlShell.Windows.General;
using EpicControlShell.Windows.Settings;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EpicControlShell.Pages.General
{
    public partial class ProjectsAndGroups : Page
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        private int IdProject;

        public ProjectsAndGroups()
        {
            InitializeComponent();
            InformationOnTheRight.Width = new GridLength(0, GridUnitType.Star);
        }

        #region InformationOnTheRight
        private void InformationOnTheRightAcceptChangesButton_Click(object sender, RoutedEventArgs e) // Применить изменения 
        {
            try
            {
                int ProjectId = Convert.ToInt32(((Button)e.OriginalSource).Tag);

                var project = context.Projects.Where(p => p.Id == ProjectId).First();

                if (project.Title != ProjectTitle.Text)
                    project.Title = ProjectTitle.Text;

                if (project.Description != ProjectDescription.Text)
                    project.Description = ProjectDescription.Text;

                context.SaveChanges();

                ShowProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InformationOnTheRightDeleteButton_Click(object sender, RoutedEventArgs e) // Удалить проект 
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete this?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var project = context.Projects.Where(p => p.Id == IdProject).FirstOrDefault();
                    context.Projects.Remove(project);
                    context.SaveChanges();

                    InformationOnTheRightCloseButton_Click(null, null);

                    ShowProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InformationOnTheRightCloseButton_Click(object sender, RoutedEventArgs e) // Закрыть информационный блок 
        {
            InformationOnTheRight.Width = new GridLength(0, GridUnitType.Star);
        }

        private void AssignResponsibleButton_Click(object sender, RoutedEventArgs e)
        {
            PurposeWindow purposeWindow = new PurposeWindow("Project", 0, IdProject, 0);
            purposeWindow.ShowDialog();
            ShowInformationAboutProject(IdProject);
        }
        #endregion

        #region Projects
        public void ShowProjects() // Показать проекты 
        {
            try
            {
                ProjectsList.Children.Clear();

                var projects = context.Projects.ToList();

                foreach (var project in projects)
                {
                    Grid grid = new Grid
                    {
                        Margin = new Thickness(5), 
                        Height = 80,
                        MinWidth = 200,
                        MaxWidth = 500
                    };

                    Button buttonBig = new Button
                    {
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                        Content = project.Title,
                        FontSize = 20,
                        Tag = project.Id
                    };
                    buttonBig.Click += new RoutedEventHandler(ProjectButton_Click);

                    Button buttonSmall = new Button
                    {
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                        BorderBrush = null,
                        Margin = new Thickness(2),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        MinHeight = 18,
                        Width = 25,
                        Content = Application.Current.FindResource("ShowIcon"),
                        Tag = project.Id
                    };
                    buttonSmall.Click += new RoutedEventHandler(ThreeDotsOnProjectButton_Click);

                    grid.Children.Add(buttonBig);
                    grid.Children.Add(buttonSmall);

                    if (project.Epics == null || project.Epics.Count == 0)
                    {
                        Label label = new Label()
                        {
                            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Content = Application.Current.FindResource("EmptyIcon"),
                            VerticalAlignment = VerticalAlignment.Bottom,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(5),
                            Height = 25,
                            Width = 25
                        };
                        grid.Children.Add(label);
                    }

                    ProjectsList.Children.Add(grid);
                }

                Button CreateNewProjectButton = new Button
                {
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    Content = "+ Add new project",
                    Margin = new Thickness(5, 5, 5, 5),
                    FontSize = 20,
                    Height = 80,
                    Width = 200,
                };
                CreateNewProjectButton.Click += new RoutedEventHandler(CreateNewProjectButton_Click);

                ProjectsList.Children.Add(CreateNewProjectButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e) // Нажатие на проект 
        {
            IdProject = 0;
            MainWindow mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.SetPage("RoadmapAndBoard");
            InformationOnTheRight.Width = new GridLength(0, GridUnitType.Star);

            mainWindow.roadmapAndBoardPage.SetProjectId(Convert.ToInt32(((Button)e.OriginalSource).Tag));
        }

        private void ThreeDotsOnProjectButton_Click(object sender, RoutedEventArgs e) // Нажатие на три точки 
        {
            InformationOnTheRight.Width = new GridLength(25, GridUnitType.Star);
            InformationOnTheRight_AcceptChanges.Tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            InformationOnTheRight_Delete.Tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            ShowInformationAboutProject(Convert.ToInt32(((Button)e.OriginalSource).Tag));
        }

        private void ShowInformationAboutProject(int ProjectId) // Вывод информации о проекте 
        {
            try
            {
                this.IdProject = ProjectId;
                var project = context.Projects.Where(p => p.Id == ProjectId).First();

                ProjectTitle.Text = project.Title;
                ProjectDescription.Text = project.Description;

                if (project.MasterId != null)
                {
                    Button button = new Button()
                    {
                        Content = Application.Current.FindResource("UserIcon"),
                        Height = 30,
                        Width = 30,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5, 20, 5, 0),
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        Tag = project.Master.Id
                    };
                    button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);

                    ToolTip toolTip = new ToolTip()
                    {
                        Content = project.Master.NickName + " - " + project.Master.FullName + "\n" + project.Master.Group.Title + "\n" + project.Master.Email
                    };
                    button.ToolTip = toolTip;

                    ProjectMasterGrid.Children.Add(button);
                }
                else
                {
                    Button button = new Button()
                    {
                        Content = Application.Current.FindResource("CreateNewIcon"),
                        Height = 30,
                        Width = 30,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(5, 20, 5, 0),
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                    };
                    button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);
                    ProjectMasterGrid.Children.Add(button);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateNewProjectButton_Click(object sender, RoutedEventArgs e) // Создание нового проекта 
        {
            var createProject = new CreateProjectWindow();
            if (createProject.ShowDialog() == true)
            {
                ShowProjects();
            }
        }
        #endregion

        #region Groups        
        public void ShowGroups() // Показать все группы 
        {
            try
            {
                GroupsList.Children.Clear();

                var groups = context.PermissionGroups.ToList();

                foreach (var group in groups)
                {
                    Button GroupButton = new Button
                    {
                        Content = group.Title,
                        Margin = new Thickness(5, 5, 5, 5),
                        Height = 50,
                        Width = 180,
                        Tag = group.Id
                    };
                    GroupButton.Click += new RoutedEventHandler(GroupButton_Click);

                    GroupsList.Children.Add(GroupButton);
                }

                Button addButton = new Button
                {
                    Content = "+ Add new Group",
                    Margin = new Thickness(5, 5, 5, 5),
                    Height = 50,
                    Width = 180
                };
                addButton.Click += new RoutedEventHandler(CreateNewGroupButton_Click);

                GroupsList.Children.Add(addButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e) // Клик по конкретной группе 
        {
            GroupsDistribution groupsDistribution = new GroupsDistribution(Convert.ToInt32(((Button)e.OriginalSource).Tag));
            if (groupsDistribution.ShowDialog() == true)
            {
                ShowGroups();
            }
        }

        private void CreateNewGroupButton_Click(object sender, RoutedEventArgs e) // Клик по кнопке "создать новую группу"
        {
            CreateGroupWindow createGroupWindow = new CreateGroupWindow();

            if (createGroupWindow.ShowDialog() == true)
                ShowGroups();
        }
        #endregion
    }
}
