using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocktailDebacle.Server.Models;
using CocktailDebacle.Server.DTOs;

namespace CocktailDebacle.Server.Utils
{
    public class UtilsUserController
    {
        public static UserDto UserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                ImgProfileUrl = user.ImgProfileUrl ?? string.Empty,
                Followed_Users = user.Followed_Users.Select(u => u.UserName).ToList() ?? new List<string>(),
                Followers_Users = user.Followers_Users.Select(u => u.UserName).ToList() ?? new List<string>(),
            };
        }
    }
}