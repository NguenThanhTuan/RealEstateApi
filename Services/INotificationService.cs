using System;
using System.Threading.Tasks;
using System.Xml;
using RealEstateApi.DOTs;
using RealEstateApi.Models;

public interface INotificationService
{
    Task CreateAndSendNotificationAsync(int type, RealEstateNotificationDto estate, int currentUserId);
}
