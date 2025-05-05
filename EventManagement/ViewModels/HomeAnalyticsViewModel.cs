using EventManagement.Models;


namespace EventManagement.ViewModels
{
    public class AnalyticsViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalUsers { get; set; }
        public int ExpertOrganizers { get; set; }
        public int CitiesCovered { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
