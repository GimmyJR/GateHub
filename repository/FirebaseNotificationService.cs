using FirebaseAdmin.Messaging;
using GateHub.Models;
using Notification = FirebaseAdmin.Messaging.Notification;

namespace GateHub.repository
{
    public class FirebaseNotificationService
    {
        private readonly GateHubContext context;

        public FirebaseNotificationService(GateHubContext context)
        {
            this.context = context;
        }
        public async Task<string> SendNotificationAsync(string title, string body, string deviceToken)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                    
                },
                Token = deviceToken
            };

            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        public async Task StoreNotification(string userId, string title, string body)
        {
            await context.Notifications.AddAsync(new Models.Notification
            {
                UserId = userId,
                Title = title,
                Body = body
            });
            await context.SaveChangesAsync();
        }
    }
}
