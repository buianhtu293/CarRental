using CarRental.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.Interfaces
{
    public interface IHomepageService
    {
        Task<List<FeedbackDto>> GetTopFeedbacksAsync();
        Task<List<CitySummaryDto>> GetTopCitiesAsync();
    }
}
