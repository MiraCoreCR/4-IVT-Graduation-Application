using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace EpicControlShell.Windows.Auth
{
    /// <summary>
    /// Логика взаимодействия для ForgetPasswordWindow.xaml
    /// </summary>
    public partial class ForgetPasswordWindow : Window
    {
        EpicDbContext context = DatabaseProvider.GetInstance();
        public ForgetPasswordWindow()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            bool isFieldsCorrect = CheckFields();

            if (isFieldsCorrect)
            {
                try
                {
                    var user = context.Users.Where(u => u.NickName == LoginTextBox.Text).First();

                    if (user != null)
                    {
                        if (user.Email == EmailTextBox.Text)
                        {
                            string inputPassword = PasswordTextBox.Password;
                            byte[] data = Encoding.Default.GetBytes(inputPassword);
                            var result = new SHA256Managed().ComputeHash(data);

                            user.Password = BitConverter.ToString(result).Replace("-", "").ToLower();

                            context.SaveChanges();

                            MessageBox.Show("Password successfull changed", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                        }

                        else
                        {
                            MessageBox.Show("Incorrect Email", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    else
                    {
                        MessageBox.Show("There is no such user in the database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } 
        }

        private bool CheckFields() 
        {
            if (LoginTextBox.Text != "" || EmailTextBox.Text != "" || PasswordTextBox.Password != "" || RepeatPasswordTextBox.Password != "")
            {
                if (PasswordTextBox.Password.Length >= 6 || RepeatPasswordTextBox.Password.Length >= 6)
                {
                    if (PasswordTextBox.Password == RepeatPasswordTextBox.Password)
                    {
                        return true;
                    }

                    else
                    {
                        MessageBox.Show("Passwords don't match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                else
                {
                    MessageBox.Show("Minimum password length is 6 characters", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            else
            {
                MessageBox.Show("All fields must be filled in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
