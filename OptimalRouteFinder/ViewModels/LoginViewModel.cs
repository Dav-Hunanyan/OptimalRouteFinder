using Microsoft.EntityFrameworkCore;
using OptimalRouteFinder.Data;
using OptimalRouteFinder.Models;
using OptimalRouteFinder.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OptimalRouteFinder.ViewModels
{
    public class LoginViewModel
    {
        private readonly AuthService _authService;
        private MapDbContext _mapDbContext;
        public string Username { get; set; }
        public string Password { get; set; }
        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel(MapDbContext dbContext)
        {
            _mapDbContext = dbContext;
            _authService = new AuthService(_mapDbContext);
            LoginCommand = new RelayCommand(param => Login());
            GoToRegisterCommand = new RelayCommand(_ => OpenRegister());
        }

        private void Login()
        {

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Խնդրում ենք մուտքագրել օգտանունը և գաղտնաբառը");
                return;
            }

            var user = _authService.Login(Username, Password);

            if (user == null)
            {
                MessageBox.Show("Սխալ օգտանուն կամ գաղտնաբառ");
                return;
            }

            // Save session
            UserSession.CurrentUser = user;

            // Open main window
            var mainWindow = new MainWindow(_mapDbContext, user);
            mainWindow.Show();

            // Close login window
            Application.Current.Windows
                .OfType<LoginView>()
                .FirstOrDefault()
                ?.Close();
        }

        private void OpenRegister()
        {
            var registerWindow = new RegisterWindow(_mapDbContext);
            registerWindow.Show();
            Application.Current.Windows
                .OfType<LoginView>()
                .FirstOrDefault()
                ?.Close();
        }
      
    }
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty));

        public static string GetBoundPassword(DependencyObject obj) =>
            (string)obj.GetValue(BoundPassword);

        public static void SetBoundPassword(DependencyObject obj, string value) =>
            obj.SetValue(BoundPassword, value);

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static bool GetBindPassword(DependencyObject obj) =>
            (bool)obj.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject obj, bool value) =>
            obj.SetValue(BindPasswordProperty, value);

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.OldValue)
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                if ((bool)e.NewValue)
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetBoundPassword(passwordBox, passwordBox.Password);
            }
        }
    }
}

