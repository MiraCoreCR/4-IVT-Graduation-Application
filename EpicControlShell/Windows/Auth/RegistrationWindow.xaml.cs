using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace EpicControlShell.Windows.Auth
{
    public partial class RegistrationWindow : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();

        private bool isGoogleAuth = false;

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        public RegistrationWindow(string NickName, string FullName, string Email)
        {
            InitializeComponent();

            NickNameTextBox.Text = NickName;
            NickNameTextBox.IsEnabled = false;

            FullNameTextBox.Text = FullName;
            FullNameTextBox.IsEnabled = false;

            EmailTextBox.Text = Email;
            EmailTextBox.IsEnabled = false;

            isGoogleAuth = true;
        }

        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckTextBoxes() == true)
            {
                try
                {
                    var userWithEnteredNickName = context.Users.Where(t => t.NickName == NickNameTextBox.Text).FirstOrDefault();
                    var groupWithNameUser = context.PermissionGroups.Where(g => g.Title == "User").First();

                    if (userWithEnteredNickName == null)
                    {

                        string inputPassword = PasswordTextBox.Password;
                        byte[] data = Encoding.Default.GetBytes(inputPassword);
                        var result = new SHA256Managed().ComputeHash(data);

                        var user = new User
                        {
                            FullName = FullNameTextBox.Text,
                            NickName = NickNameTextBox.Text,
                            Email = EmailTextBox.Text,
                            Password = BitConverter.ToString(result).Replace("-", "").ToLower(),
                            GroupId = groupWithNameUser.Id
                        };

                        context.Users.Add(user);
                        context.SaveChanges();

                        MessageBox.Show("User is successfully registered", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (isGoogleAuth)
                            this.DialogResult = true;

                        else
                            this.DialogResult = false;
                    }

                    else
                        MessageBox.Show("User with this nickname already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

         private bool CheckTextBoxes()
         {
            if (FullNameTextBox.Text == "" || NickNameTextBox.Text == "" || EmailTextBox.Text == "" || PasswordTextBox.Password == "" || RepeatPasswordTextBox.Password == "")
            {
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            else
            {
                if (FullNameTextBox.Text.Length > 300)
                {
                    FullNameErrorText.Visibility = Visibility.Visible;
                    return false;
                }

                else if (NickNameTextBox.Text.Length > 50)
                {
                    NickNameErrorText.Visibility = Visibility.Visible;
                    return false;
                }

                else if (EmailTextBox.Text.Length > 100)
                {
                    EmailErrorText.Visibility = Visibility.Visible;
                    return false;
                }

                else if (PasswordTextBox.Password.Length < 6)
                {
                    PasswordErrorText.Visibility = Visibility.Visible;
                    return false;
                }

                else if (RepeatPasswordTextBox.Password != PasswordTextBox.Password)
                {
                    RepeatPasswordErrorText.Visibility = Visibility.Visible;
                    return false;
                }

                else
                    return true;
            }
         }
    }
}
