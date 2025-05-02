using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class OrderHub : Hub
    {
        public async Task SendOrderUpdate(string orderId, string status)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", orderId, status);
        }
    }
}
