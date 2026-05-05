using OptimalRouteFinder.Data;
using System.Windows.Input;
using System.Windows;
using OptimalRouteFinder.Services;
using System.Text.RegularExpressions;

namespace OptimalRouteFinder.ViewModels
{
    public class RegisterViewModel
    {
        private readonly UserService _userService;
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel(MapDbContext dbContext)
        {
            _userService = new UserService(dbContext);

            RegisterCommand = new RelayCommand(_ => Register(dbContext));
            GoToLoginCommand = new RelayCommand(_ => OpenLogin(dbContext));
        }

        private void Register(MapDbContext mapDbContext)
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                MessageBox.Show("Խնդրում ենք լրացնել բոլոր պարտադիր դաշտերը");
                return;
            }
            if (!IsValidEmail(Email))
            {
                MessageBox.Show("Էլեկտրոնային փոստի հասցեն սխալ է");
                return;
            }

            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Գաղտնաբառերը չեն համընկնում");
                return;
            }
            if (!IsValidPassword(Password))
            {
                MessageBox.Show("Գաղտնաբառը պետք է պարունակի առնվազն 8 նիշ, ներառյալ մեծատառ, փոքրատառ, թիվ և հատուկ նշան");
                return;
            }
            try
            {
                _userService.RegisterUser(Username, Email, Password);
                MessageBox.Show("Գրանցումը հաջողությամբ ավարտվեց: Խնդրում ենք մուտք գործել համակարգ");
                OpenLogin(mapDbContext);
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenLogin(MapDbContext mapDbContext)
        {
            var loginWindow = new LoginView(mapDbContext);
            loginWindow.Show();

            Application.Current.Windows
                .OfType<RegisterWindow>()
                .FirstOrDefault()
                ?.Close();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
       
        private bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
