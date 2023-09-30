﻿using AutoMapper;
using E_CommerceBlazor.Server.Repossitory.Abstract;
using E_CommerceBlazor.Shared.Dto;
using E_CommerceBlazor.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceBlazor.Server.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AddressController: ControllerBase
    {
        private readonly IAddressRepository _addressRepository;

        public AddressController(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
           
        }

        [HttpPost]
        public async Task<ActionResult> CreateAddress(AddressDTO addressDTO)
        {
          
            var result = await _addressRepository.CreateAddress(addressDTO);
            if(result.Success == false)
            {
                return BadRequest();
            }
            return Ok(result);
        }
    }
}