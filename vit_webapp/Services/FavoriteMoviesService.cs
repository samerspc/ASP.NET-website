using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace vit_webapp.Services
{
    public class FavoriteMoviesService
    {
        private readonly string _favoritesFilePath;
        private readonly ILogger<FavoriteMoviesService> _logger;
        private List<string> _favoriteMovieTitles = new List<string>();
        private readonly object _lock = new object(); // For thread-safe file access

        public FavoriteMoviesService(ILogger<FavoriteMoviesService> logger)
        {
            // Path within the Docker container
            _favoritesFilePath = Path.Combine(AppContext.BaseDirectory, "favorites.json");
            _logger = logger;
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            lock (_lock)
            {
                if (File.Exists(_favoritesFilePath))
                {
                    try
                    {
                        var jsonString = File.ReadAllText(_favoritesFilePath);
                        _favoriteMovieTitles = JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading favorites from {FilePath}", _favoritesFilePath);
                        _favoriteMovieTitles = new List<string>();
                    }
                }
                else
                {
                    _logger.LogInformation("Favorites file not found, starting with empty favorites: {FilePath}", _favoritesFilePath);
                    _favoriteMovieTitles = new List<string>();
                }
            }
        }

        private void SaveFavorites()
        {
            lock (_lock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var jsonString = JsonSerializer.Serialize(_favoriteMovieTitles, options);
                    File.WriteAllText(_favoritesFilePath, jsonString);
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Error saving favorites to {FilePath}", _favoritesFilePath);
                }
            }
        }

        public bool IsFavorite(string movieTitle)
        {
            lock (_lock)
            {
                return _favoriteMovieTitles.Contains(movieTitle);
            }
        }

        public void AddFavorite(string movieTitle)
        {
            lock (_lock)
            {
                if (!_favoriteMovieTitles.Contains(movieTitle))
                {
                    _favoriteMovieTitles.Add(movieTitle);
                    SaveFavorites();
                    _logger.LogInformation("Added '{MovieTitle}' to favorites.", movieTitle);
                }
            }
        }

        public void RemoveFavorite(string movieTitle)
        {
            lock (_lock)
            {
                if (_favoriteMovieTitles.Contains(movieTitle))
                {
                    _favoriteMovieTitles.Remove(movieTitle);
                    SaveFavorites();
                     _logger.LogInformation("Removed '{MovieTitle}' from favorites.", movieTitle);
                }
            }
        }

        public List<string> GetFavoriteMovieTitles()
        {
             lock (_lock)
            {
                return new List<string>(_favoriteMovieTitles); // Return a copy to prevent external modification
            }
        }
    }
} 