using Microsoft.AspNetCore.SignalR;

namespace LiveShoppingCart_RealTime.Hubs
{
    public class ChatHub : Hub
    {
        /*
        //Called when a user sends a message about poduct
        public async Task SendMessage(string productId, string user, string msg)
        {
            await Clients.Group(productId).SendAsync("ReceiveMessage", user, msg);
        }

        //when user joins a product chat room
        public async Task JoinProductGroup(string productId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, productId);
            await Clients.Group(productId).SendAsync("UserJoined", $"{Context.ConnectionId} has joined the chat for product {productId}.");
        }

        //when user leaves a product chat room
        public async Task LeaveProductGroup(string productId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, productId);
            await Clients.Group(productId).SendAsync("UserLeft", $"{Context.ConnectionId} has left the chat for product {productId}.");
        }
        */
    }
}
