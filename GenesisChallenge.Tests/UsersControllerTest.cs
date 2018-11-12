using GenesisChallenge.Controllers;
using GenesisChallenge.Helpers;
using GenesisChallenge.Models;
using GenesisChallenge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using Xunit;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;

namespace GenesisChallenge.Tests
{
    public class UsersControllerTest
    {

        UsersController _controller;
        IUserService _service;
        IGenesisChallengeContext _context;

        public UsersControllerTest()
        {
            //IOptions<AppSettings> appSettings = new IOptions<AppSettings>();
            var options = new DbContextOptionsBuilder<GenesisChallengeContext>()
                .UseInMemoryDatabase(databaseName: "UsersControllerTest")
                .Options;
            //;
            _context = new GenesisChallengeContext(options);

            _service = new UserService(_context, GetIConfigurationRoot());
            _controller = new UsersController(_service);

            User user = new User();
            user.Name = "Stuart";
            user.Email = "existinguser@gmail.com";
            user.Password = "password";

            var result = _controller.SignUp(user) as OkObjectResult;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json",
                         optional: true, reloadOnChange: true)
                .Build();
        }



        [Fact]
        public void SignUp_ShouldReturnUser()
        {
            User user = new User();
            user.Name = "Stuart";
            user.Email = "bigfathorse@gmail.com";
            user.Password = "password";

            var result = _controller.SignUp(user) as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(result);
            Assert.IsType<User>(result.Value);
            var resultUser = result.Value as User;
            Assert.IsType<Guid>(resultUser.Id);
            Assert.IsType<DateTime>(resultUser.CreatedOn);
            Assert.IsType<DateTime>(resultUser.LastUpdatedOn);
            Assert.IsType<DateTime>(resultUser.LastLoginOn);
            Assert.IsType<String>(resultUser.Token);
        }

        [Fact]
        public void SignUp_ShouldReturnEmailExistsError()
        {
            User user = new User();
            user.Name = "Stuart";
            user.Email = "existinguser@gmail.com";
            user.Password = "password";

            var result = _controller.SignUp(user) as ConflictObjectResult;
            Assert.IsType<ConflictObjectResult>(result);
       
        }

        [Fact]
        public void SignIn_ShouldReturnUser()
        {
            SignIn signIn = new SignIn();
            signIn.Email = "existinguser@gmail.com";
            signIn.Password = "password";

            var result = _controller.SignIn(signIn) as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void SignIn_ShouldReturn401()
        {
            SignIn signIn = new SignIn();
            signIn.Email = "existinguser@gmail.com";
            signIn.Password = "wrongpassword";

            var result = _controller.SignIn(signIn) as ObjectResult;
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void SearchUser_ShouldReturnUser()
        {
            //get user id
            var user = _context.User.Where(x => x.Email == "existinguser@gmail.com").SingleOrDefault();
            //mock ClaimsPrincipal
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity( new List<Claim>(new Claim[] { new Claim(ClaimTypes.Name.ToString(), user.Id.ToString()) } ))));
            _controller.ControllerContext.HttpContext = contextMock.Object;

            var result = _controller.Search(user.Id) as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void SearchUser_ShouldReturn401()
        {
            //update users last login date to beyond 30 mins
            var user = _context.User.Where(x => x.Email == "existinguser@gmail.com").SingleOrDefault();
            user.LastLoginOn = DateTime.UtcNow.AddMinutes(-31);
            _context.SaveChanges();
            //mock ClaimsPrincipal
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(new Claim[] { new Claim(ClaimTypes.Name.ToString(), user.Id.ToString()) }))));
            _controller.ControllerContext.HttpContext = contextMock.Object;

            var result = _controller.Search(user.Id) as ObjectResult;
            Assert.Equal(401, result.StatusCode);
        }
    }
}
