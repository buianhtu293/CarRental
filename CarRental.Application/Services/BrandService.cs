using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BrandService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync()
        {
            var brands = await _unitOfWork.Repository<Brand, Guid>().GetAllAsync();

            return brands.Select(b => new BrandDto
            {
                BrandName = b.BrandName ?? string.Empty,
                ModelName = b.ModelName ?? string.Empty
            });
        }

        public async Task<IEnumerable<BrandDto>> GetModelsByBrandAsync(string brandName)
        {
            var models = await _unitOfWork.Repository<Brand, Guid>().GetAllAsync();

            return models.Select(b => new BrandDto
            {
                BrandName = b.BrandName ?? string.Empty,
                ModelName = b.ModelName ?? string.Empty
            }).Where(x => x.BrandName.ToLower().Equals(brandName.ToLower()));
        }
    }
}
