using AutoTrack.Database;
using System;
using System.Data;
using System.Data.SqlClient;

namespace AutoTrack.Helpers
{
    public static class PasswordMigration
    {
        public static void MigrateExistingPasswords()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT UserID, Password FROM Users");
                int migratedCount = 0;

                foreach (DataRow row in dt.Rows)
                {
                    int userId = Convert.ToInt32(row["UserID"]);
                    string currentPassword = row["Password"].ToString();

                    bool isAlreadyHashed = currentPassword.Length > 30 &&
                                           (currentPassword.Contains("+") || currentPassword.Contains("/") || currentPassword.Contains("="));

                    if (!isAlreadyHashed && !string.IsNullOrEmpty(currentPassword))
                    {
                        string hashedPassword = PasswordHelper.HashPassword(currentPassword);

                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE Users SET Password = @Hashed WHERE UserID = @ID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@Hashed", hashedPassword),
                                new SqlParameter("@ID", userId)
                            });

                        migratedCount++;
                    }
                }

                // Use Debug.WriteLine instead of Console.WriteLine for WinForms
                System.Diagnostics.Debug.WriteLine($"Migration complete! {migratedCount} users migrated.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
            }
        }
    }
}