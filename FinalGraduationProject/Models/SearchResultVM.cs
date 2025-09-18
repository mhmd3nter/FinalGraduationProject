namespace FinalGraduationProject.Models
{
    public class SearchResultVM
        {
            public List<Product> Products { get; set; } = new();
            // Add other collections if you want to search more entities:
            // public List<Sport> Sports { get; set; } = new();
            // public List<Boot> Boots { get; set; } = new();
            // public List<TrendingSelling> TrendingSellings { get; set; } = new();
            // public List<Oxford> Oxfords { get; set; } = new();
        }
    }

