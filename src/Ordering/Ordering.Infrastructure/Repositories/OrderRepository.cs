using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Repositories.Base;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(OrderContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Order>> GetOrderByUsername(string username)
        {
            return (IEnumerable<Order>)await _dbContext.Orders.FindAsync(username);
        }
    }
}
