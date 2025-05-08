using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.SignalR.Protocol;
using RealEstateApi.DOTs;

namespace RealEstateApi.Models
{
    public class FirebaseService
    {
        private readonly FirebaseApp? _app;

        public FirebaseService(IWebHostEnvironment env)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                _app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(Path.Combine(env.ContentRootPath, "realestateapp-b7468-firebase-adminsdk-fbsvc-a0202bfb86.json"))
                });
            }
        }

        public async Task<string> SendNotification(string deviceToken, string title, string body, string imageUrl)
        {
            var message = new Message()
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = imageUrl
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }

        public async Task SendMessageAsync(PushRequest message)
        {
            var msg = new Message()
            {
                Token = message.token,
                Notification = new Notification
                {
                    Title = message.title,
                    Body = message.body
                },
                //Data = message.Data
            };

            if (!string.IsNullOrEmpty(message.imageUrl))
            {
                msg.Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        ImageUrl = message.imageUrl
                    }
                };

                msg.Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        MutableContent = true
                    },
                    FcmOptions = new ApnsFcmOptions
                    {
                        ImageUrl = message.imageUrl
                    }
                };
            }

            await FirebaseMessaging.DefaultInstance.SendAsync(msg);
        }

        public async Task SendMulticastNotificationAsync(List<string> fcmTokens, string title, string body, string imageUrl, RealEstateNotificationDto estate)
        {
            var multicastMessage = new MulticastMessage
            {
                Tokens = fcmTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                    //ImageUrl = imageUrl
                },
                Data = new Dictionary<string, string>
                {
                    { "realEstateId", estate.realEstateId.ToString() }
                },
                Android = new AndroidConfig
                {
                    Notification = new AndroidNotification()
                    //{
                    //    ImageUrl = imageUrl
                    //}
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        MutableContent = true
                    },
                    FcmOptions = new ApnsFcmOptions()
                    //{
                    //    ImageUrl = imageUrl
                    //}
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage);

                Console.WriteLine($"[FCM] Sent to {response.Responses.Count(r => r.IsSuccess)} devices. Failed: {response.Responses.Count(r => !r.IsSuccess)}");
                //_logger.LogInformation($"Đã gửi thông báo đến {response.SuccessCount} user, lỗi: {response.FailureCount}");

                //foreach (var (res, index) in response.Responses.Select((res, idx) => (res, idx)))
                //{
                //    if (!res.IsSuccess)
                //    {
                //        Console.WriteLine($"[FCM] Failed to send to token {fcmTokens[index]}: {res.Exception?.Message}");
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] Error sending multicast: {ex.Message}");
            }
        }
    }

    public class PushRequest
    {
        public string token { get; set; } = "";
        public string title { get; set; } = "";
        public string body { get; set; } = "";
        public string imageUrl { get; set; } = "";
    }
}
