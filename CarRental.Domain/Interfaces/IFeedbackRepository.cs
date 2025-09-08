using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Models.Feedback;

namespace CarRental.Domain.Interfaces
{
    public interface IFeedbackRepository : IGenericRepository<Feedback, Guid>
    {
        /// <summary>
        /// Calculates aggregates for the given filter, optionally scoping to an owner.
        /// </summary>
        Task<FeedbackAggregates> GetAggregatesAsync(
            FeedbackReportFilter filter,
            Guid? ownerId = null,
            bool scopeToOwner = false,
            CancellationToken ct = default);

        /// <summary>
        /// Returns paged list of feedback report items (domain read-model).
        /// </summary>
        Task<List<FeedbackReportItem>> GetPagedReportAsync(
            FeedbackReportFilter filter,
            int page,
            int pageSize,
            Guid? ownerId = null,
            bool scopeToOwner = false,
            CancellationToken ct = default);
    }
}
