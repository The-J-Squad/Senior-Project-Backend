using MongoDB.Driver;
using RecipeBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeBackend.Repositories
{
    public interface IAccountRepository
    {
        Task<bool> TryValidatingAccount(AccountInformation credentials);
        Task<bool> TryAddAccountAsync(AccountInformation credentials);
        Task<ReplaceOneResult> UpdateAccount(string username, AccountInformation credentials);
    }
}
