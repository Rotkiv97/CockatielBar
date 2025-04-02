using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocktailDebacle.Server.Models;

namespace CocktailDebacle.Server.Service
{
    public interface IAuthService
    {
        Task<string> AuthenticateUser(string UserName, string password, User user);
    }
}