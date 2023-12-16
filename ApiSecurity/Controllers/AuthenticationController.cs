﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiSecurity.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private IConfiguration _config;

    public AuthenticationController(IConfiguration config)
    {
        _config = config;
    }
    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int UserId, string UserName);

    // api/Authentication/token
    [HttpPost("token")]
    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        var user = ValidateCredentials(data);

        if (user is null)
        {
            return Unauthorized();
        }

        var token =GenerateToken(user);

        return Ok(token);
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(
                                        Encoding.ASCII.GetBytes(
                                            _config.GetValue<string>("Authentication:Secretkey")));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new();

        claims.Add(new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.UniqueName, user.UserName));

        var token = new JwtSecurityToken(
                                                       _config.GetValue<string>("Authentication:Issuer"),
                                                       _config.GetValue<string>("Authentication:Audience"),
                                                       claims,
                                                       DateTime.UtcNow, //when this token becomes valid
                                                       DateTime.UtcNow.AddMinutes(1), // when the token will expire
                                                       signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(token);                             

    }


    private UserData? ValidateCredentials(AuthenticationData data)
    {
        //THIS IS NOT PRODUCTION CODE- 
        //THIS IS ONLY A DEMO
        // DO NOT USE IN REAL LIFE

        if (CompareValue(data.UserName, "Micheal") && CompareValue(data.Password, "Shodamola"))
        {
            return new UserData(1, data.UserName!);
        }

        if (CompareValue(data.UserName, "Mike") && CompareValue(data.Password, "Shodamola"))
        {
            return new UserData(1, data.UserName!);
        }

        return null;
    }

    private bool CompareValue(string? actual, string expected)
    {
        if (actual is not null)
        {
            // TO IGNORE CASE (actual.Equals(expected, StringComparison.InvariantCultureIgnoreCase))
            if (actual.Equals(expected))
            {
                return true;
            }
        }

        return false;
    }
}