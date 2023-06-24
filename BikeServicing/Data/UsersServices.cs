using System.Data;
using System.Text.Json;

// Methods to manage users.
namespace BikeServicing.Data
{
    public static class UsersServices
    {
        public const string SeedUsername = "admin";
        public const string SeedPassword = "admin";

        // Methods to save all user in a file.
        private static void SaveAll(List<User> users)
        {
            string appDataDirectoryPath = Utils.GetAppDirectoryPath();
            string appUsersFilePath = Utils.GetAppUsersFilePath();

            if (!Directory.Exists(appDataDirectoryPath))
            {
                Directory.CreateDirectory(appDataDirectoryPath);
            }

            var json = JsonSerializer.Serialize(users);
            File.WriteAllText(appUsersFilePath, json);
        }

        // To create a list collection of users.
        public static List<User> GetAll()
        {
            string appUsersFilePath = Utils.GetAppUsersFilePath();
            if (!File.Exists(appUsersFilePath))
            {
                return new List<User>();
            }

            var json = File.ReadAllText(appUsersFilePath);

            return JsonSerializer.Deserialize<List<User>>(json);
        }

        // To set a limit to admins.
        public static bool AdminLimit()
        {
            int count = GetAll().Where(x => x.Role == Roles.Admin).Count();
            return count < 2;
        }
        
        // To create a user.
        public static List<User> Create(Guid userId, string username, string password, Roles role)
        {
            List<User> users = GetAll();
            bool usernameExists = users.Any(x => x.Username == username);

            if (usernameExists)
            {
                throw new Exception("Username already exists.");
            }
            else if (role == Roles.Admin && !AdminLimit())
            {
                throw new Exception("There cannot be more than two admins.");
            }
            else
            {
                users.Add(
                new User
                {
                    Username = username,
                    PasswordHash = Utils.HashSecret(password),
                    Role = role,
                    CreatedBy = userId
                }
            );
                SaveAll(users);
                return users;
            }
        }

        // The first user of the system.
        public static void SeedUsers()
        {
            var users = GetAll().FirstOrDefault(x => x.Role == Roles.Admin);

            if (users == null)
            {
                Create(Guid.Empty, SeedUsername, SeedPassword, Roles.Admin);
            }
        }

        // Get user by id.
        public static User GetById(Guid id)
        {
            List<User> users = GetAll();
            return users.FirstOrDefault(x => x.Id == id);
        }

        // Delete user.
        public static List<User> Delete(Guid id)
        {
            List<User> users = GetAll();
            User user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            List<InventoryRecord> records = ItemServices.FetchItemsRecord();
            records.RemoveAll(record => user.Id == record.TakenBy || user.Id == record.ApprovedBy);

            ItemServices.SaveItemsLogs(records);
            users.Remove(user);
            SaveAll(users);

            return users;
        }

        // Login the system.
        public static User Login(string username, string password)
        {
            var loginErrorMessage = "Invalid username or password.";
            List<User> users = GetAll();
            User user = users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception(loginErrorMessage);
            }

            bool passwordIsValid = Utils.VerifyHash(password, user.PasswordHash);

            if (!passwordIsValid)
            {
                throw new Exception(loginErrorMessage);
            }

            return user;
        }
    }
}
