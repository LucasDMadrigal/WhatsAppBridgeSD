using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhatsAppBridgeSD.src.Core.Interfaces;
using WhatsAppBridgeSD.src.Core.Models;
using WhatsAppBridgeSD.src.SignalR;

namespace WhatsAppBridgeSD.src.Api.Controllers
    
{[Route("webhook")]
    [ApiController]
    public class WhatsAppWebhookController: ControllerBase
    {
        private readonly IConfiguration _cfg;
        private readonly IMessageService _svc;
        private readonly IHubContext<ChatHub>? _hub;
        
        public WhatsAppWebhookController(IConfiguration cfg, IMessageService svc, IHubContext<ChatHub>? hub = null)
        {
            _cfg = cfg; _svc = svc; _hub = hub;
        }
        
        // Verificación inicial (Meta)
        [HttpGet]
        public IActionResult Verify([FromQuery(Name = "hub.mode")] string mode,
                                    [FromQuery(Name = "hub.verify_token")] string token,
                                    [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var expected = _cfg["WhatsApp:VerifyToken"];
            if (mode == "subscribe" && token == expected) return Ok(challenge);
            return Forbid();
        }
        
        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            Request.EnableBuffering();
            string body = "";
            using (var sr = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                body = await sr.ReadToEndAsync();
            Request.Body.Position = 0;
            
            // verificar firma
            var header = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var secret = _cfg["WhatsApp:AppSecret"];
            if (!VerifySignature(header, body, secret)) return Unauthorized();

            // parseo simple (depende del payload real de WhatsApp)
            // ejemplo: iterar entries -> changes -> value.messages
            var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("entry", out var entries))
            {
                foreach (var e in entries.EnumerateArray())
                {
                    if (e.TryGetProperty("changes", out var changes))
                    {
                        foreach (var c in changes.EnumerateArray())
                        {
                            if (c.TryGetProperty("value", out var value))
                            {
                                if (value.TryGetProperty("messages", out var messages))
                                {
                                    foreach (var m in messages.EnumerateArray())
                                    {
                                        var waId = m.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                                        var from = m.GetProperty("from").GetString() ?? "";
                                        var type = m.GetProperty("type").GetString() ?? "unknown";
                                        string text = "";
                                        if (type == "text" && m.TryGetProperty("text", out var textObj))
                                            text = textObj.GetProperty("body").GetString() ?? "";

                                        var msg = new Message
                                        {
                                            WhatsAppMessageId = waId,
                                            FromPhone = from,
                                            ToPhone = value.GetProperty("metadata").GetProperty("phone_number_id").GetString() ?? "",
                                            Type = type,
                                            Body = text,
                                            ReceivedAt = DateTimeOffset.UtcNow,
                                            Status = "received"
                                        };

                                        var saved = await _svc.SaveIncomingMessageAsync(msg);
                                        // notificar via SignalR si está configurado
                                        if (_hub != null) await _hub.Clients.All.SendAsync("ReceiveMessage", saved);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Ok();
        }

        private bool VerifySignature(string header, string payload, string secret)
        {
            if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(secret)) return false;
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var hex = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return string.Equals(hex, header, StringComparison.OrdinalIgnoreCase);
        }
    }
}
