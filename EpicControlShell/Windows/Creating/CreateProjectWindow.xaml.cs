using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System.Linq;
using System.Windows;

namespace EpicControlShell.Windows.Creating
{
    /// <summary>
    /// Логика взаимодействия для CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window
    {
        private EpicDbContext context = DatabaseProvider.GetInstance();

        public CreateProjectWindow()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (TitleTextBox.Text == "" || DescriptionTextBox.Text == "")
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            else if (TitleTextBox.Text.Length > 100)
                TitleErrorMessage.Visibility = Visibility.Visible;

            else if (DescriptionTextBox.Text.Length > 100)
                DescriptionErrorMessage.Visibility = Visibility.Visible;

            else
            {
                try
                {
                    var temp = context.Projects.ToList().Where(t => t.Title == TitleTextBox.Text);

                    if (temp.Count() == 0)
                    {
                        var project = new Project
                        {
                            Title = TitleTextBox.Text,
                            Description = DescriptionTextBox.Text
                        };

                        context.Projects.Add(project);
                        context.SaveChanges();

                        MessageBox.Show("Project is successfully created", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    else
                        MessageBox.Show("Project with this title already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }

            this.DialogResult = true;
        }
    }
}
