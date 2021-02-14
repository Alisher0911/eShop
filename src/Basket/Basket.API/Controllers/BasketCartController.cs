using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using EventBusRabbitMQ.Common;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Producers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    public class BasketCartController : ControllerBase
    {
        private readonly IBasketCartRepository _repository;
        private readonly ILogger<BasketCartController> _logger;
        private readonly EventBusRabbitMQProducer _eventBus;
        private readonly IMapper _mapper;

        public BasketCartController(IBasketCartRepository repository, EventBusRabbitMQProducer eventBus, IMapper mapper, ILogger<BasketCartController> logger)
        {
            _repository = repository;
            _eventBus = eventBus;
            _mapper = mapper;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> GetBasket(string username)
        {
            var basket = await _repository.GetBasketCart(username);
            return Ok(basket ?? new BasketCart(username));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> UpdateBasket([FromBody] BasketCart basket)
        {
            var updatedBasket = await _repository.UpdateBasketCart(basket);
            return Ok(updatedBasket);
        }

        [HttpDelete("{name}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> DeleteBasket(string username)
        {
            return Ok(await _repository.DeleteBasketCart(username));
        }


        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            var basket = await _repository.GetBasketCart(basketCheckout.Username);
            if(basket == null)
            {
                _logger.LogError("Basket not found for user: " + basketCheckout.Username);
                return BadRequest();
            }

            var removed = await _repository.DeleteBasketCart(basketCheckout.Username);
            if (!removed)
            {
                _logger.LogError("Basket not removed for user: " + basketCheckout.Username);
                return BadRequest();
            }
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.RequestId = Guid.NewGuid();

            try
            {
                _eventBus.PublishBasketCheckout(EventBusConstants.BasketCheckoutQueue, eventMessage);
            } catch(Exception e)
            {
                _logger.LogError("Error: Message not published for user: " + basketCheckout.Username + ", " + e.Message);
                throw;
            }

            return Accepted();
        }
    }
}
