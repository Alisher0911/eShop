using System;
using AutoMapper;
using EventBusRabbitMQ.Events;
using Ordering.API.DTO;
using Ordering.Core.Entities;

namespace Ordering.API.Mapping
{
    public class OrderMapping : Profile
    {
        public OrderMapping()
        {
            CreateMap<Order, OrderResponse>();
            CreateMap<BasketCheckoutEvent, Order>();
        }
    }
}
