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
    public class FavoritesModel : PageModel
    {
        private readonly ILogger<FavoritesModel> _logger;
        private readonly FavoriteMoviesService _favoriteMoviesService;
        private List<Movie> _allMovies = new List<Movie>(); // Store all movies temporarily

        public List<Movie> FavoriteMovies { get; set; } = new List<Movie>();

        public FavoritesModel(ILogger<FavoritesModel> logger, FavoriteMoviesService favoriteMoviesService)
        {
            _logger = logger;
            _favoriteMoviesService = favoriteMoviesService;
        }

        public void OnGet()
        {
            LoadAllMoviesFromCsv();
            FilterFavoriteMovies();
        }

        private void LoadAllMoviesFromCsv()
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

        private void FilterFavoriteMovies()
        {
            var favoriteTitles = _favoriteMoviesService.GetFavoriteMovieTitles();
            FavoriteMovies = _allMovies.Where(m => favoriteTitles.Contains(m.Название)).ToList();
            
            // Set IsFavorite to true for display
            foreach (var movie in FavoriteMovies)
            {
                movie.IsFavorite = true;
            }
        }

        public IActionResult OnPostRemoveFavorite(string movieTitle)
        {
            _favoriteMoviesService.RemoveFavorite(movieTitle);
            return RedirectToPage(); // Redirect back to refresh the list
        }
    }
} 