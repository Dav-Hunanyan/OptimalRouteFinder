using Microsoft.EntityFrameworkCore;
using OptimalRouteFinder.Data;
using OptimalRouteFinder.ViewModels;
using System.Windows;

namespace OptimalRouteFinder
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _vm;
        public LoginView()
        {
            InitializeComponent();
            var options = new DbContextOptionsBuilder<MapDbContext>()
               .UseSqlite("Data Source=MapDatabase.db") // single portable file
               .Options;

            var dbContext = new MapDbContext(options);
            dbContext.Database.EnsureCreated();
            _vm = new LoginViewModel(dbContext);
            this.DataContext = _vm;
        }
        public LoginView(MapDbContext mapDbContext)
        {
            InitializeComponent();          
            _vm = new LoginViewModel(mapDbContext);
            this.DataContext = _vm;
        }
    }
}
