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
                "FPS",
                "RTS",
                "Novel",
                "Quest",
                "Puzzle",
                "VR"
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
                730,    // Counter-Strike 2
                570,    // Dota 2
                578080, // PUBG
                440,    // Team Fortress 2
                252490, // Rust
                304930, // Unturned
                892970, // Valheim
                1085660,// Destiny 2
                1172470,// Apex Legends
                1091500,// Cyberpunk 2077
                271590, // GTA V
                292030, // The Witcher 3
                1086940,// Baldur's Gate 3
                413150, // Stardew Valley
                105600, // Terraria
                4000,   // Garry's Mod
                1145360,// Hades
                236850, // Europa Universalis IV
                1245620,// Elden Ring
                1817070 // Marvel's Spider-Man Remastered
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

            var firstGenre = data["genres"]?[0]?["description"]?.GetValue<string>() ?? string.Empty;
            var category = categoires.FirstOrDefault(x => x.Name.ToLower().Equals(firstGenre.ToLower()));
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