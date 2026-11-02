using System.Globalization;
using System.Text.Json.Nodes;
using GameStore.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data
{
    public class DataSeeder
    {
        private readonly GameDbContext dbContext;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IHttpClientFactory httpClientFactory;

        public DataSeeder(GameDbContext dbContext,
         IPasswordHasher<User> passwordHasher,
         IHttpClientFactory httpClientFactory)
        {
            this.dbContext = dbContext;
            this.passwordHasher = passwordHasher;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task SeedAsync()
        {
            await SeedCategoriesAsync();
            await SeedGamesAsync();
            await SeedAdminAsync();

        }

        private async Task SeedCategoriesAsync()
        {
            var anyCategory = await dbContext.Categories.AnyAsync();
            if (anyCategory)
            {
                return;
            }

            var categories = new string[]
            {
                "Action",
                "Adventure",
                "RPG",
                "Racing",
                "Fighting",
                "Idle",
                "Roguelike",
                "Strategy",
                "Music",
                "Rhytm",
                "FPS",
                "RTS",
                "Novel",
                "Quest",
                "Puzzle",
                "VR",
                "Simulation",
                "Indie",
                "Platformer"
            };

            foreach (var category in categories)
            {
                dbContext.Categories.Add(new Category { Name = category.Trim() });
            }

            await dbContext.SaveChangesAsync();
        }

        private async Task SeedGamesAsync()
        {
            var anyGame = await dbContext.Games.AnyAsync();
            if (anyGame)
            {
                return;
            }

            var appIds = new[]
            {
                413150,
                105600,
                236850,
                1817070,
                4126040,
                4732690,
                731490,
                996580,
                1378990,
                2215200,
                728880,
                1426210,
                5335510,
                433340,
                1657630,
                1641700,
                3527290,
                284160,
                264710,
                227300,
                305620,
                275850,
                1798230,
                2929250
            };

            Random random = new Random((int)DateTime.UtcNow.Ticks);

            var categoires = await dbContext.Categories.ToListAsync();
            var httpClient = httpClientFactory.CreateClient();

            foreach (var appId in appIds)
            {
                var response = await httpClient.GetAsync($"https://store.steampowered.com/api/appdetails?appids={appId}&cc=us&l=english");
                if (response.StatusCode is not System.Net.HttpStatusCode.OK)
                {
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();
                var game = ParseGame(appId, content, categoires, random);
                if (game is null)
                {
                    continue;
                }
                dbContext.Games.Add(game);
            }

            await dbContext.SaveChangesAsync();
        }

        private Game? ParseGame(int appId, string content, List<Category> categoires, Random random)
        {
            var root = JsonNode.Parse(content);
            var data = root?[appId.ToString()]?["data"];
            if (data is null)
            {
                return null;
            }

            var game = new Game();

            var name = data["name"]?.GetValue<string>();
            if (name is null)
            {
                return null;
            }
            game.Name = name;
            game.Description = data["short_description"]?.GetValue<string>();
            var cents = data["price_overview"]?["final"]?.GetValue<int>() ?? 0;
            game.Price = cents / 100m;

            game.ImageUrl = data["header_image"]?.GetValue<string>();
            var dateText = data["release_date"]?["date"]?.GetValue<string>();
            //30 May, 2023
            if (DateTime.TryParse(dateText, CultureInfo.GetCultureInfo("en-US"),
                DateTimeStyles.None, out var parsed))
            {
                game.ReleaseDate = DateOnly.FromDateTime(parsed);
            }
            else
            {
                game.ReleaseDate = null;
            }

            Category? category = null;
            var genres = data["genres"]?.AsArray();
            if (genres is not null)
            {
                foreach (var genre in genres)
                {
                    var desc = genre?["description"]?.GetValue<string>()??string.Empty;
                    category = categoires.FirstOrDefault(x => x.Name.ToLower().Equals(desc.ToLower()));
                    if(category is not null)
                    {
                        break;
                    }
                }
            }

            if (category is null)
            {
                category = categoires[random.Next(0, categoires.Count)];
            }

            game.CategoryId = category.Id;

            return game;
        }

        private async Task SeedAdminAsync()
        {
            var anyAdmin = await dbContext.Users.AnyAsync(x => x.Role == "Admin" && x.Email == "admin@gmail.com");
            if (anyAdmin)
            {
                return;
            }

            var user = new User { Email = "admin@gmail.com", Role = "Admin" };
            user.PasswordHash = passwordHasher.HashPassword(user, "admin123");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
    }
}