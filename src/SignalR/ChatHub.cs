using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WhatsAppBridgeSD.src.SignalR
{
    public class ChatHub : Hub
    {
        public async Task Join(string room) => await Groups.AddToGroupAsync(Context.ConnectionId, room);
        public async Task Leave(string room) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

    }
}
