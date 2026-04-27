using System.Windows;
using Microsoft.EntityFrameworkCore;
using OptimalRouteFinder.Data.Entities;
using OptimalRouteFinder.Data;
using OptimalRouteFinder.ViewModels;
using OptimalRouteFinder.Services;

namespace OptimalRouteFinder
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        public MainWindow(MapDbContext dbContext, User user)
        {
            InitializeComponent();

            var templateService = new TemplateService(dbContext);

            // Initialize MainViewModel with the service
            _vm = new MainViewModel(templateService,user);

            this.DataContext = _vm;
        }

        private void MapCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SetCanvas(MapCanvas);
            }

            // Handle resize
            MapCanvas.SizeChanged += (s, ev) =>
            {
                if (DataContext is MainViewModel vm2)
                {
                    vm2.SetCanvas(MapCanvas);
                }
            };
        }

    }
}