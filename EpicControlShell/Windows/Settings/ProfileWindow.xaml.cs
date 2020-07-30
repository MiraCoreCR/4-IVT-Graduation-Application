using EpicControlShell.Resources.Entities;
using EpicControlShell.Resources;
using System.Linq;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using System;

namespace EpicControlShell.Windows.Settings
{
    public partial class ProfileWindow : Window
    {
        private int workId;

        private EpicDbContext context = DatabaseProvider.GetInstance();

        public ProfileWindow(int workId)
        {
            InitializeComponent();

            this.workId = workId;

            using (var context = new EpicDbContext())
            {
                var user = context.Users.Where(u => u.Id == this.workId).First();

                FullNickNameLabel.Content = user.FullName + $"({user.NickName})";
                EmailLabel.Content = user.Email;
                GroupLabel.Content = user.Group.Title;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (OldPasswordTextBox.Text == "" || NewPasswordTextBox.Password == "" || RepeatPasswordTextBox.Password == "")
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            else if (NewPasswordTextBox.Password.Length < 6)
                MessageBox.Show("Password must not be shorter than 6 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            else if (NewPasswordTextBox.Password != RepeatPasswordTextBox.Password)
                MessageBox.Show("Passwords don't match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            else
            {
                string oldP = OldPasswordTextBox.Text;
                byte[] data = Encoding.Default.GetBytes(oldP);
                var temp = new SHA256Managed().ComputeHash(data);
                string oldPassword = BitConverter.ToString(temp).Replace("-", "").ToLower();

                string newP = NewPasswordTextBox.Password;
                byte[] data1 = Encoding.Default.GetBytes(newP);
                temp = new SHA256Managed().ComputeHash(data1);
                string newPassword = BitConverter.ToString(temp).Replace("-", "").ToLower();

                var user = context.Users.Where(u => u.Id == workId).First();

                if (oldPassword != Convert.ToString(user.Password))
                    MessageBox.Show("Incorrect Password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                else
                {
                    user.Password = newPassword;
                    context.SaveChanges();

                    OldPasswordTextBox.Text = "";
                    NewPasswordTextBox.Password = "";
                    RepeatPasswordTextBox.Password = "";

                    MessageBox.Show("Password successfull changed", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
