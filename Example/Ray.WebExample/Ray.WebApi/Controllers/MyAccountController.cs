using System;
using Ray.WebApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Ray.Core.Client;
using Ray.IGrains.Actors;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ray.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "SuperAdminOnly")]
    public class MyAccountController : Controller
    {
        private IClientFactory clientFactory;
        public MyAccountController(IClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }
        /// <summary>
        /// 获取余额
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<decimal> GetBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = clientFactory.Create();
            return await client.GetGrain<IAccount>(Int64.Parse(userId)).GetBalance();
        }
        /// <summary>
        /// 增加余额
        /// </summary>
        /// <remarks>
        /// Sample Request
        ///
        ///     POST /api/myaccount
        ///     {
        ///       "amount":100     
        ///     }
        /// </remarks>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> AddAmount([FromBody]AddAmountViewModel amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = clientFactory.Create();
            return await client.GetGrain<IAccount>(Int64.Parse(userId)).AddAmount(amount.Amount);
        }
        /// <summary>
        /// 转账
        /// </summary>
        /// <remarks>
        /// Sample Request
        /// 
        ///     POST /api/myaccount
        ///     {
        ///      "toAccountId":0
        ///      "amount":100
        ///     }
        /// </remarks>
        /// <param name="transfer"></param>
        /// <returns></returns>
        [Route("Transfer")]
        [HttpPost]
        public async Task Transfer([FromBody]TransferViewModel transfer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = clientFactory.Create();
            await client.GetGrain<IAccount>(Int64.Parse(userId)).Transfer(transfer.ToAccountId, transfer.Amount);
        }
    }
}
