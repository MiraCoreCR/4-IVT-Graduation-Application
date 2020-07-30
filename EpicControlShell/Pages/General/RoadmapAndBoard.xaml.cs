using EpicControlShell.Resources.Entities;
using EpicControlShell.Resources;
using EpicControlShell.Windows.Creating;
using EpicControlShell.Windows.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EpicControlShell.Pages.General
{
    public partial class RoadmapAndBoard : Page
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        private int ProjectId, EpicId, CardId;
        private bool isEpicInfoOpen = false, isCardInfoOpen = false;    // Так будем различать при применении изменений

        readonly List<ComboBoxItem> comboBoxItems = new List<ComboBoxItem>() // Статусы карточек и эпиков
        {
            new ComboBoxItem() { Content = "Planned", Tag = 0},
            new ComboBoxItem() { Content = "In Progress", Tag = 1},
            new ComboBoxItem() { Content = "Testing", Tag = 2},
            new ComboBoxItem() { Content = "Complete", Tag = 3}
        };

        public RoadmapAndBoard()
        {
            InitializeComponent();
        }

        #region Prepare and Stop Work with Project
        public void SetProjectId(int ProjectId) // Установить рабочий проект 
        {
            this.ProjectId = ProjectId;

            TabCards.IsSelected = false;
            TabLinks.IsSelected = false;
            TabRoadMap.IsSelected = true;

            ShowEpicsInCurrentProject();
            ShowLinksInCurrentProject();

            // Сюда вставить проверку на просроченные эпики, в которых остались задачи
        }

        public int GetProjectId() // Вернуть Id рабочего проекта 
        {
            return ProjectId;
        }

        public void SetPageParametrsAllAtNull() // Установить все параметры в ноль 
        {
            EpicsList_WrapPanel.Children.Clear();

            PlanedCards_StackPanel.Children.Clear();
            InProgressCards_StackPanel.Children.Clear();
            CompletedCards_StackPanel.Children.Clear();
            TestingCards_StackPanel.Children.Clear();

            TabCards.Visibility = Visibility.Collapsed;
            TabCards.IsEnabled = false;
            TabCards.IsSelected = false;
            TabRoadMap.IsEnabled = true;

            TabLinks_WrapPanel.Children.Clear();

            InformationOnTheRight.Width = new GridLength(0, GridUnitType.Star);

            ProjectId = 0; EpicId = 0; CardId = 0;
            isCardInfoOpen = false; isEpicInfoOpen = false;
        }
        #endregion

        #region InformationOnTheRight 
        private void InformationOnTheRightCloseButton_Click(object sender, RoutedEventArgs e) // Закрыть вкладку справа с информацией 
        {
            InformationOnTheRight.Width = new GridLength(0, GridUnitType.Star);
            isEpicInfoOpen = false; isCardInfoOpen = false;
        }

        private void InformationOnTheRightAcceptButton_Click(object sender, RoutedEventArgs e) // Нажатие на кнопку "Применить изменения" 
        {
            if (isEpicInfoOpen)
                AcceptChangesInEpic(Convert.ToInt32(((Button)e.OriginalSource).Tag));

            if (isCardInfoOpen)
                AcceptChangesInCard(Convert.ToInt32(((Button)e.OriginalSource).Tag));
        }

        private void InformationOnTheRightDeleteButton_Click(object sender, RoutedEventArgs e) // Нажатие на кнопку "Удалить" 
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete this?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (isCardInfoOpen)
                    {
                        var card = context.Cards.Where(c => c.Id == CardId).FirstOrDefault();

                        context.Cards.Remove(card);
                        context.SaveChanges();

                        ShowCardsInCurrentEpic();
                        ShowEpicsInCurrentProject();
                    }

                    if (isEpicInfoOpen)
                    {
                        var epic = context.Epics.Where(ep => ep.Id == EpicId).FirstOrDefault();

                        context.Epics.Remove(epic);
                        context.SaveChanges();

                        ShowEpicsInCurrentProject();
                    }

                    InformationOnTheRightCloseButton_Click(null, null);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void InformationOnTheRightArchiveCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to do this card archival? The card will be removed from the public.", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (isCardInfoOpen)
                    {
                        MakeCurrentCardArchival();
                    }

                    InformationOnTheRightCloseButton_Click(null, null);
                    ShowCardsInCurrentEpic();
                    ShowEpicsInCurrentProject();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (isCardInfoOpen)
                MakeCurrentCardArchival();
        }

        private void AssignResponsibleButton_Click(object sender, RoutedEventArgs e) // Добавление ответственных за эпик и исполнителей 
        {
            if (isEpicInfoOpen)
            {
                PurposeWindow purposeWindow = new PurposeWindow("Epic", EpicId, ProjectId, 0);
                purposeWindow.ShowDialog();
                ShowEpicInfo();
            }
            
            else if (isCardInfoOpen)
            {
                PurposeWindow purposeWindow = new PurposeWindow("Card", CardId, ProjectId, Convert.ToInt32(((Button)e.OriginalSource).Tag));
                purposeWindow.ShowDialog();
                ShowCardInfo();
            }

            
        }

        private void ExportCardInAnotherEpicButton_Click(object sender, RoutedEventArgs e) // Перенос карточки в другой эпик
        {
            TransferCards transferCards = new TransferCards(CardId);
            if (transferCards.ShowDialog() == true)
            {
                ShowCardsInCurrentEpic();
                ShowEpicsInCurrentProject();
                InformationOnTheRightCloseButton_Click(null, null);
            }
        }
        #endregion

        #region Epics
        private void ShowEpicsInCurrentProject() // Вывод информации в конкретном эпике 
        {
            EpicsList_WrapPanel.Children.Clear();

            try
            {
                var epics = context.Epics.Where(e => e.ProjectId.Equals(this.ProjectId)).ToList();

                foreach (var epic in epics)
                {
                    Grid grid = new Grid
                    {
                        Margin = new Thickness(5),
                        Height = 80,
                        Width = 200
                    };

                    Button buttonBig = new Button
                    {
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                        FontSize = 20,
                        Content = epic.Title,
                        Tag = epic.Id
                    };
                    buttonBig.Click += new RoutedEventHandler(EpicButton_Click);

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
                        Tag = epic.Id
                    };
                    buttonSmall.Click += new RoutedEventHandler(ThreeDotsOnEpicButton_Click);

                    grid.Children.Add(buttonBig);
                    grid.Children.Add(buttonSmall);

                    if (epic.Cards == null || epic.Cards.Count == 0)
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

                    EpicsList_WrapPanel.Children.Add(grid);
                }

                Button addNewEpicButton = new Button
                {
                    Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    Height = 80,
                    Width = 200,
                    FontSize = 20,
                    Content = "+ Add New Epic"
                };
                addNewEpicButton.Click += new RoutedEventHandler(CreateNewEpicButton_Click);

                EpicsList_WrapPanel.Children.Add(addNewEpicButton);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EpicButton_Click(object sender, RoutedEventArgs e) // Нажатие на конкретный эпик 
        {

            EpicTitle_Label.Content = ((Button)e.OriginalSource).Content;
            TabCards.IsEnabled = true;
            TabCards.Visibility = Visibility.Visible;
            TabRoadMap.IsSelected = false;
            TabCards.IsSelected = true;

            EpicId = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            ShowCardsInCurrentEpic();
        }

        private void ThreeDotsOnEpicButton_Click(object sender, RoutedEventArgs e) // Нажатие на три точки конкретного эпика 
        {
            InformationOnTheRight.Width = new GridLength(25, GridUnitType.Star);
            EpicId = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            ShowEpicInfo();
        }

        private void ShowEpicInfo() // Просмотр информации о эпике с помощью вкладки справа 
        {
            try
            {
                isEpicInfoOpen = true; isCardInfoOpen = false;

                InformationOnTheRight_Delete.Visibility = Visibility.Visible;
                InformationOnTheRight_Delete.IsEnabled = true;
                InformationOnTheRight_ArchiveCard.Visibility = Visibility.Hidden;
                InformationOnTheRight_ArchiveCard.IsEnabled = false;

                InformationOnTheRight_Comments_StackPanel.Children.Clear();

                InformationOnTheRight_StatusAndTransfer_Grid.IsEnabled = false;
                InformationOnTheRight_StatusAndTransfer_Grid.Visibility = Visibility.Collapsed;

                InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Clear();

                InformationOnTheRight_StartDate_Grid.IsEnabled = true;
                InformationOnTheRight_StartDate_Grid.Visibility = Visibility.Visible;

                InformationOnTheRight_Status_ComboBox.ItemsSource = null;

                var project = context.Projects.Where(p => p.Id.Equals(ProjectId)).First();
                var epic = context.Epics.Where(e => e.Id.Equals(EpicId)).First();
                var epicComments = context.EpicComments.ToList().Where(e => e.EpicId.Equals(EpicId));

                InformationOnTheRight_SecondaryTitle.Text = epic.Project.Title;
                InformationOnTheRight_MainTitle.Text = epic.Title;
                InformationOnTheRight_Description.Text = epic.Description;

                InformationOnTheRight_ExecutorsOrEpicMaster.Content = "Epic Master";

                if (epic.MasterId != null)
                {
                    Button button = new Button()
                    {
                        Content = Application.Current.FindResource("UserIcon"),
                        Height = 30,
                        Margin = new Thickness(5, 0, 5, 0),
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        Tag = epic.Master.Id
                    };
                    button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);

                    ToolTip toolTip = new ToolTip()
                    {
                        Content = epic.Master.NickName + " - " + epic.Master.FullName + "\n" + epic.Master.Group.Title +"\n" + epic.Master.Email
                    };
                    button.ToolTip = toolTip;

                    InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Add(button);
                }
                else
                {
                    Button button = new Button()
                    {
                        Content = Application.Current.FindResource("CreateNewIcon"),
                        Height = 30,
                        Margin = new Thickness(5, 0, 5, 0),
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
                    };
                    button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);
                    InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Add(button);
                }


                InformationOnTheRight_StartDate.SelectedDate = epic.StartDay;
                InformationOnTheRight_DeadLine.SelectedDate = epic.DeadLine;
                InformationOnTheRight_AcceptChanges.Tag = epic.Id;

                Label label = new Label()
                {
                    Content = "Comments",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                InformationOnTheRight_Comments_StackPanel.Children.Add(label);

                foreach (var comment in epicComments)
                {
                    if (comment.UserId == InformationAboutCurrnetUser.UserId)
                    {
                        Grid grid = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)), 
                            MinHeight = 60, 
                            Margin =  new Thickness(5)
                        };

                        TextBlock textBlock = new TextBlock()
                        {
                            Text = comment.User.NickName,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 30, 0),
                            VerticalAlignment = VerticalAlignment.Top,
                            Padding = new Thickness(3)
                        };
                        grid.Children.Add(textBlock);

                        TextBox textBox = new TextBox()
                        {
                            Text = comment.Text,
                            HorizontalAlignment = HorizontalAlignment.Stretch, 
                            Margin = new Thickness(0, 20, 30, 0),
                            TextWrapping = TextWrapping.WrapWithOverflow,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                        };
                        grid.Children.Add(textBox);

                        Button acceptButton = new Button()
                        {
                            Height = 30,
                            Width = 30,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = Application.Current.FindResource("AcceptIcon"),
                            Padding = new Thickness(5),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Tag = comment.Id
                        };
                        acceptButton.Click += new RoutedEventHandler(AcceptEpicCommentChanges);
                        grid.Children.Add(acceptButton);

                        Button deleteButton = new Button()
                        {
                            Height = 30,
                            Width = 30,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Content = Application.Current.FindResource("DeleteIcon"),
                            Padding = new Thickness(2),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Tag = comment.Id
                        };
                        deleteButton.Click += new RoutedEventHandler(DeleteEpicComment);
                        grid.Children.Add(deleteButton);

                        InformationOnTheRight_Comments_StackPanel.Children.Add(grid);
                    }
                    
                    else
                    {
                        Grid grid = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            Margin = new Thickness(5)
                        };

                        TextBlock textBlock = new TextBlock()
                        {
                            Text = comment.User.NickName,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            VerticalAlignment = VerticalAlignment.Top,
                            Padding = new Thickness(3)
                        };
                        grid.Children.Add(textBlock);

                        TextBlock commentTextBlock = new TextBlock()
                        {
                            Text = comment.Text,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Margin = new Thickness(0, 20, 0, 0),
                            TextWrapping = TextWrapping.WrapWithOverflow,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            FontSize = 12,
                            Padding = new Thickness(3),
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192))
                        };
                        grid.Children.Add(commentTextBlock);

                        InformationOnTheRight_Comments_StackPanel.Children.Add(grid);
                    }
                }

                Grid addCommentGrid = new Grid()
                {
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    MinHeight = 60,
                    Margin = new Thickness(5)
                };

                TextBlock addCommentTextBlock = new TextBlock()
                {
                    Text = "+ Add New Comment",
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 30, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(3)
                };
                addCommentGrid.Children.Add(addCommentTextBlock);

                TextBox addCommentTextBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 20, 30, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192))
                };
                addCommentGrid.Children.Add(addCommentTextBox);

                Button addCommentAcceptButton = new Button()
                {
                    Height = 30,
                    Width = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Content = Application.Current.FindResource("AcceptIcon"),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                };
                addCommentAcceptButton.Click += new RoutedEventHandler(CreateNewEpicComment);
                addCommentGrid.Children.Add(addCommentAcceptButton);

                Button addCommentResetButton = new Button()
                {
                    Height = 30,
                    Width = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Content = Application.Current.FindResource("CloseIcon"),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                };
                addCommentResetButton.Click += new RoutedEventHandler(ResetNewEpicComment);
                addCommentGrid.Children.Add(addCommentResetButton);

                InformationOnTheRight_Comments_StackPanel.Children.Add(addCommentGrid);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
        } 

        private void AcceptEpicCommentChanges(object sender, RoutedEventArgs e) // Обновить комментарий
        {
            int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            var epicComment = context.EpicComments.Where(ec => ec.Id == tag).First();

            var grid = (Grid)((Button)sender).Parent;

            if (grid.Children.OfType<TextBox>().First().Text.Length <= 1000 && grid.Children.OfType<TextBox>().First().Text != null)
            {
                epicComment.Text = grid.Children.OfType<TextBox>().First().Text;

                context.SaveChanges();

                MessageBox.Show("Comment succesfully changed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                ShowEpicInfo();
            }

            else
                MessageBox.Show("The maximum length of a comment is 1000 characters and must not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CreateNewEpicComment(object sender, RoutedEventArgs e) // Создать новый комментарий
        {
            var grid = (Grid)((Button)sender).Parent;

            if (grid.Children.OfType<TextBox>().First().Text.Length <= 1000 && grid.Children.OfType<TextBox>().First().Text != null)
            {
                EpicComment epicComment = new EpicComment()
                {
                    UserId = (int)InformationAboutCurrnetUser.UserId,
                    Text = grid.Children.OfType<TextBox>().First().Text,
                    EpicId = this.EpicId
                };

                context.EpicComments.Add(epicComment);
                context.SaveChanges();

                ShowEpicInfo();
            }
            
            else
                MessageBox.Show("The maximum length of a comment is 1000 characters and must not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ResetNewEpicComment(object sender, RoutedEventArgs e) // Сбросить введённый текст 
        {
            var grid = (Grid)((Button)sender).Parent;
            grid.Children.OfType<TextBox>().First().Text = null;
        }

        private void DeleteEpicComment(object sender, RoutedEventArgs e) // Обновить комментарий
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete this?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
                var epicComment = context.EpicComments.Where(ec => ec.Id == tag).First();

                context.EpicComments.Remove(epicComment);
                context.SaveChanges();

                ShowEpicInfo();
            }
        }

        private void CreateNewEpicButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEpicWindow createCardWindow = new CreateEpicWindow(ProjectId);
            if (createCardWindow.ShowDialog() == true)
            {
                ShowEpicsInCurrentProject();
            }
        }

        private void AcceptChangesInEpic(int EpicId) // Применить изменения в эпике 
        {
            try
            {
                var epic = context.Epics.Where(e => e.Id.Equals(EpicId)).First();

                if (epic.Title != InformationOnTheRight_MainTitle.Text)
                    epic.Title = InformationOnTheRight_MainTitle.Text;

                if (epic.Description != InformationOnTheRight_Description.Text)
                    epic.Description = InformationOnTheRight_Description.Text;

                try
                {
                    DateTime dateTime = Convert.ToDateTime(InformationOnTheRight_StartDate.Text);

                    if (epic.StartDay != dateTime)
                        epic.StartDay = dateTime;
                }
                catch (Exception)
                {
                    MessageBox.Show("Incorrect start date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }



                try
                {
                    DateTime dateTime = Convert.ToDateTime(InformationOnTheRight_DeadLine.Text);

                    if (epic.DeadLine != dateTime)
                        epic.DeadLine = dateTime;
                }
                catch (Exception)
                {
                    MessageBox.Show("Incorrect due date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                context.SaveChanges();

                ShowEpicsInCurrentProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Cards
        public void ShowCardsInCurrentEpic() // Показать карточки в конкретном эпике 
        {
            PlanedCards_StackPanel.Children.Clear();
            InProgressCards_StackPanel.Children.Clear();
            CompletedCards_StackPanel.Children.Clear();
            TestingCards_StackPanel.Children.Clear();

            try
            {
                var cards = context.Cards.Where(c => c.EpicId.Equals(EpicId)).ToList();

                foreach (var card in cards)
                {
                    if (!card.Archival)
                    {
                        Button button = new Button()
                        {
                            MinHeight = 50,
                            Margin = new Thickness(5),
                            Content = card.Title,
                            Tag = card.Id
                        };
                        button.Click += new RoutedEventHandler(CardButton_Click);

                        switch (card.Status)
                        {
                            case 0:
                                PlanedCards_StackPanel.Children.Add(button);
                                break;
                            case 1:
                                InProgressCards_StackPanel.Children.Add(button);
                                break;
                            case 2:
                                TestingCards_StackPanel.Children.Add(button);
                                break;
                            case 3:
                                CompletedCards_StackPanel.Children.Add(button);
                                break;
                            default:
                                MessageBox.Show("Unknown status. Check card with id " + card.Id);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardButton_Click(object sender, RoutedEventArgs e) // Нажатие на карточку 
        {
            InformationOnTheRight.Width = new GridLength(25, GridUnitType.Star);
            CardId = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            ShowCardInfo();
        }

        private void ShowCardInfo() // Вывести информацию о конкретной карточке во вкладке справа 
        {
            try
            {
                isEpicInfoOpen = false; isCardInfoOpen = true;

                InformationOnTheRight_Delete.IsEnabled = false;
                InformationOnTheRight_Delete.Visibility = Visibility.Hidden;
                InformationOnTheRight_ArchiveCard.IsEnabled = true;
                InformationOnTheRight_ArchiveCard.Visibility = Visibility.Visible;

                InformationOnTheRight_Comments_StackPanel.Children.Clear();
                InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Clear();

                InformationOnTheRight_Status_ComboBox.ItemsSource = null;
                InformationOnTheRight_Status_ComboBox.Items.Clear();

                InformationOnTheRight_StatusAndTransfer_Grid.IsEnabled = true;
                InformationOnTheRight_StatusAndTransfer_Grid.Visibility = Visibility.Visible;
                
                InformationOnTheRight_Status_ComboBox.ItemsSource = comboBoxItems;

                InformationOnTheRight_StartDate_Grid.IsEnabled = false;
                InformationOnTheRight_StartDate_Grid.Visibility = Visibility.Collapsed;

                
                var card = context.Cards.Where(c => c.Id.Equals(CardId)).First();
                var cardComments = context.CardComments.ToList().Where(com => com.CardId.Equals(CardId));

                InformationOnTheRight_SecondaryTitle.Text = card.Epic.Project.Title + " / " + card.Epic.Title;
                InformationOnTheRight_MainTitle.Text = card.Title;
                InformationOnTheRight_Status_ComboBox.SelectedIndex = card.Status;
                InformationOnTheRight_Description.Text = card.Description;
                InformationOnTheRight_ExecutorsOrEpicMaster.Content = "Executors";

                var executors = context.Executors.Where(e => e.CardId == CardId).ToList();
                if (executors.Count != 0)
                {
                    foreach (var executor in executors)
                    {
                        Button button = new Button()
                        {
                            Content = Application.Current.FindResource("UserIcon"),
                            Height = 30,
                            Margin = new Thickness(5, 0, 5, 0),
                            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                            Tag = executor.UserId
                        };
                        button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);

                        ToolTip toolTip = new ToolTip()
                        {
                            Content = executor.User.NickName + " - " + executor.User.FullName + "\n" + executor.User.Group.Title + "\n" + executor.User.Email
                        };
                        button.ToolTip = toolTip;

                        InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Add(button);
                    }

                    if (executors.Count < 5)
                    {
                        Button button = new Button()
                        {
                            Content = Application.Current.FindResource("CreateNewIcon"),
                            Height = 30,
                            Margin = new Thickness(5, 0, 5, 0),
                            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                            Tag = 0
                        };
                        button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);
                        InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Add(button);
                    }

                }
                else
                {
                    Button button = new Button()
                    {
                        Content = Application.Current.FindResource("CreateNewIcon"),
                        Height = 30,
                        Margin = new Thickness(5, 0, 5, 0),
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        Tag = 0
                    };
                    button.Click += new RoutedEventHandler(AssignResponsibleButton_Click);
                    InformationOnTheRight_ExecutorsOrEpicMaster_WrapPanel.Children.Add(button);
                }


                InformationOnTheRight_StartDate.SelectedDate = null;
                InformationOnTheRight_AcceptChanges.Tag = card.Id;

                if (card.DeadLine == null)
                    InformationOnTheRight_DeadLine.SelectedDate = null;

                else
                    InformationOnTheRight_DeadLine.SelectedDate = card.DeadLine;

                Label label = new Label()
                {
                    Content = "Comments",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                InformationOnTheRight_Comments_StackPanel.Children.Add(label);

                foreach (var comment in cardComments)
                {
                    if (comment.UserId == InformationAboutCurrnetUser.UserId)
                    {
                        Grid grid = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            Margin = new Thickness(5)
                        };

                        TextBlock textBlock = new TextBlock()
                        {
                            Text = comment.User.NickName,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 30, 0),
                            VerticalAlignment = VerticalAlignment.Top,
                            Padding = new Thickness(3)
                        };
                        grid.Children.Add(textBlock);

                        TextBox textBox = new TextBox()
                        {
                            Text = comment.Text,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Margin = new Thickness(0, 20, 30, 0),
                            TextWrapping = TextWrapping.WrapWithOverflow,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                        };
                        grid.Children.Add(textBox);

                        Button acceptButton = new Button()
                        {
                            Height = 30,
                            Width = 30,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = Application.Current.FindResource("AcceptIcon"),
                            Padding = new Thickness(5),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Tag = comment.Id
                        };
                        acceptButton.Click += new RoutedEventHandler(AcceptCardCommentChanges);
                        grid.Children.Add(acceptButton);

                        Button deleteButton = new Button()
                        {
                            Height = 30,
                            Width = 30,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Content = Application.Current.FindResource("DeleteIcon"),
                            Padding = new Thickness(2),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Tag = comment.Id
                        };
                        deleteButton.Click += new RoutedEventHandler(DeleteCardComment);
                        grid.Children.Add(deleteButton);

                        InformationOnTheRight_Comments_StackPanel.Children.Add(grid);
                    }

                    else
                    {
                        Grid grid = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            Margin = new Thickness(5)
                        };

                        TextBlock textBlock = new TextBlock()
                        {
                            Text = comment.User.NickName,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            VerticalAlignment = VerticalAlignment.Top,
                            Padding = new Thickness(3)
                        };
                        grid.Children.Add(textBlock);

                        TextBlock commentTextBlock = new TextBlock()
                        {
                            Text = comment.Text,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Margin = new Thickness(0, 20, 0, 0),
                            TextWrapping = TextWrapping.WrapWithOverflow,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            FontSize = 12,
                            Padding = new Thickness(3),
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192))
                        };
                        grid.Children.Add(commentTextBlock);

                        InformationOnTheRight_Comments_StackPanel.Children.Add(grid);
                    }
                }

                Grid addCommentGrid = new Grid()
                {
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    MinHeight = 60,
                    Margin = new Thickness(5)
                };

                TextBlock addCommentTextBlock = new TextBlock()
                {
                    Text = "+ Add New Comment",
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 30, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(3)
                };
                addCommentGrid.Children.Add(addCommentTextBlock);

                TextBox addCommentTextBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 20, 30, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    Tag = "AddNewEpicCommentTextBox"
                };
                addCommentGrid.Children.Add(addCommentTextBox);

                Button addCommentAcceptButton = new Button()
                {
                    Height = 30,
                    Width = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Content = Application.Current.FindResource("AcceptIcon"),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                };
                addCommentAcceptButton.Click += new RoutedEventHandler(CreateNewCardComment);
                addCommentGrid.Children.Add(addCommentAcceptButton);

                Button addCommentResetButton = new Button()
                {
                    Height = 30,
                    Width = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Content = Application.Current.FindResource("CloseIcon"),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                };
                addCommentResetButton.Click += new RoutedEventHandler(ResetNewCardComment);
                addCommentGrid.Children.Add(addCommentResetButton);

                InformationOnTheRight_Comments_StackPanel.Children.Add(addCommentGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptCardCommentChanges(object sender, RoutedEventArgs e) // Обновить комментарий
        {
            int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            var cardComment = context.CardComments.Where(ec => ec.Id == tag).First();

            var grid = (Grid)((Button)sender).Parent;

            if (grid.Children.OfType<TextBox>().First().Text.Length <= 1000 && grid.Children.OfType<TextBox>().First().Text != null)
            {
                cardComment.Text = grid.Children.OfType<TextBox>().First().Text;

                context.SaveChanges();

                MessageBox.Show("Comment succesfully changed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                ShowCardInfo();
            }

            else
                MessageBox.Show("The maximum length of a comment is 1000 characters and must not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CreateNewCardComment(object sender, RoutedEventArgs e) // Создать новый комментарий
        {
            var grid = (Grid)((Button)sender).Parent;

            if (grid.Children.OfType<TextBox>().First().Text.Length <= 1000 && grid.Children.OfType<TextBox>().First().Text != null)
            {
                CardComment cardComment = new CardComment()
                {
                    UserId = (int)InformationAboutCurrnetUser.UserId,
                    Text = grid.Children.OfType<TextBox>().First().Text,
                    CardId = this.CardId,
                    TimeToDelete = DateTime.Today.AddDays(7)
                };

                context.CardComments.Add(cardComment);
                context.SaveChanges();

                ShowCardInfo();
            }

            else
                MessageBox.Show("The maximum length of a comment is 1000 characters and must not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ResetNewCardComment(object sender, RoutedEventArgs e) // Сбросить введённый текст 
        {
            var grid = (Grid)((Button)sender).Parent;
            grid.Children.OfType<TextBox>().First().Text = null;
        }

        private void DeleteCardComment(object sender, RoutedEventArgs e) // Обновить комментарий
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete this?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
                var cardComment = context.CardComments.Where(cc => cc.Id == tag).First();

                context.CardComments.Remove(cardComment);
                context.SaveChanges();

                ShowCardInfo();
            }
        }

        private void CreateNewCardButton_Click(object sender, RoutedEventArgs e) // Клик по кнопке "Добавить новую карточку" 
        {
            CreateCardWindow createCardWindow = new CreateCardWindow(EpicId, Convert.ToInt32(((Button)e.OriginalSource).Tag));
            if (createCardWindow.ShowDialog() == true)
            {
                ShowCardsInCurrentEpic();
            }
        }

        private void AcceptChangesInCard(int CardId) // Применить изменения в карточке 
        {
            try
            {
                var card = context.Cards.Where(e => e.Id.Equals(CardId)).First();

                if (card.Title != InformationOnTheRight_MainTitle.Text)
                    card.Title = InformationOnTheRight_MainTitle.Text;

                if (card.Description != InformationOnTheRight_Description.Text)
                    card.Description = InformationOnTheRight_Description.Text;

                if (card.Status != InformationOnTheRight_Status_ComboBox.SelectedIndex)
                    card.Status = InformationOnTheRight_Status_ComboBox.SelectedIndex;

                try
                {
                    if (InformationOnTheRight_DeadLine.Text != "")
                    {
                        DateTime dateTime = Convert.ToDateTime(InformationOnTheRight_DeadLine.Text);

                        if (card.DeadLine != dateTime)
                            card.DeadLine = dateTime;
                    }

                    else
                        card.DeadLine = null;
                }
                catch (Exception)
                {
                    MessageBox.Show("Incorrect due date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                context.SaveChanges();

                ShowCardsInCurrentEpic();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MakeCurrentCardArchival()  // Сделать текущую карточку архивной
        {
            var card = context.Cards.Where(c => c.Id == CardId).FirstOrDefault();

            card.Archival = true;

            context.SaveChanges();
        }
        #endregion

        #region Links
        private void ShowLinksInCurrentProject() // Вывод ссылок в выбранном проекте  
        {
            try
            {
                TabLinks_WrapPanel.Children.Clear();

                var links = context.Links.ToList().Where(l => l.ProjectId.Equals(this.ProjectId));
                foreach (var link in links)
                {
                    if (link.UserId == InformationAboutCurrnetUser.UserId)
                    {
                        Grid grid = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            MaxWidth = 400,
                            Margin = new Thickness(5)
                        };

                        StackPanel stackPanel = new StackPanel()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            MinWidth = 300,
                            Margin = new Thickness(5)
                        };

                        TextBlock userNameTextBlock = new TextBlock()
                        {
                            Text = link.User.NickName,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)), 
                            FontSize = 15,
                            Margin = new Thickness(0, 0, 30, 5),
                            Height = 20,
                            Padding = new Thickness(3),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58))
                        };
                        stackPanel.Children.Add(userNameTextBlock);

                        TextBox linkTextBox = new TextBox()
                        {
                            Text = link.LinkToResources,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 30, 10),
                            TextWrapping = TextWrapping.WrapWithOverflow
                        };
                        stackPanel.Children.Add(linkTextBox);

                        TextBox commentTextBox = new TextBox()
                        {
                            Text = link.Comment,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            Height = 30,
                            Margin = new Thickness(0, 0, 30, 0),
                            TextWrapping = TextWrapping.WrapWithOverflow
                        };
                        stackPanel.Children.Add(commentTextBox);
                        grid.Children.Add(stackPanel);

                        Button acceptButton = new Button()
                        {
                            Height = 30,
                            Width = 30,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = Application.Current.FindResource("AcceptIcon"),
                            Padding = new Thickness(5),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Tag = link.Id
                        };
                        acceptButton.Click += new RoutedEventHandler(AcceptLinkChanges);
                        grid.Children.Add(acceptButton);

                        Button deleteButton = new Button()
                        {
                            Height = 30,
                            Width = 30,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Content = Application.Current.FindResource("DeleteIcon"),
                            Padding = new Thickness(5),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Tag = link.Id
                        };
                        deleteButton.Click += new RoutedEventHandler(DeleteLink);
                        grid.Children.Add(deleteButton);

                        TabLinks_WrapPanel.Children.Add(grid);
                    }

                    else
                    {
                        Grid grid = new Grid()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            MaxWidth = 400,
                            Margin = new Thickness(5)
                        };

                        StackPanel stackPanel = new StackPanel()
                        {
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            MinHeight = 60,
                            MinWidth = 300,
                            Margin = new Thickness(5)
                        };

                        TextBlock userNameTextBlock = new TextBlock()
                        {
                            Text = link.User.NickName,
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 15,
                            Margin = new Thickness(0, 0, 0, 5),
                            Height = 20,
                            Padding = new Thickness(3),
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58))
                        };
                        stackPanel.Children.Add(userNameTextBlock);

                        TextBlock linkTextBlock = new TextBlock()
                        {
                            Text = link.LinkToResources,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 0, 5),
                            TextWrapping = TextWrapping.WrapWithOverflow
                        };
                        stackPanel.Children.Add(linkTextBlock);

                        TextBlock commentTextBlock = new TextBlock()
                        {
                            Text = link.Comment,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                            Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                            FontSize = 12,
                            TextWrapping = TextWrapping.WrapWithOverflow
                        };
                        stackPanel.Children.Add(commentTextBlock);
                        grid.Children.Add(stackPanel);

                        TabLinks_WrapPanel.Children.Add(grid);
                    }
                }

                Grid newLinkGrid = new Grid()
                {
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    MinHeight = 60,
                    MaxWidth = 400,
                    Margin = new Thickness(5)
                };

                StackPanel newLinkStackPanel = new StackPanel()
                {
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    MinHeight = 60,
                    MinWidth = 300,
                    Margin = new Thickness(5)
                };

                TextBlock newLinkTextBlock = new TextBlock()
                {
                    Text = "+ Add new Link",
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    FontSize = 15,
                    Margin = new Thickness(0, 0, 30, 5),
                    Height = 20,
                    Padding = new Thickness(3),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58))
                };
                newLinkStackPanel.Children.Add(newLinkTextBlock);

                Label newLinkLabelLink = new Label()
                {
                    Content = "Link",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromArgb(100, 135, 214, 192))
                };
                newLinkStackPanel.Children.Add(newLinkLabelLink);

                TextBox newLinkLinkTextBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 30, 5),
                    TextWrapping = TextWrapping.WrapWithOverflow
                };
                newLinkStackPanel.Children.Add(newLinkLinkTextBox);

                Label newLinkCommentLabel = new Label()
                {
                    Content = "Comment",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromArgb(100, 135, 214, 192))
                };
                newLinkStackPanel.Children.Add(newLinkCommentLabel);

                TextBox newLinkCommentTextBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58)),
                    Foreground = new SolidColorBrush(Color.FromRgb(135, 214, 192)),
                    FontSize = 12,
                    Height = 30,
                    Margin = new Thickness(0, 0, 30, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow
                };
                newLinkStackPanel.Children.Add(newLinkCommentTextBox);
                newLinkGrid.Children.Add(newLinkStackPanel);

                Button newLinkAcceptButton = new Button()
                {
                    Height = 30,
                    Width = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Content = Application.Current.FindResource("AcceptIcon"),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58))
                };
                newLinkAcceptButton.Click += new RoutedEventHandler(CreateNewLink);
                newLinkGrid.Children.Add(newLinkAcceptButton);

                Button newLinkResetButton = new Button()
                {
                    Height = 30,
                    Width = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Content = Application.Current.FindResource("CloseIcon"),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(54, 54, 58))
                };
                newLinkResetButton.Click += new RoutedEventHandler(ResetLink);
                newLinkGrid.Children.Add(newLinkResetButton);

                TabLinks_WrapPanel.Children.Add(newLinkGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptLinkChanges(object sender, RoutedEventArgs e)
        {
            int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
            var link = context.Links.Where(l => l.Id == tag).First();

            var grid = (Grid)((Button)sender).Parent;
            var stackpanel = grid.Children.OfType<StackPanel>().First();
            var textBoxes = stackpanel.Children.OfType<TextBox>().ToList();

            if (textBoxes.First().Text != null && textBoxes.Last().Text.Length <= 200 && textBoxes.Last() != null)
            {
                link.LinkToResources = textBoxes.First().Text;
                link.Comment = textBoxes.Last().Text;

                context.SaveChanges();

                MessageBox.Show("Link successfully changed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                ShowLinksInCurrentProject();
            }

            else
                MessageBox.Show("The maximum length of a comment is 200 characters. Link and Comment must not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteLink(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you really want to delete this?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);
                var link = context.Links.Where(l => l.Id == tag).First();

                context.Links.Remove(link);
                context.SaveChanges();

                ShowLinksInCurrentProject();
            }
        }

        private void CreateNewLink(object sender, RoutedEventArgs e)
        {
            int tag = Convert.ToInt32(((Button)e.OriginalSource).Tag);

            var grid = (Grid)((Button)sender).Parent;
            var stackpanel = grid.Children.OfType<StackPanel>().First();
            var textBoxes = stackpanel.Children.OfType<TextBox>().ToList();

            if (textBoxes.First().Text != null && textBoxes.Last().Text.Length <= 200 && textBoxes.Last() != null)
            {
                Link link = new Link()
                {
                    ProjectId = this.ProjectId,
                    UserId = (int)InformationAboutCurrnetUser.UserId,
                    LinkToResources = textBoxes.First().Text,
                    Comment = textBoxes.Last().Text
                };
                context.Links.Add(link);
                context.SaveChanges();

                MessageBox.Show("Link successfully added", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                ShowLinksInCurrentProject();
            }

            else
                MessageBox.Show("The maximum length of a comment is 200 characters. Link and Comment must not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ResetLink(object sender, RoutedEventArgs e)
        {
            var grid = (Grid)((Button)sender).Parent;
            var stackpanel = grid.Children.OfType<StackPanel>().First();
            var textBoxes = stackpanel.Children.OfType<TextBox>().ToList();

            textBoxes.First().Text = null;
            textBoxes.Last().Text = null;
        }
        #endregion
    }
}