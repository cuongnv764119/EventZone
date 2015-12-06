using System;
using System.Web;
using Microsoft.AspNet.SignalR;
namespace EventZone.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string data)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.broadcastMessage(data);
        }
    }
}