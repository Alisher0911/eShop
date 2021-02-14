using System;
using System.Threading.Tasks;
using Basket.API.Data.Interfaces;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Basket.API.Repositories
{
    public class BasketCartRepository : IBasketCartRepository
    {
        private readonly IBasketContext _context;

        public BasketCartRepository(IBasketContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }



        public async Task<BasketCart> GetBasketCart(string username)
        {
            var basket = await _context.Redis.StringGetAsync(username);
            if (basket.IsNullOrEmpty)
            {
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<BasketCart>(basket);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<BasketCart> UpdateBasketCart(BasketCart basket)
        {
            var updated = await _context.Redis.StringSetAsync(basket.Username, JsonConvert.SerializeObject(basket));
            if (!updated)
            {
                return null;
            }

            return await GetBasketCart(basket.Username);
        }


        public async Task<bool> DeleteBasketCart(string username)
        {
            return await _context.Redis.KeyDeleteAsync(username);
        }
    }
}
