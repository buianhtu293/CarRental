using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs.Feedback;
using CarRental.Domain.Models.Feedback;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Repositories;

namespace CarRental.Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedbackService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddFeedbackAsync(CreateFeedbackDto dto)
        {
            var feedback = new Feedback
            {
                UserID = dto.UserID,
                CarID = dto.CarID,
                BookingID = dto.BookingID,
                Stars = dto.Stars,
                Comment = dto.Comment
            };

            await _unitOfWork.Feedbacks.AddAsync(feedback);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<FeedbackReportDto> GetReportAsync(Guid currentUserId, bool isAdmin, FeedbackReportRequestDto request)
        {
            // Translate application request DTO to domain filter
            var filter = new FeedbackReportFilter
            {
                CarId = request.CarId,
                MinStars = request.MinStars,
                MaxStars = request.MaxStars,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Keyword = request.Keyword
            };

            var scopeToOwner = !isAdmin;
            var ownerId = scopeToOwner ? currentUserId : (Guid?)null;

            // Setup background worker DbContext for read-only actions
            var secondFeedBackRepoWrapperTask = _unitOfWork.CreateExtraRepository(context => new FeedbackRepository((CarRentalDbContext)context));
            var thirdFeedBackRepoWrapperTask =  _unitOfWork.CreateExtraRepository(context => new FeedbackRepository((CarRentalDbContext)context));
            await Task.WhenAll(secondFeedBackRepoWrapperTask, thirdFeedBackRepoWrapperTask);
            
            var secondFeedBackRepoWrapper = await secondFeedBackRepoWrapperTask;
            await using var extraContext = secondFeedBackRepoWrapper.ExtraContext;
            var secondFeedBackRepo = secondFeedBackRepoWrapper.ExtraRepo;
            
            var thirdFeedBackRepoWrapper = await thirdFeedBackRepoWrapperTask;
            await using var thirdContext = thirdFeedBackRepoWrapper.ExtraContext;
            var thirdFeedBackRepo = thirdFeedBackRepoWrapper.ExtraRepo;
            
            // Get domain-level aggregates + items.
            // Use separate additional repositories other than the one with the DI-Injected DbContext,
            // to avoid `InvalidOperationException` caused by two operations running concurrently in the same DbContext,
            // since we will do Task.WhenAll() as the below.
            var aggregatesTask = secondFeedBackRepo.GetAggregatesAsync(filter, ownerId, scopeToOwner);
            var itemsDomainTask = thirdFeedBackRepo.GetPagedReportAsync(filter, Math.Max(1, request.Page), Math.Clamp(request.PageSize, 5, 50), ownerId, scopeToOwner);

            await Task.WhenAll(aggregatesTask, itemsDomainTask);
            var aggregates = await aggregatesTask;
            var itemsDomain = await itemsDomainTask;

            // Map domain read-models -> application DTOs (explicit mapping)
            var items = itemsDomain.Select(d => new FeedbackReportItemDto
            {
                FeedbackId = d.FeedbackId,
                FeedbackDate = d.FeedbackDate,
                Stars = d.Stars,
                Comment = d.Comment,
                UserName = d.UserName,
                CarId = d.CarId,
                CarLabel = d.CarLabel,
                BookingNo = d.BookingNo,
                BookingItemId = d.BookingItemId,
                PickupDate  = d.PickupDate,
                ReturnDate  = d.ReturnDate,
                CarImageUrls = d.CarImageUrls != null ? new List<string>(d.CarImageUrls) : null
            }).ToList();

            return new FeedbackReportDto
            {
                Items = items,
                Total = aggregates.Total,
                AverageStars = aggregates.AverageStars,
                Count1Star = aggregates.Count1,
                Count2Stars = aggregates.Count2,
                Count3Stars = aggregates.Count3,
                Count4Stars = aggregates.Count4,
                Count5Stars = aggregates.Count5
            };
        }
    }
}