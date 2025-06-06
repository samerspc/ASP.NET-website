using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using vit_webapp.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;
using vit_webapp.Services;
using System;

namespace vit_webapp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly FavoriteMoviesService _favoriteMoviesService;

    public List<Movie> Movies { get; set; } = new List<Movie>();

    public IndexModel(ILogger<IndexModel> logger, FavoriteMoviesService favoriteMoviesService)
    {
        _logger = logger;
        _favoriteMoviesService = favoriteMoviesService;
    }

    public void OnGet()
    {
        string csvFilePath = Path.Combine(AppContext.BaseDirectory, "movies.csv");
        List<Movie> allMovies = new List<Movie>();

        if (System.IO.File.Exists(csvFilePath))
        {
            var lines = System.IO.File.ReadAllLines(csvFilePath);
            // Skip header row
            foreach (var line in lines.Skip(1))
            {
                 var values = ParseCsvLine(line);
                // Expecting 6 columns now: Название, Жанры, Описание, Год выпуска, Постер, Рейтинг
                if (values.Count == 6)
                {
                    try
                    {
                        allMovies.Add(new Movie
                        {
                            Название = values[0].Trim('"'),
                            Жанры = values[1].Trim('"'),
                            Описание = values[2].Trim('"'),
                            Год_выпуска = int.TryParse(values[3].Trim('"'), out int year) ? year : 0,
                            Постер = values[4].Trim('"'),
                            // Use InvariantCulture to handle potential comma/dot decimal separators
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

        // Select a random subset of movies
        Random random = new Random();
        int numberOfRandomMovies = 9; // You can adjust this number
        Movies = allMovies.OrderBy(m => random.Next()).Take(numberOfRandomMovies).ToList();

        // Set IsFavorite property
        foreach (var movie in Movies)
        {
            movie.IsFavorite = _favoriteMoviesService.IsFavorite(movie.Название);
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

    public IActionResult OnPostAddFavorite(string movieTitle)
    {
        _favoriteMoviesService.AddFavorite(movieTitle);
        return RedirectToPage(); // Redirect back to the current page
    }

    public IActionResult OnPostRemoveFavorite(string movieTitle)
    {
        _favoriteMoviesService.RemoveFavorite(movieTitle);
        return RedirectToPage(); // Redirect back to the current page
    }
}
