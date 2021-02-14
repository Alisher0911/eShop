using System;
using System.Threading.Tasks;
using Basket.API.Entities;

namespace Basket.API.Repositories.Interfaces
{
    public interface IBasketCartRepository
    {
        Task<BasketCart> GetBasketCart(string name);
        Task<BasketCart> UpdateBasketCart(BasketCart basket);
        Task<bool> DeleteBasketCart(string name);
    }
}
