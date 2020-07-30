using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System.Linq;
using System.Windows;

namespace EpicControlShell.Windows.Creating
{
    
    public partial class CreateGroupWindow : Window
    {
        readonly EpicDbContext context = DatabaseProvider.GetInstance();

        public CreateGroupWindow()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (TitleTextBox.Text != "")
            {
                if (TitleTextBox.Text.Length <= 100)
                {
                    if (context.PermissionGroups.Any(g => g.Title == TitleTextBox.Text) == false)
                    {
                        PermissionGroup group = new PermissionGroup()
                        {
                            Title = TitleTextBox.Text
                        };

                        context.PermissionGroups.Add(group);
                        context.SaveChanges();

                        MessageBox.Show("Group successfull created", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.DialogResult = true;
                    }

                    else
                        MessageBox.Show("Group with this title already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else
                    MessageBox.Show("Maximum length of the name is 100 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else
                MessageBox.Show("Group must have a Title", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
