// Royal Cars Main JavaScript

(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner();

    // Initiate the wowjs
    if (typeof WOW !== 'undefined') {
        new WOW().init();
    }

    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 1500, 'easeInOutExpo');
        return false;
    });

    // Testimonials carousel
    $(".testimonial-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1000,
        center: true,
        margin: 25,
        dots: true,
        loop: true,
        nav: false,
        responsive: {
            0: {
                items: 1
            },
            768: {
                items: 2
            },
            992: {
                items: 3
            }
        }
    });

    // Vendor carousel
    $('.vendor-carousel').owlCarousel({
        loop: true,
        margin: 29,
        nav: false,
        autoplay: true,
        smartSpeed: 1000,
        responsive: {
            0: {
                items: 2
            },
            576: {
                items: 3
            },
            768: {
                items: 4
            },
            992: {
                items: 5
            },
            1200: {
                items: 6
            }
        }
    });

    // Date and time picker
    $('#date').datetimepicker({
        format: 'L'
    });
    $('#time').datetimepicker({
        format: 'LT'
    });

    // Form validation
    $('form').on('submit', function (e) {
        var isValid = true;
        $(this).find('[required]').each(function () {
            if ($(this).val() === '') {
                isValid = false;
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        if (!isValid) {
            e.preventDefault();
            alert('Please fill in all required fields.');
        }
    });

    // AJAX setup for authentication
    $.ajaxSetup({
        beforeSend: function (xhr) {
            var token = localStorage.getItem('authToken');
            if (token) {
                xhr.setRequestHeader('Authorization', 'Bearer ' + token);
            }
        }
    });

    // Car search functionality
    $('.car-search-form').on('submit', function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        
        $.ajax({
            url: '/api/cars/search',
            type: 'POST',
            data: formData,
            success: function (data) {
                // Update car listing
                updateCarListing(data);
            },
            error: function () {
                alert('Error searching cars. Please try again.');
            }
        });
    });

    function updateCarListing(cars) {
        var carListContainer = $('.car-listing-container');
        carListContainer.empty();
        
        if (cars.length === 0) {
            carListContainer.append('<div class="col-12"><p class="text-center">No cars found matching your criteria.</p></div>');
            return;
        }

        cars.forEach(function (car) {
            var carHtml = generateCarCard(car);
            carListContainer.append(carHtml);
        });
    }

    function generateCarCard(car) {
        return `
            <div class="col-lg-4 col-md-6 mb-2">
                <div class="rent-item mb-4">
                    <img class="img-fluid mb-4" src="${car.imageUrl || '/img/car-rent-1.png'}" alt="${car.brand} ${car.model}">
                    <h4 class="text-uppercase mb-4">${car.brand} ${car.model}</h4>
                    <div class="d-flex justify-content-center mb-4">
                        <div class="px-2">
                            <i class="fa fa-car text-primary mr-1"></i>
                            <span>${car.year}</span>
                        </div>
                        <div class="px-2 border-left border-right">
                            <i class="fa fa-cogs text-primary mr-1"></i>
                            <span>${car.transmission}</span>
                        </div>
                        <div class="px-2">
                            <i class="fa fa-road text-primary mr-1"></i>
                            <span>${car.fuelType}</span>
                        </div>
                    </div>
                    <div class="d-flex justify-content-center mb-4">
                        <div class="px-2">
                            <i class="fa fa-users text-primary mr-1"></i>
                            <span>${car.seats} Seats</span>
                        </div>
                        <div class="px-2 border-left">
                            <i class="fa fa-palette text-primary mr-1"></i>
                            <span>${car.color}</span>
                        </div>
                    </div>
                    <div class="d-flex justify-content-center mb-4">
                        <span class="badge badge-${car.isAvailable ? 'success' : 'danger'}">
                            ${car.status}
                        </span>
                    </div>
                    <div class="d-flex justify-content-center">
                        <div class="text-center">
                            <h3>$${car.pricePerDay.toFixed(2)}</h3>
                            <p class="mb-0">Per Day</p>
                        </div>
                    </div>
                    <div class="d-flex justify-content-center mt-4">
                        <a class="btn btn-primary px-3" href="/Car/Details/${car.id}">View Details</a>
                        ${car.isAvailable ? `<a class="btn btn-outline-primary px-3 ml-2" href="/Booking/Create?carId=${car.id}">Book Now</a>` : ''}
                    </div>
                </div>
            </div>
        `;
    }

})(jQuery);