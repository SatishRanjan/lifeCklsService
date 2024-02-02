using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordHelper
{
    public static string GetPasswordHash(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Convert the password string to a byte array
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Compute the hash value of the byte array
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Convert the hash byte array to a hexadecimal string
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashedPassword;
        }
    }

    public static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
    {
        // Hash the entered password using the same method used during registration
        string enteredPasswordHash = GetPasswordHash(enteredPassword);

        // Compare the entered password hash with the stored hashed password
        return enteredPasswordHash.Equals(storedHashedPassword, StringComparison.OrdinalIgnoreCase);
    }
}