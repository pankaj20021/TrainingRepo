using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.DTOs.UserDtos;
using SchoolProject.JWToken;
using SchoolProject.Model;

namespace SchoolProject.Repository
{
    public class UserRepository
    {    
            private readonly UserManager<UserApp> _userManager;
            private readonly SignInManager<UserApp> _signInManager;
            public readonly RoleManager<IdentityRole> _roleManager;
            private readonly JwtTokenGenerator _tokenGenerator;
            public UserRepository(UserManager<UserApp> userManager,
                                  SignInManager<UserApp> signInManager,
                                  RoleManager<IdentityRole> roleManager,
                                  JwtTokenGenerator tokenGenerator)
            { 
                _userManager = userManager;
                _signInManager = signInManager;
               _roleManager = roleManager;
            _tokenGenerator = tokenGenerator;
            }

        public async Task<(bool Success, string Message)> RegisterAsync([FromForm] UserRegisterDTos registerDto)
        {
            if (string.IsNullOrWhiteSpace(registerDto.FullName))
                return (false, "FullName is required");

            if (string.IsNullOrWhiteSpace(registerDto.Email))
                return (false, "Email is required");

            if (string.IsNullOrWhiteSpace(registerDto.Password))
                return (false, "Password is required.");
            var user = new UserApp
            {
                FullName = registerDto.FullName,  
                UserName = registerDto.Email,     
                Email = registerDto.Email,
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            //if (!result.Succeeded)
            //{
            //    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            //}

            // Assign role only if Role is not None
            if (registerDto.Role != Role.None)
            {
                var roleName = registerDto.Role.ToString();

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return (false, $"Role '{roleName}' does not exist.");
                }
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    return (false, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            return (true, "User registered successfully and role assigned.");
        }

        public async Task<(bool Success, string TokenOrMessage)> LoginAsync(UserLogInDTos loginDto)
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                    return (false, "Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault(); // Assume one role per user. If multiple, adjust token generator.

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                    return (false, "Invalid email or password");

                var token = _tokenGenerator.GenerateToken(user, role);
                return (true, token);
            }

        }
    }
