using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WhatsAppBridgeSD.src.Core.Interfaces;
using WhatsAppBridgeSD.src.Core.Models;
using WhatsAppBridgeSD.src.Infrastructure.Data;

namespace WhatsAppBridgeSD.src.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _db;
        public MessageService(AppDbContext db) { _db = db; }
        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string toPhone, int limit = 100)
        {
            return await _db.Messages
                .Where(m => m.ToPhone == toPhone && m.Status == "received")
                .OrderBy(m => m.ReceivedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(long id)
        {
            var m = await _db.Messages.FindAsync(id);
            if (m == null) return;
            m.Status = "acknowledged";
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsReadBulkAsync(IEnumerable<long> ids)
        {
            var list = await _db.Messages.Where(m => ids.Contains(m.Id)).ToListAsync();
            foreach (var m in list) m.Status = "acknowledged";
            await _db.SaveChangesAsync();
        }

        public async Task<Message> SaveIncomingMessageAsync(Message msg)
        {
            // prevención de duplicados por WhatsAppMessageId
            var exists = await _db.Messages
                .FirstOrDefaultAsync(m => m.WhatsAppMessageId == msg.WhatsAppMessageId);
            if (exists != null) return exists;

            msg.ReceivedAt = msg.ReceivedAt == default ? DateTimeOffset.UtcNow : msg.ReceivedAt;
            msg.Status = "received";
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();
            return msg;
        }
    }
}
