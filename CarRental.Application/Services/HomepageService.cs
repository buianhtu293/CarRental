using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using CarRental.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.Services
{
    public class HomepageService:IHomepageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomepageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<FeedbackDto>> GetTopFeedbacksAsync()
        {
            var feedbacks = await _unitOfWork.Homepages.GetTopFeedbacksAsync(4);

            return feedbacks.Select(f => new FeedbackDto
            {
                UserName = f.User.UserName,
                Comment = f.Comment,
                Rating = f.Stars,
                FeedbackDate = f.Booking.ReturnDate
            }).ToList();
        }

        public async Task<List<CitySummaryDto>> GetTopCitiesAsync()
        {
            var cityData = await _unitOfWork.Homepages.GetTopProvincesWithMostCarsAsync(6);

            return cityData.Select(tuple => new CitySummaryDto
            {
                ProvinceName = tuple.Province,
                CarCount = tuple.CarCount
            }).ToList();
        }

    }
}
