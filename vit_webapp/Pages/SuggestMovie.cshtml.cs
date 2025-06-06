using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using vit_webapp.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace vit_webapp.Pages
{
    public class SuggestMovieModel : PageModel
    {
        private readonly ILogger<SuggestMovieModel> _logger;
        private List<Movie> _allMovies = new List<Movie>(); // Store all movies

        public Movie SuggestedMovie { get; set; }

        [BindProperty]
        public List<string> SelectedGenres { get; set; } = new List<string>();
        
        [BindProperty]
        public double? MinRating { get; set; }

        [BindProperty]
        public int? MinYear { get; set; }

        [BindProperty]
        public int? MaxYear { get; set; }

        public List<string> AllGenres { get; set; } = new List<string>(); // All unique genres for selection

        public SuggestMovieModel(ILogger<SuggestMovieModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            LoadMoviesFromCsv();
            PopulateAllGenres();
             // No suggested movie on initial load
        }

        public IActionResult OnPost()
        {
            LoadMoviesFromCsv();
            PopulateAllGenres(); // Repopulate genres on post

            IEnumerable<Movie> possibleMovies = _allMovies;

            // Apply genre filter if genres are selected
            if (SelectedGenres != null && SelectedGenres.Any())
            {
                 possibleMovies = possibleMovies.Where(m =>
                    m.Жанры.Split(',').Select(g => g.Trim()).Intersect(SelectedGenres).Any());
            }
            
            // Apply minimum rating filter
            if (MinRating.HasValue)
            {
                possibleMovies = possibleMovies.Where(m => m.Рейтинг >= MinRating.Value);
            }

            // Apply year range filter
            if (MinYear.HasValue)
            {
                possibleMovies = possibleMovies.Where(m => m.Год_выпуска >= MinYear.Value);
            }
            if (MaxYear.HasValue)
            {
                possibleMovies = possibleMovies.Where(m => m.Год_выпуска <= MaxYear.Value);
            }

            // Select a random movie from the filtered list
            Random random = new Random();
            SuggestedMovie = possibleMovies.OrderBy(m => random.Next()).FirstOrDefault();

            // If no movie was suggested, perhaps log a warning or set a message
            if (SuggestedMovie == null)
            {
                _logger.LogWarning("No movie found matching selected criteria.");
                // Optionally add a message to display on the page
            }
            
            return Page(); // Stay on the same page to display the result
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
                    var values = line.Split(',');
                    // Expecting 6 columns: Название, Жанры, Описание, Год выпуска, Постер, Рейтинг
                    if (values.Length == 6)
                    {
                        try
                        {
                            _allMovies.Add(new Movie
                            {
                                Название = values[0].Trim('"'),
                                Жанры = values[1].Trim('"'),
                                Описание = values[2].Trim('"'),
                                Год_выпуска = int.TryParse(values[3], out int year) ? year : 0,
                                Постер = values[4].Trim('"'),
                                Рейтинг = double.TryParse(values[5].Trim('"'), NumberStyles.Any, CultureInfo.InvariantCulture, out double rating) ? rating : 0.0
                            });
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing CSV line: {Line}", line);
                        }
                    }
                     else
                    {
                        _logger.LogWarning("CSV line has unexpected number of columns ({ColumnCount}): {Line}", values.Length, line);
                    }
                }
            }
            else
            {
                 _logger.LogWarning("movies.csv not found at {Path}", csvFilePath);
            }
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
    }
} 