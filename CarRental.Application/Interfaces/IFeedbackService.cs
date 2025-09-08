using CarRental.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs.Feedback;

namespace CarRental.Application.Interfaces
{
    public interface IFeedbackService
    {
        Task AddFeedbackAsync(CreateFeedbackDto dto);
        
        /// <summary>
        /// Retrieves a feedback report for the given user context.
        /// </summary>
        /// <param name="currentUserId">ID of the current logged-in user.</param>
        /// <param name="isAdmin">Whether the user is an Administrator.</param>
        /// <param name="request">Report request filters and paging.</param>
        Task<FeedbackReportDto> GetReportAsync(Guid currentUserId, bool isAdmin, FeedbackReportRequestDto request);
    }
}
