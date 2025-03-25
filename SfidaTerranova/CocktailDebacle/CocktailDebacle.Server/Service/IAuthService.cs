using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocktailDebacle.Server.Service
{
    public interface IAuthService
    {
        Task<string> AuthenticateUser(string UserName, string password);
    }
}