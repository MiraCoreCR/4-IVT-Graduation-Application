using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using LiveCharts;
using LiveCharts.Wpf;

namespace EpicControlShell.Windows.Settings
{
    public partial class StatisticWindow : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        private Project CurrentProject;

        public StatisticWindow(int ProjectId)
        {
            InitializeComponent();

            LoadProjectStatistic(ProjectId);
        }

        private void LoadProjectStatistic(int ProjectId)
        {
            try
            {
                CurrentProject = context.Projects.Where(p => p.Id == ProjectId).FirstOrDefault();
                var epics = CurrentProject.Epics;

                LoadUpperInfo(epics);
                LoadLowerInfo(epics);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUpperInfo(ICollection<Epic> epics)
        {
            try
            {
                ProjectTitle.Content = CurrentProject.Title;

                EpicsCountTextBlock.Text = Convert.ToString(CurrentProject.Epics.Count) + " Epics";


                DateTime today = DateTime.Today, lastDeadLine = epics.Where(e => (e.DeadLine == epics.Max(l => l.DeadLine))).First().DeadLine;
                int overduesEpics = 0;

                foreach (var epic in epics)
                {
                    if (epic != null && epic.DeadLine < today && (epic.Cards != null && epic.Cards.Where(c => c.Status != 2).Count() != 0))
                    {
                        overduesEpics++;
                    }
                }

                LastEpicDeadLineTextBlock.Text = "Deadline: " + lastDeadLine.ToShortDateString();
                OverduesEpicsCountTextBlock.Text = Convert.ToString(overduesEpics) + " Epics overdues";

                var daysleft = lastDeadLine - today;

                DaysLeftTextBlock.Text = daysleft.TotalDays.ToString() + " Days left";

                var usersInProjectCount = context.UserInProjects.Where(u => u.ProjectId == CurrentProject.Id).Count();
                UsersInProjectCountTextBlock.Text = Convert.ToString(usersInProjectCount);

                DrawCardPieGraphics(epics);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLowerInfo(ICollection<Epic> epics) 
        {
            if (CurrentProject.Master != null)
                CurrentProjectMasterTextBlock.Text = Convert.ToString(CurrentProject.Master.FullName + " / " + CurrentProject.Master.NickName);

            else
                CurrentProjectMasterTextBlock.Text = "In this project, a master has been appointed";

            EpicMasters_DataGrid.ItemsSource = epics.Where(e => e.Master != null).ToList();

            foreach (var epic in epics)
            {
                if (epic != null && epic.Cards != null)
                {
                    var cards = epic.Cards;

                    foreach (var card in cards)
                    {
                        var cardExecutorsChanges = context.ExecutorsChanges.Where(ec => ec.CardId == card.Id).ToList();

                        if (cardExecutorsChanges != null && cardExecutorsChanges.Count != 0)
                        {
                            foreach (var change in cardExecutorsChanges)
                            {
                                Border border = new Border()
                                {
                                    BorderBrush = new SolidColorBrush(Color.FromArgb(130, 135, 135, 135)),
                                    BorderThickness = new Thickness(1, 1, 1, 1),
                                    Margin = new Thickness(5),
                                    Padding = new Thickness(5)
                                };

                                TextBlock textBlock = new TextBlock()
                                {
                                    TextWrapping = TextWrapping.WrapWithOverflow,
                                    Text = "\"" + card.Title + "\"\n",
                                    Foreground = new SolidColorBrush(Color.FromArgb(160, 135, 214, 192))
                                };

                                if (change.OldExecutor != null)
                                    textBlock.Text += change.OldExecutor.NickName;

                                textBlock.Text += " -> ";

                                if (change.NewExecutor != null)
                                    textBlock.Text += change.NewExecutor.NickName + "\n";

                                else
                                    textBlock.Text += "<Empty>" + "\n";

                                textBlock.Text += change.Reason + "\n" + change.Date;

                                border.Child = textBlock;
                                PreviousCardExecutorsStackPanel.Children.Add(border);
                            }
                        }
                    }
                }
            }
        }

        private void DrawCardPieGraphics(ICollection<Epic> epics)
        {
            int planedCardCount = 0, inProgressCardCount = 0, completeCardCount = 0, testingCardCount = 0;
            int planedCardArchivalCount = 0, inProgressCardArchivalCount = 0, completeCardArchivalCount = 0, testingCardArchivalCount = 0;

            foreach (var epic in epics)
            {
                if (epic != null && epic.Cards != null)
                {
                    var cards = epic.Cards;

                    foreach (var card in cards)
                    {
                        if (card.Archival == false)
                        {
                            switch (card.Status)
                            {
                                case 0:
                                    planedCardCount++;
                                    break;
                                case 1:
                                    inProgressCardCount++;
                                    break;
                                case 2:
                                    completeCardCount++;
                                    break;
                                case 3:
                                    testingCardCount++;
                                    break;
                            }
                        }

                        else
                        {
                            switch (card.Status)
                            {
                                case 0:
                                    planedCardArchivalCount++;
                                    break;
                                case 1:
                                    inProgressCardArchivalCount++;
                                    break;
                                case 2:
                                    completeCardArchivalCount++;
                                    break;
                                case 3:
                                    testingCardArchivalCount++;
                                    break;
                            }
                        }
                    }
                }
            }

            if (planedCardCount != 0 || inProgressCardCount != 0 || completeCardCount != 0 || testingCardCount != 0)
            {
                CardStatisticChart.Series.Add(new PieSeries { Title = "Palanned", Fill = new SolidColorBrush(Color.FromRgb(254, 134, 0)), StrokeThickness = 1, Values = new ChartValues<int> { planedCardCount } });
                CardStatisticChart.Series.Add(new PieSeries { Title = "In Progress", Fill = new SolidColorBrush(Color.FromRgb(7, 114, 160)), StrokeThickness = 1, Values = new ChartValues<int> { inProgressCardCount } });
                CardStatisticChart.Series.Add(new PieSeries { Title = "Testing", Fill = new SolidColorBrush(Color.FromRgb(19, 57, 171)), StrokeThickness = 1, Values = new ChartValues<int> { testingCardCount } });
                CardStatisticChart.Series.Add(new PieSeries { Title = "Completed", Fill = new SolidColorBrush(Color.FromRgb(0, 174, 99)), StrokeThickness = 1, Values = new ChartValues<int> { completeCardCount } });
            }

            if (planedCardArchivalCount != 0 || inProgressCardArchivalCount != 0 || completeCardArchivalCount != 0 || testingCardArchivalCount != 0)
            {
                CardStatisticChart_Archival.Series.Add(new PieSeries { Title = "Palanned(Archival)", Fill = new SolidColorBrush(Color.FromRgb(254, 134, 0)), StrokeThickness = 1, Values = new ChartValues<int> { planedCardArchivalCount } });
                CardStatisticChart_Archival.Series.Add(new PieSeries { Title = "In Progress(Archival)", Fill = new SolidColorBrush(Color.FromRgb(7, 114, 160)), StrokeThickness = 1, Values = new ChartValues<int> { inProgressCardArchivalCount } });
                CardStatisticChart_Archival.Series.Add(new PieSeries { Title = "Testing(Archival)", Fill = new SolidColorBrush(Color.FromRgb(19, 57, 171)), Values = new ChartValues<int> { testingCardArchivalCount } });
                CardStatisticChart_Archival.Series.Add(new PieSeries { Title = "Completed(Archival)", Fill = new SolidColorBrush(Color.FromRgb(0, 174, 99)), StrokeThickness = 1, Values = new ChartValues<int> { completeCardArchivalCount } });
            }            
        }
    }
}
