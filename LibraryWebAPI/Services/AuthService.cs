using AutoMapper;
using LibraryWebAPI.Data;
using LibraryShared.DTOs;
using LibraryWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryWebAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(UserRegisterDTO userDto);
        Task<AuthResponseDTO> LoginAsync(UserLoginDTO userDto);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration, IMapper mapper)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<AuthResponseDTO> RegisterAsync(UserRegisterDTO userDto)
        {
            var user = new AppUser
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                FullName = userDto.FullName
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"User creation failed: {errors}");
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            return await GenerateJwtToken(user);
        }

        public async Task<AuthResponseDTO> LoginAsync(UserLoginDTO userDto)
        {
            var user = await _userManager.FindByNameAsync(userDto.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, userDto.Password))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            return await GenerateJwtToken(user);
        }

        private async Task<AuthResponseDTO> GenerateJwtToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expires,
                Username = user.UserName,
                FullName = user.FullName
            };
        }
    }
}