using CarRental.Application.DTOs.Cloudinary;
using CarRental.Application.DTOs.Mail;
using CarRental.Application.Interfaces;
using CarRental.Application.Services;
using CarRental.Application.Services.Implements;
using CarRental.Application.Services.Interfaces;
using CarRental.MVC.Services;

namespace CarRental.MVC.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add MVC specific services
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IBookingListService, BookingListService>();
            services.AddScoped<ICarService, CarService>();
            services.AddScoped<IHomepageService, HomepageService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICartItemService, CartItemService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<ICarOwnerBookingService, CarOwnerBookingService>();
            services.AddScoped<IFeedbackService, FeedbackService>();

            #region send mail
            //Dang ki mail
            services.AddOptions();
            var mailsetting = configuration.GetSection("MailSettings");
            services.Configure<MailSettings>(mailsetting);
            services.AddSingleton<IEmailSender, SendMailService>();
            #endregion

            #region upload photo
            var cloudinarysettings = configuration.GetSection("CloudinarySettings");
            services.Configure<CloudinarySettings>(cloudinarysettings);
			services.AddSingleton<IPhotoService, PhotoService>();
			#endregion

			return services;
        }
    }
}