using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TodoApi.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration config;

    public AuthenticationController(IConfiguration config)
    {
        this.config = config;
    }

    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int Id, string FirstName, string LastName, string UserName);

    /// <summary>
    /// Authenticates the user and generates a JWT token.
    /// </summary>
    /// <param name="data">The authentication data containing username and password.</param>
    /// <returns>An ActionResult containing the JWT token or an unauthorized response.</returns>
    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        var user = ValidateCredentials(data);
        if (user == null)
        {
            return Unauthorized("Invalid Username or Password!");
        }

        string token = GenerateToken(user);
        return Ok(token);
    }

    /// <summary>
    /// Generates a JWT token for the authenticated user.
    /// </summary>
    /// <param name="user">User data for which the token is to be generated.</param>
    /// <returns>A JWT token string.</returns>
    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            config.GetValue<string>("Authentication:SecretKey")));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        // .Sub claim type is equal to NameIdentifier type
        List<Claim> claims = [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName)];

        var token = new JwtSecurityToken(
            config.GetValue<string>("Authentication:Issuer"),
            config.GetValue<string>("Authentication:Audience"),
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates the user credentials.
    /// </summary>
    /// <param name="data">Authentication data containing username and password.</param>
    /// <returns>The UserData if credentials are valid, null otherwise.</returns>
    /// <remarks>This method currently uses hardcoded credentials. Replace with actual authentication logic.</remarks>
    private UserData? ValidateCredentials(AuthenticationData data)
    {
        // This is not production code - replace this with a call to your auth system.
        if (CompareValues(data.UserName, "Test1") &&
            CompareValues(data.Password, "Test1"))
        {
            return new UserData(1, "FName1", "LName1", data.UserName!);
        }
        if (CompareValues(data.UserName, "Test2") &&
            CompareValues(data.Password, "Test2"))
        {
            return new UserData(1, "FName2", "LName2", data.UserName!);
        }

        return null;
    }

    /// <summary>
    /// Compares a given value with an expected one.
    /// </summary>
    /// <param name="actual">The actual value to compare.</param>
    /// <param name="expected">The expected value to compare against.</param>
    /// <returns>True if values are equal, false otherwise.</returns>
    private bool CompareValues(string? actual, string expected)
    {
        if (actual is not null)
        {
            if (actual.Equals(expected))
            {
                return true;
            }
        }
        return false;
    }
}
