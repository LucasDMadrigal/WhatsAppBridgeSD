using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhatsAppBridgeSD.src.Core.Models;

namespace WhatsAppBridgeSD.src.Core.Interfaces
{
    public interface IMessageService
    {
        Task<Message> SaveIncomingMessageAsync(Message msg);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(string toPhone, int limit = 100);
        Task MarkAsReadAsync(long id);
        Task MarkAsReadBulkAsync(IEnumerable<long> ids);
    }
}
