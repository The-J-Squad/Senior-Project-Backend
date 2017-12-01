using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using RecipeBackend.Models;
using Microsoft.Extensions.Options;
using RecipeBackend.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace RecipeBackend.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountContext _context = null;

        public AccountRepository(IOptions<Settings> settings)
        {
            _context = new AccountContext(settings);
        }

        public async Task<bool> TryAddAccountAsync(AccountInformation credentials)
        {
            try
            {
                Hash(credentials);
                await _context.Accounts.InsertOneAsync(credentials);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AccountInformation> GetAccount(string username)
        {
            var filter = Builders<AccountInformation>.Filter.Eq("Username", username);

            try
            {
                return await _context.Accounts
                                .Find(filter)
                                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> TryValidatingAccount(AccountInformation credentials)
        {
            AccountInformation properCredentials;
            try
            {
                properCredentials = await GetAccount(credentials.Username);
                credentials.Salt = properCredentials.Salt;
            }
            catch
            {
                return false;
            }

            Hash(credentials);

            if (properCredentials.Password.Equals(credentials.Password))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<ReplaceOneResult> UpdateAccount(string username, AccountInformation credentials)
        {
            if (await TryValidatingAccount(credentials))
            {

                try
                {
                    return await _context.Accounts
                                .ReplaceOneAsync(n => n.Username.Equals(username)
                                                , credentials
                                                , new UpdateOptions { IsUpsert = true });
                }
                catch (Exception ex)
                {
                    // log or manage the exception
                    throw ex;
                }
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }

        private void Hash(AccountInformation info)
        {
            string password = info.Password;

            if (info.Salt == null || info.Salt.Length == 0)
            {
                // generate a 128-bit salt using a secure PRNG
                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                info.Salt = salt;
            }

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            info.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: info.Salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}

