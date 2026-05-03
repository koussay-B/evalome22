
using api.data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.data
{
    public class SeedData
    {
        private const string DefaultPassword = "Password";
        public static async Task SaveSeedData(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ApplicationContext context)
        {
            Console.WriteLine("Starting data seeding process...");

            // Seed roles
            await SeedRoles(roleManager);

            // // Seed groups
            // var groups = await SeedGroups(context);

            // // Seed users
            await SeedUsers(userManager);

            Console.WriteLine("Data seeding completed.");
        }
        private static async Task SeedRoles(RoleManager<AppRole> roleManager)
        {
            var roles = Enum.GetNames<UserRole>();

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new AppRole { Name = roleName });
                    Console.WriteLine($"Created role: {roleName}");
                }
            }
        }

        private static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            // 1. Ensure core system users exist
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                var roleName = role.ToString().ToLower();
                var email = $"{roleName}@{roleName}.com";
                await CreateOrUpdateCoreUser(userManager, email, roleName, role.ToString(), DefaultPassword);
            }

            // 2. UNIVERSAL FIX: Align ALL users in the database
            // This ensures every user (including those created via UI) 
            // is compatible with the Dev Panel's default password.
            var allUsers = await userManager.Users.ToListAsync<AppUser>();
            Console.WriteLine($"Synchronizing {allUsers.Count} users for Development environment...");

            foreach (var user in allUsers)
            {
                // Clean the username: lowercase and remove spaces
                var cleanUserName = user.UserName?.Replace(" ", "").ToLower() ?? "user";
                if (user.UserName != cleanUserName)
                {
                    user.UserName = cleanUserName;
                    await userManager.UpdateAsync(user);
                }

                // Force reset password to DefaultPassword
                // Note: RemovePasswordAsync followed by AddPasswordAsync is the most 
                // reliable way to ensure the password matches during data seeding.
                await userManager.RemovePasswordAsync(user);
                var result = await userManager.AddPasswordAsync(user, DefaultPassword);
                
                if (result.Succeeded) {
                    Console.WriteLine($"Aligned user: {user.UserName} ({user.Email})");
                }
            }
        }

        private static async Task CreateOrUpdateCoreUser(UserManager<AppUser> userManager, string email, string userName, string role, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    Console.WriteLine($"Seeded core user: {userName}");
                }
            }
        }
    }

}