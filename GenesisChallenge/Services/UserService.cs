using GenesisChallenge.Helpers;
using GenesisChallenge.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace GenesisChallenge.Services
{
    public interface IUserService
    {
        User CreateUser(User user);
        Boolean EmailExists(string email);
        User Authenticate(string email, string password);
        User GetUser(Guid Id);
    }

    public class UserService : IUserService
    {
        private IGenesisChallengeContext _context;
        private IConfiguration _configuration { get; }

        public UserService(IGenesisChallengeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public User CreateUser(User user)
        {
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            
            user.Password = hasher.HashPassword(user, user.Password);
            user.CreatedOn = DateTime.UtcNow;
            user.LastUpdatedOn = DateTime.UtcNow;
            user.LastLoginOn = DateTime.UtcNow;
            _context.User.Add(user);
            _context.SaveChanges();
            
            user.Token = BuildToken(user.Id);
            _context.SaveChanges();

            // remove password before returning
            user.Password = null;
            return user;
        }

        public Boolean EmailExists(string email)
        {
            var user = _context.User.Where(x => x.Email == email).FirstOrDefault();
            if (user == null)
            {
                return false;
            }
            return true;
        }

        public User Authenticate(string email, string password)
        {
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            var user = _context.User.SingleOrDefault(x => x.Email == email);
            if (user == null)
                return null;

            var verifyResult = hasher.VerifyHashedPassword(user, user.Password, password);
            if (verifyResult != PasswordVerificationResult.Success)
                return null;
            
            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            user.Token = BuildToken(user.Id);
            user.LastLoginOn = DateTime.UtcNow;
            _context.SaveChanges();
            
            // remove password before returning
            user.Password = null;

            return user;
        }


        private string BuildToken(Guid Id)
        {
            var secret = _configuration.GetSection("AppSettings").GetValue<string>("Secret");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public User GetUser(Guid Id)
        {
            var user = _context.User.Where(x => x.Id == Id && x.LastLoginOn > DateTime.UtcNow.AddMinutes(-30)).SingleOrDefault();

            // return null if user not found
            if (user == null)
                return null;
            // remove password before returning
            user.Password = null;
            return user;
        }


    }
}
