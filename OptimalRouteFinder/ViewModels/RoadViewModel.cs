using OptimalRouteFinder.Models;
using System.Windows.Input;

namespace OptimalRouteFinder.ViewModels
{
    public class RoadViewModel
    {
        public int Id { get; set; }
        public City From { get; set; }
        public City To { get; set; }
        public double Distance { get; set; }
        public ICommand DeleteCommand { get; set; }
        public string Display => $"{From.Name} - {To.Name} ({Distance} km)";
    }
}
