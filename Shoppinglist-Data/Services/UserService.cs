using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Digitalist.Helpers;
using Digitalist_Data.Helpers;
using Digitalist_Data.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Digitalist_Data.Services
{
    public interface IUserService
    {
        IResult<object> AddToDb(User user);
        IResult<string> Authenticate(string username, string password);
        IResult<User> GetUserForUsername(string username);
    }
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IResult<object> AddToDb(User user)
        {
            using var context = new ListContext();
            try
            {
                context.Users.Add(user);
                context.SaveChanges();
                return new Result($"User {user.Username} successfully added", ResultType.Success);
            }
            catch (Exception ex)
            {
                return new Result($"The User could not be added because of an error: \n {ex.Message}", ResultType.Error);
            }
        }

        public IResult<string> Authenticate(string username, string password)
        {
            var result = GetUserForUsername(username);
            if (result.Type == ResultType.Success && result.ReturnObj != null)
            {
                var user = result.ReturnObj;
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.Username)
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return new Result<string>("Token successfully created", ResultType.Success,
                        tokenHandler.WriteToken(token));
                }
                return new Result<string>("Incorrect Password or Username", ResultType.Error);
                
            }
            return new Result<string>(result.Message, ResultType.Error);
        }

        public IResult<User> GetUserForUsername(string username)
        {
            using var context = new ListContext();
            try
            {
                var user = context.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    return new Result<User>($"No User with username: {username} found", ResultType.Error);
                }
                return new Result<User>($"User {username} found", ResultType.Success, user);
            }
            catch (Exception ex)
            {
                return new Result<User>(ex.Message, ResultType.Error);
            }
        }
    }
}
