# Movie Catalog Website

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white) ![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white) ![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

This is a simple ASP.NET Core Razor Pages web application for viewing a movie catalog, managing favorites, and getting movie suggestions based on filters.

## Features

- Display a curated selection of movies on the main page.
- View a full catalog of movies with search and genre filtering.
- Add and remove movies from a favorites list, persisted to a JSON file.
- Get movie suggestions based on selected genres, minimum rating, and year range.
- Movies data is loaded from a CSV file.

## Prerequisites

- Docker and Docker Compose installed (for Docker setup).
- [.NET SDK 7.0 or later](https://dotnet.microsoft.com/download) (for local setup).

## Setup and Running with Docker

1.  Clone or download the project files.
2.  Place your `movies.csv` file in the root directory of the `vit_webapp` folder (e.g., `vit/vit_webapp/movies.csv`). Ensure your CSV file has the following columns in this exact order:
    `Название,Жанры,Описание,Год выпуска,Постер,Рейтинг`
    Quoted fields with commas are supported.
3.  Navigate to the root directory of the project in your terminal (where `docker-compose.yml` is located):
    ```bash
    cd /path/to/your/vit
    ```
4.  Build and run the Docker containers using Docker Compose:
    ```bash
    docker-compose up --build -d
    ```
    The `--build` flag is important the first time you run it or after making changes to the code/Dockerfile.
    The `-d` flag runs the containers in detached mode (in the background).
5.  The application will be available at `http://localhost:8080` in your web browser.

## Stopping the application (Docker)

To stop the running containers, navigate to the project's root directory in your terminal and run:
```bash
docker-compose down
```

## Running Locally (without Docker)

1.  Ensure you have the [.NET SDK 7.0 or later](https://dotnet.microsoft.com/download) installed.
2.  Place your `movies.csv` file in the root directory of the `vit_webapp` folder (e.g., `vit/vit_webapp/movies.csv`).
3.  Navigate to the `vit_webapp` directory in your terminal:
    ```bash
    cd /path/to/your/vit/vit_webapp
    ```
4.  Run the application using the .NET CLI:
    ```bash
    dotnet run
    ```
5.  The application will typically be available at `https://localhost:7095` or `http://localhost:5241` (the exact port is shown in the terminal output when you run the command).

## Project Structure (Key Files)

- `docker-compose.yml`: Defines the services to be run (the web application).
- `vit_webapp/Dockerfile`: Dockerfile to build the ASP.NET Core application image.
- `vit_webapp/Pages/Index.cshtml` and `Index.cshtml.cs`: Main page showing a random selection.
- `vit_webapp/Pages/Catalog.cshtml` and `Catalog.cshtml.cs`: Full movie catalog page with filters.
- `vit_webapp/Pages/Favorites.cshtml` and `Favorites.cshtml.cs`: Page displaying favorite movies.
- `vit_webapp/Pages/SuggestMovie.cshtml` and `SuggestMovie.cshtml.cs`: Movie suggestion/quiz page.
- `vit_webapp/Models/Movie.cs`: C# model for movie data.
- `vit_webapp/Services/FavoriteMoviesService.cs`: Service for managing favorite movies.
- `vit_webapp/wwwroot/css/site.css`: Custom CSS styles.
- `vit_webapp/Pages/Shared/_Layout.cshtml`: Main layout file.
- `vit_webapp/Program.cs`: Application startup and service configuration.

