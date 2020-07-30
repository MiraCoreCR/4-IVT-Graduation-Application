using EpicControlShell.Resources;
using EpicControlShell.Resources.Entities;
using EpicControlShell.Windows.General;
using System;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Linq;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using System.Collections.Generic;

namespace EpicControlShell.Windows.Auth
{
    public partial class AuthorizationWindow : Window
    {
        private Auth0Client client;

        EpicDbContext context = DatabaseProvider.GetInstance();

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e) 
        {
            LogInButton.IsEnabled = false;
            SignInWithGoogleButton.IsEnabled = false;
            RegisterButton.IsEnabled = false;
            ForgetPassButton.IsEnabled = false;

            try
            {
                var user = context.Users.Where(u => u.NickName == LoginTextBox.Text).First();

                if (user != null)
                {
                    string inputPassword = PasswordTextBox.Password;
                    byte[] data = Encoding.Default.GetBytes(inputPassword);
                    var result = new SHA256Managed().ComputeHash(data);

                    if (Convert.ToString(user.Password) == BitConverter.ToString(result).Replace("-", "").ToLower())
                    {
                        InformationAboutCurrnetUser.UserId = user.Id;
                        InformationAboutCurrnetUser.GroupID = user.GroupId;

                        MainWindow mainWindow = new MainWindow(client);
                        mainWindow.Show();
                        this.Close();
                    }

                    else
                        MessageBox.Show("Incorrent password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else
                    MessageBox.Show("There is no user with this nickname or there are more than one of them ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoginWithGoogleButton_Click(object sender, RoutedEventArgs e)
        {
            LogInButton.IsEnabled = false;
            SignInWithGoogleButton.IsEnabled = false;
            RegisterButton.IsEnabled = false;
            ForgetPassButton.IsEnabled = false;

            Auth0ClientOptions clientOptions = new Auth0ClientOptions
            {
                Domain = "epiccontrollshellapp.auth0.com",
                ClientId = "V7YlfGUCzVQiRrLhIQVeF4z1KMWSUteF"
            };
            client = new Auth0Client(clientOptions);
            clientOptions.PostLogoutRedirectUri = clientOptions.RedirectUri;

            var extraParameters = new Dictionary<string, string>();

            extraParameters.Add("connection", "google-oauth2");

            SignInWithGoogle(await client.LoginAsync(extraParameters: extraParameters));
        }

        private void SignInWithGoogle(LoginResult loginResult)
        {
            // Display error
            if (loginResult.IsError)
            {
                MessageBox.Show(loginResult.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LogInButton.IsEnabled = true;
                SignInWithGoogleButton.IsEnabled = true;
                RegisterButton.IsEnabled = true;
                ForgetPassButton.IsEnabled = true;
                return;
            }

            string NickName = null, FullName = null, Email = null;

            foreach (var claim in loginResult.User.Claims)
            {
                if (claim.Type == "name")
                {
                    FullName = claim.Value;
                }

                else if (claim.Type == "nickname")
                {
                    NickName = claim.Value;
                }

                else if (claim.Type == "email")
                {
                    Email = claim.Value;
                }
            }

            try
            {
                var googleUser = context.Users.Where(gu => gu.NickName == NickName).FirstOrDefault();
                if (googleUser != null)
                {
                    InformationAboutCurrnetUser.UserId = googleUser.Id;
                    InformationAboutCurrnetUser.GroupID = googleUser.GroupId;

                    MainWindow mainWindow = new MainWindow(client);
                    mainWindow.Show();
                    this.Close();
                }

                else
                {
                    RegistrationWindow registration = new RegistrationWindow(NickName, FullName, Email);

                    if (registration.ShowDialog() == true)
                    {
                        var user = context.Users.Where(u => u.NickName == NickName).FirstOrDefault();

                        InformationAboutCurrnetUser.UserId = user.Id;
                        InformationAboutCurrnetUser.GroupID = user.GroupId;

                        MainWindow mainWindow = new MainWindow(client);
                        mainWindow.Show();
                        this.Close();
                    }

                    else
                    {
                        LogInButton.IsEnabled = true;
                        SignInWithGoogleButton.IsEnabled = true;
                        RegisterButton.IsEnabled = true;
                        ForgetPassButton.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e) 
        {
            RegistrationWindow registration = new RegistrationWindow();
            registration.ShowDialog();
        }

        private void ForgetPasswordButton_Click(object sender, RoutedEventArgs e) 
        {
            ForgetPasswordWindow forgetPasswordWindow = new ForgetPasswordWindow();
            forgetPasswordWindow.ShowDialog();
        }
    }
}
