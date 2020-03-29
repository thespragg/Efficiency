using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using server.Helpers;
using server.Models;
using server.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserService _userService;
        private IConfiguration _config;

        public AccountController(UserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpPost("Login")]
        public string Login(UserLogin user)
        {
            var storedUser = _userService.Find(user.Username);
            if(storedUser == null)
            {
                //Need to add a time delay here to simulate password checking
                return "";
            }

            if(VerifyHash(user.Password, storedUser.Hash))
            {
                var jwt = new JwtProvider(_config);
                var token = jwt.GenerateSecurityToken(user.Username);
                return token;
            }
            return "";
        }

        [HttpPost("Register")]
        public bool Register(NewUser user)
        {
            var hash = HashPassword(user.Password);

            //Check email isnt taken
            var newUser = new User()
            {
                Username = user.Email,
                Hash = hash,
                Created = DateTime.Now,
                Name = user.Name,
            };

            _userService.Create(newUser);
            return true;
        }

        private string HashPassword(string password)
        {
            return PasswordHasher.Hash(password, 4);
        }

        private bool VerifyHash(string password, string hash)
        {
            return PasswordHasher.Verify(password, hash);
        }
    }
}