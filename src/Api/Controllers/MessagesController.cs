using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WhatsAppBridgeSD.src.Core.Interfaces;

namespace WhatsAppBridgeSD.src.Api.Controllers
{
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _svc;
        public MessagesController(IMessageService svc) { _svc = svc; }
        
        // GET api/messages/unread?toPhone=123
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread([FromQuery] string toPhone)
        {
            var list = await _svc.GetUnreadMessagesAsync(toPhone);
            return Ok(list);
        }

        // POST api/messages/ack
        [HttpPost("ack")]
        public async Task<IActionResult> Ack([FromBody] long[] ids)
        {
            await _svc.MarkAsReadBulkAsync(ids);
            return Ok();
        }
    }
}
