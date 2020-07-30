using EpicControlShell.Resources.Entities;
using EpicControlShell.Resources;
using EpicControlShell.Windows.Settings;
using EpicControlShell.Windows.Auth;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows.Input;
using EpicControlShell.Pages.General;
using IdentityModel.OidcClient.Browser;

namespace EpicControlShell.Windows.General
{
    public partial class MainWindow : Window
    {
        private Auth0.OidcClient.Auth0Client client;

        private EpicDbContext context = DatabaseProvider.GetInstance();

        public RoadmapAndBoard roadmapAndBoardPage = new RoadmapAndBoard();
        public ProjectsAndGroups projectsAndGroupsPage = new ProjectsAndGroups();

        public MainWindow(Auth0.OidcClient.Auth0Client client)
        {
            InitializeComponent();

            this.client = client;

            HomeButton_Click(null, null);
            FrameArea.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
        }

        public void SetPage(string PageName)
        {
            if (PageName == "RoadmapAndBoard")
            {
                FrameArea.Content = roadmapAndBoardPage;
                HomeButton.Visibility = Visibility.Visible;
                HomeButton.IsEnabled = true;

                ProjectTeamButton.Visibility = Visibility.Visible;
                ProjectTeamButton.IsEnabled = true;

                InformationButtonAbouProject.Visibility = Visibility.Visible;
                InformationButtonAbouProject.IsEnabled = true;

                ArchiveMaterialsButton.Visibility = Visibility.Visible;
                ArchiveMaterialsButton.IsEnabled = true;
            }

            if (PageName == "ProjectsAndGroups")
            {
                FrameArea.Content = projectsAndGroupsPage;

                roadmapAndBoardPage.SetPageParametrsAllAtNull();
                projectsAndGroupsPage.ShowProjects();
                projectsAndGroupsPage.ShowGroups();

                HomeButton.Visibility = Visibility.Hidden;
                HomeButton.IsEnabled = false;

                ProjectTeamButton.Visibility = Visibility.Hidden;
                ProjectTeamButton.IsEnabled = false;

                InformationButtonAbouProject.Visibility = Visibility.Hidden;
                InformationButtonAbouProject.IsEnabled = false;

                ArchiveMaterialsButton.Visibility = Visibility.Hidden;
                ArchiveMaterialsButton.IsEnabled = false;
            }
        }


        #region Buttons
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage("ProjectsAndGroups");
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = new ProfileWindow((int)InformationAboutCurrnetUser.UserId);
            if (profileWindow.ShowDialog() == true)
            {
                LogOutOnGoogle();
                InformationAboutCurrnetUser.NullAllSettings();
                AuthorizationWindow authorization = new AuthorizationWindow();
                authorization.Show();
                this.Close();
            }
        }

        private void ProjectTeamButton_Click(object sender, RoutedEventArgs e)
        {
            TeamDefinitionWindow teamDefinitionWindow = new TeamDefinitionWindow(roadmapAndBoardPage.GetProjectId());
            teamDefinitionWindow.ShowDialog();
        }

        private void InformationButtonAbouProject_Click(object sender, RoutedEventArgs e)
        {
            if (roadmapAndBoardPage.EpicsList_WrapPanel.Children.Count > 1)
            {
                StatisticWindow statisticWindow = new StatisticWindow(roadmapAndBoardPage.GetProjectId());
                statisticWindow.ShowDialog();
            }

            else
                MessageBox.Show("Project don't have epics", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ArchiveMaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            ArchivalMaterialsWindow archivalMaterialsWindow = new ArchivalMaterialsWindow(roadmapAndBoardPage.GetProjectId());

            if (archivalMaterialsWindow.ShowDialog() == true)
            {
                roadmapAndBoardPage.ShowCardsInCurrentEpic();
            }
        }
        #endregion

        private async void LogOutOnGoogle()
        {
            if (client != null)
                await client.LogoutAsync();
        }
    }
}
