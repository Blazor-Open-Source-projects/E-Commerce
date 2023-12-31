﻿
using Microsoft.AspNetCore.Mvc;
using E_CommerceBlazor.Shared.Dto;
using E_CommerceBlazor.Server.Repository.Abstract;

namespace E_CommerceBlazor.Server.Controllerss
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController :ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }   

        [HttpPost("/register")]
        public async Task<ActionResult<DataResponse<string>>> Register(RegisterDto register)
        {
            var response = await _authService.Register(register);
             
            return Ok(response);
        }
        [HttpPost("/login")]
        public async Task<ActionResult<DataResponse<string>>> login(LoginDto login)
        {
            var response = await _authService.Login(login);
           
            return Ok(response);
        }
    }
}
