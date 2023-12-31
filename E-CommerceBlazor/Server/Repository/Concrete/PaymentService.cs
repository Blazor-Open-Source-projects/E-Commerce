﻿using E_CommerceBlazor.Shared.Model;
using System.Text.Json;
using StackExchange.Redis;
using Iyzipay;
using Iyzipay.Request;
using Iyzipay.Model;
using E_CommerceBlazor.Server.Repository.Abstract;
using E_CommerceBlazor.Server.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace E_CommerceBlazor.Server.Repository.Concrete
{
    public class PaymentService : IPaymentService
    {
        IDatabase _db;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBasketRepository _basketRepository;
        private readonly IOrderRepository _orderRepository;

        public PaymentService(IConnectionMultiplexer db, IConfiguration config, 
            IUserService userService, ApplicationDbContext context, IMapper mapper, IBasketRepository basketRepository,
            IOrderRepository orderRepository)
        {
            _db = db.GetDatabase();
            _config = config;
            _userService = userService;
            _context = context;
            _mapper = mapper;
            _basketRepository = basketRepository;
            _orderRepository = orderRepository;
        }
        public async Task<Response> CreatePaymentRequest(string basketId, Shared.Model.PaymentCard card)
        {
            var cardMapped = _mapper.Map<Iyzipay.Model.PaymentCard>(card);   
            var basketJson =await _db.StringGetAsync(basketId);
            var basket = JsonSerializer.Deserialize<Basket>(basketJson);
        
            if(basket == null)
            {
                throw new ArgumentNullException(nameof(basket));   
            }

            Options options = new Options();
            options.ApiKey = _config["iyzipay:ApiKey"];
            options.SecretKey = _config["iyzipay:SecretKey"];
            options.BaseUrl = _config["iyzipay:BaseUrl"];



            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Currency = Currency.TRY.ToString();
            request.PaidPrice = Convert.ToString(basket.TotalPrice);
            request.Price = Convert.ToString(basket.TotalPrice);
            request.BasketId = basketId;
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentCard = cardMapped;

            var userId = _userService.GetUserId();
            var user =_context.Users.Where(opt => opt.Id == userId).Include(opt => opt.Address).FirstOrDefault();

            if(user == null)
            {
                return new Response
                {
                    Message = "You should login to be able to make payment",
                    Success = false
                };
            }

            var mappedAddresss = await AddressOperationForPayment(user.Address);
            var basketItem = await BasketItemForPayment(basketId);
            var buyer = await BuyerOperationForPayment(user);

            var addressData = mappedAddresss.Data;
            var basketData = basketItem.Data;
            var buyerData = buyer.Data;

            request.BillingAddress = addressData;
            request.BasketItems = basketData;
            request.ShippingAddress = addressData;
            request.Buyer = buyerData;
            
              
            Payment payment = Payment.Create(request, options);

            var createOrder = await _orderRepository.CreateOrder(basketId);

            if(createOrder.Success == false )
            {
                return new Response
                {
                    Message = "Beklenmedik bir hatayla karşılaşıldı, lütfen tekrar deneyiniz",
                    Success = false
                };
            }
            return new Response 
            {
                Message = payment.PaymentStatus,
                Success = true
            };

        }

        public async Task<DataResponse<List<Iyzipay.Model.BasketItem>>> BasketItemForPayment(string basketId)
        {
            List<Iyzipay.Model.BasketItem> basketItems = new List<Iyzipay.Model.BasketItem>();
            
            var basketJson = await _db.StringGetAsync(basketId);
            var basket = JsonSerializer.Deserialize<Basket>(basketJson);

           foreach(var basketItem in basket.Items)
           {
                Iyzipay.Model.BasketItem basketItem1 = new Iyzipay.Model.BasketItem
                {
                    Id = basketItem.Id,
                    Name = basketItem.Name,
                    Price = Convert.ToString(basketItem.TotalItemPrice),
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Category1 = "category"
                    
                };
                basketItems.Add(basketItem1);
           }
           if(basketItems != null)
           {
                return new DataResponse<List<Iyzipay.Model.BasketItem>>
                {
                    Data = basketItems,
                    Message = "iyzipay Basket Item list getted",
                    Success = true
                };
           }
           else
           {
                return new DataResponse<List<Iyzipay.Model.BasketItem>>
                {
                    Data = null,
                    Message = "An arror occured in BasketItemPayment service",
                    Success = false
                };

           }
           
        }

        public async Task<DataResponse<Iyzipay.Model.Address>> AddressOperationForPayment(Shared.Model.Address address)
        {
            Iyzipay.Model.Address addressMaping = new Iyzipay.Model.Address
            {
                ContactName = address.ContactName,
                City = address.City,
                Country = address.Country,
                Description = address.Description,
                ZipCode = address.ZipCode
            };

            if(addressMaping == null)
            {
                return new DataResponse<Iyzipay.Model.Address>
                {
                    Data = null,
                    Message = "An error occured in AddressOperationForPayment method check there you should add an addres before you make the payment",
                    Success = false
                };
            }
            
            return new DataResponse<Iyzipay.Model.Address>
            {
                Data = addressMaping,
                Message = "Addres is mapped successfully for payment",
                Success = true
            };
            

        }

        public async Task<DataResponse<Buyer>> BuyerOperationForPayment(User user)
        {

            Buyer buyer = new Buyer()
            {
                Id = Convert.ToString(user.Id),
                Name = user.FirstName,
                Surname = user.LastName,
                Email = user.Email,
                IdentityNumber = Guid.NewGuid().ToString(),
                RegistrationAddress = "12345",
                City = user.Address.City,
                Country = user.Address.Country,
                ZipCode = user.Address.ZipCode,
             
            };
            if(buyer == null)
            {
                return new DataResponse<Buyer>
                {
                    Data =null,
                    Success = false,
                    Message = "in BuyerOperationForPayment an error occured"
                };
            }
            return new DataResponse<Buyer>
            {
                Data = buyer,
                Success = true,
                Message = "BuyerOperationForPayment succeded"
            };
        }
    }
}
