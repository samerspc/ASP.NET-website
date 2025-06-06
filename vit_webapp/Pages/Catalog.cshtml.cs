using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using vit_webapp.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using vit_webapp.Services;
using System;
using System.Text;

namespace vit_webapp.Pages
{
    public class CatalogModel : PageModel
    {
        private readonly ILogger<CatalogModel> _logger;
        private readonly FavoriteMoviesService _favoriteMoviesService;
        private List<Movie> _allMovies = new List<Movie>(); // Store all movies

        public List<Movie> Movies { get; set; } = new List<Movie>(); // Filtered list for display
        
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public List<string> SelectedGenres { get; set; } = new List<string>();

        public List<string> AllGenres { get; set; } = new List<string>(); // All unique genres for filtering

        public CatalogModel(ILogger<CatalogModel> logger, FavoriteMoviesService favoriteMoviesService)
        {
            _logger = logger;
            _favoriteMoviesService = favoriteMoviesService;
        }

        public void OnGet()
        {
            LoadMoviesFromCsv();
            PopulateAllGenres();
            ApplyFilters();
            SetFavoriteStatus();
        }

        private void LoadMoviesFromCsv()
        {
            string csvFilePath = Path.Combine(AppContext.BaseDirectory, "movies.csv");
            _allMovies = new List<Movie>(); // Clear previous data

            if (System.IO.File.Exists(csvFilePath))
            {
                var lines = System.IO.File.ReadAllLines(csvFilePath);
                // Skip header row
                foreach (var line in lines.Skip(1))
                {
                    var values = ParseCsvLine(line);
                    // Expecting 6 columns: Название, Жанры, Описание, Год выпуска, Постер, Рейтинг
                    if (values.Count == 6)
                    {
                        try
                        {
                            _allMovies.Add(new Movie
                            {
                                Название = values[0].Trim('"'),
                                Жанры = values[1].Trim('"'),
                                Описание = values[2].Trim('"'),
                                Год_выпуска = int.TryParse(values[3].Trim('"'), out int year) ? year : 0,
                                Постер = values[4].Trim('"'),
                                Рейтинг = double.TryParse(values[5].Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out double rating) ? rating : 0.0
                            });
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing CSV line to Movie object: {Line}", line);
                        }
                    }
                     else
                    {
                        _logger.LogWarning("CSV line has unexpected number of columns ({ColumnCount}): {Line}", values.Count, line);
                    }
                }
            }
            else
            {
                 _logger.LogWarning("movies.csv not found at {Path}", csvFilePath);
            }
        }

        // Simple CSV parsing function that handles quoted fields with commas
        private List<string> ParseCsvLine(string line)
        {
            List<string> values = new List<string>();
            bool inQuotes = false;
            System.Text.StringBuilder currentValue = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Handle escaped quote (double quote inside quotes)
                        currentValue.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString()); // Add the last value
            return values;
        }

        private void PopulateAllGenres()
        {
            AllGenres = _allMovies
                .SelectMany(m => m.Жанры.Split(',').Select(g => g.Trim()))
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        private void ApplyFilters()
        {
            IEnumerable<Movie> filteredMovies = _allMovies;

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredMovies = filteredMovies.Where(m =>
                    m.Название.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Жанры.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Описание.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply genre filter
            if (SelectedGenres != null && SelectedGenres.Any())
            {
                 filteredMovies = filteredMovies.Where(m =>
                    m.Жанры.Split(',').Select(g => g.Trim()).Intersect(SelectedGenres).Any());
            }

            Movies = filteredMovies.ToList();
        }

        private void SetFavoriteStatus()
        {
            foreach (var movie in Movies)
            {
                movie.IsFavorite = _favoriteMoviesService.IsFavorite(movie.Название);
            }
        }

         public IActionResult OnPostAddFavorite(string movieTitle)
        {
            _favoriteMoviesService.AddFavorite(movieTitle);
            return RedirectToPage(new { SearchTerm = SearchTerm, SelectedGenres = SelectedGenres }); // Redirect back with current filters
        }

        public IActionResult OnPostRemoveFavorite(string movieTitle)
        {
            _favoriteMoviesService.RemoveFavorite(movieTitle);
            return RedirectToPage(new { SearchTerm = SearchTerm, SelectedGenres = SelectedGenres }); // Redirect back with current filters
        }
    }
} 