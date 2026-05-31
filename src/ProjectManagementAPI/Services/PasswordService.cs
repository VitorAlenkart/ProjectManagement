using System;
using Microsoft.AspNetCore.Identity;

namespace ProjectManagementAPI.Services
{

	public class PasswordService
	{

        private PasswordHasher<string> _passwordHasher ;

        public PasswordService()
		{
            _passwordHasher = new PasswordHasher<string>();
        }

        public string HashPassword(string email, string password)
        {
            string result = _passwordHasher.HashPassword(email, password);

            return result;
        }

        public bool VerifyPassword(string email, string password)
        {
            string hashedPassword = HashPassword(email, password);
            bool result = _passwordHasher.VerifyHashedPassword(email, hashedPassword, password) == PasswordVerificationResult.Success;

            return result;
        }

    }
}
