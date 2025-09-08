// Booking Edit JavaScript
var currentStep = 1;
var selectedCity = "";
var selectedDistrict = "";
var selectedWard = "";

$(document).ready(function () {
    // Get selected values from data attributes
    selectedCity = $('#RenterCity').data('selected-city') || "";
    selectedDistrict = $('#RenterDistrict').data('selected-district') || "";
    selectedWard = $('#RenterWard').data('selected-ward') || "";

    // Load provinces and set selected values
    loadProvinces();

    // Initialize driver dropdowns
    initializeDriverDropdowns();

    // Same as renter functionality for drivers
    $('.same-as-renter').change(function () {
        var driverIndex = $(this).data('driver-index');
        var isChecked = $(this).is(':checked');

        if (isChecked) {
            // Uncheck other same-as-renter checkboxes
            $('.same-as-renter').not(this).prop('checked', false);
            $('.driver-details').removeClass('d-none');

            // Hide current driver details
            $('#driver-details-' + driverIndex).addClass('d-none');

            // Copy renter info to this driver
            copyRenterToDriver(driverIndex);
        } else {
            $('#driver-details-' + driverIndex).removeClass('d-none');
        }

        // Update hidden field
        $('input[name="Information.Drivers[' + driverIndex + '].IsSameAsRenter"]').val(isChecked.toString().toLowerCase());
    });

    // Continue to confirmation
    $('#continue-to-confirmation').click(function () {
        if (validateEditForm()) {
            showConfirmation();
        }
    });

    // Back to edit
    $('#back-to-edit').click(function () {
        showStep(1);
    });

    // Save changes
    $('#save-changes').click(function () {
        saveChanges();
    });

    // Same validation logic as _BookingInformation
    function validateEditForm() {
        var isValid = true;
        $('.form-control:not([disabled])').removeClass('is-invalid is-valid');
        $('.invalid-feedback').hide();

        // Validate required fields (except disabled date fields)
        $('[required]:not([disabled])').each(function () {
            if (!$(this).val().trim()) {
                $(this).addClass('is-invalid');
                $(this).next('.invalid-feedback').show();
                isValid = false;
            } else {
                $(this).addClass('is-valid');
            }
        });

        // Validate email format
        var email = $('#RenterEmail').val();
        if (email && !isValidEmail(email)) {
            $('#RenterEmail').addClass('is-invalid');
            isValid = false;
        }

        // Validate driver emails
        $('input[type="email"][name*="Drivers"]').each(function () {
            var driverEmail = $(this).val();
            if (driverEmail && !isValidEmail(driverEmail)) {
                $(this).addClass('is-invalid');
                isValid = false;
            }
        });

        // Validate date of birth (must be 18+)
        var dob = new Date($('#RenterDateOfBirth').val());
        var today = new Date();
        var age = today.getFullYear() - dob.getFullYear();
        var monthDiff = today.getMonth() - dob.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
            age--;
        }
        if (age < 18) {
            $('#RenterDateOfBirth').addClass('is-invalid');
            isValid = false;
        }

        // Validate driver ages
        $('input[type="date"][name*="Drivers"][name*="DateOfBirth"]').each(function () {
            var driverDob = new Date($(this).val());
            var driverAge = today.getFullYear() - driverDob.getFullYear();
            var driverMonthDiff = today.getMonth() - driverDob.getMonth();
            if (driverMonthDiff < 0 || (driverMonthDiff === 0 && today.getDate() < driverDob.getDate())) {
                driverAge--;
            }
            if (driverAge < 18) {
                $(this).addClass('is-invalid');
                isValid = false;
            }
        });

        if (!isValid) {
            toastr.error('Please correct the form errors before continuing.');
        }

        return isValid;
    }

    function isValidEmail(email) {
        var atIndex = email.indexOf('@');
        var dotIndex = email.lastIndexOf('.');
        return atIndex > 0 && dotIndex > atIndex && email.length > 5;
    }

    function copyRenterToDriver(driverIndex) {
        var renterInfo = {
            fullName: $('#RenterFullName').val() || '',
            email: $('#RenterEmail').val() || '',
            phoneNumber: $('#RenterPhone').val() || '',
            dateOfBirth: $('#RenterDateOfBirth').val() || '',
            licenseNumber: $('#RenterLicense').val() || '',
            address: $('#RenterAddress').val() || '',
            licenseImage: $('#renter-license-url').val() || '' // S? d?ng licenseImage thay vì licenseImageUrl
        };

        // Copy basic info
        $('input[name="Information.Drivers[' + driverIndex + '].FullName"]').val(renterInfo.fullName);
        $('input[name="Information.Drivers[' + driverIndex + '].Email"]').val(renterInfo.email);
        $('input[name="Information.Drivers[' + driverIndex + '].PhoneNumber"]').val(renterInfo.phoneNumber);
        $('input[name="Information.Drivers[' + driverIndex + '].DateOfBirth"]').val(renterInfo.dateOfBirth);
        $('input[name="Information.Drivers[' + driverIndex + '].LicenseNumber"]').val(renterInfo.licenseNumber);
        $('input[name="Information.Drivers[' + driverIndex + '].Address"]').val(renterInfo.address);

        // Copy address selections (TEXT VALUES instead of IDs)
        var renterCityText = $('#RenterCity option:selected').text();
        var renterDistrictText = $('#RenterDistrict option:selected').text();
        var renterWardText = $('#RenterWard option:selected').text();

        // Set driver address v?i text values
        if (renterCityText && renterCityText !== '-- Select City --') {
            $('#DriverCity_' + driverIndex).val(renterCityText).trigger('change');
            setTimeout(function () {
                if (renterDistrictText && renterDistrictText !== '-- Select District --') {
                    $('#DriverDistrict_' + driverIndex).val(renterDistrictText).trigger('change');
                    setTimeout(function () {
                        if (renterWardText && renterWardText !== '-- Select Ward --') {
                            $('#DriverWard_' + driverIndex).val(renterWardText);
                        }
                    }, 500);
                }
            }, 500);
        }

        // Copy license image
        if (renterInfo.licenseImage) {
            $('#license-url-' + driverIndex).val(renterInfo.licenseImage);
            showLicensePreview(driverIndex, renterInfo.licenseImage);
        }
    }

    function showConfirmation() {
        showStep(2);

        // Populate confirmation content
        var confirmationHtml = generateConfirmationHtml();
        $('.confirmation-content').html(confirmationHtml);
    }

    function generateConfirmationHtml() {
        var html = '';

        // Rental Period (Unchanged)
        html += '<div class="form-card card">';
        html += '<div class="card-header bg-info text-white">';
        html += '<h5 class="mb-0"><i class="fas fa-calendar mr-2"></i>Rental Period (Unchanged)</h5>';
        html += '</div>';
        html += '<div class="card-body">';
        html += '<div class="row">';
        html += '<div class="col-md-6">';
        html += '<strong>Pickup Date:</strong><br>';
        html += '<span class="text-success">' + new Date($('#PickupDate').val()).toLocaleDateString() + ' ' + new Date($('#PickupDate').val()).toLocaleTimeString() + '</span>';
        html += '</div>';
        html += '<div class="col-md-6">';
        html += '<strong>Return Date:</strong><br>';
        html += '<span class="text-danger">' + new Date($('#ReturnDate').val()).toLocaleDateString() + ' ' + new Date($('#ReturnDate').val()).toLocaleTimeString() + '</span>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        html += '</div>';

        // Updated Renter Information
        html += '<div class="form-card card">';
        html += '<div class="card-header bg-primary text-white">';
        html += '<h5 class="mb-0"><i class="fas fa-user mr-2"></i>Updated Renter Information</h5>';
        html += '</div>';
        html += '<div class="card-body">';
        html += '<div class="row">';
        html += '<div class="col-md-6">';
        html += '<p><strong>Name:</strong> ' + $('#RenterFullName').val() + '</p>';
        html += '<p><strong>Email:</strong> ' + $('#RenterEmail').val() + '</p>';
        html += '<p><strong>Phone:</strong> ' + $('#RenterPhone').val() + '</p>';
        html += '</div>';
        html += '<div class="col-md-6">';
        html += '<p><strong>Date of Birth:</strong> ' + new Date($('#RenterDateOfBirth').val()).toLocaleDateString() + '</p>';
        html += '<p><strong>License Number:</strong> ' + $('#RenterLicense').val() + '</p>';
        html += '<p><strong>Address:</strong> ' + $('#RenterAddress').val() + '</p>';
        html += '</div>';
        html += '</div>';
        html += '<div class="row">';
        html += '<div class="col-12">';
        html += '<p><strong>Location:</strong> ';

        var wardText = getSelectedText('#RenterWard');
        var districtText = getSelectedText('#RenterDistrict');
        var cityText = getSelectedText('#RenterCity');

        var locationParts = [];
        if (wardText) locationParts.push(wardText);
        if (districtText) locationParts.push(districtText);
        if (cityText) locationParts.push(cityText);

        html += locationParts.join(', ');
        html += '</p>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        html += '</div>';

        // Driver Information
        $('.driver-card').each(function (index) {
            var driverIndex = index;
            var isSameAsRenter = $('#SameAsRenter_' + driverIndex).is(':checked');

            html += '<div class="form-card card">';
            html += '<div class="card-header bg-warning text-dark">';
            html += '<h5 class="mb-0"><i class="fas fa-user-tie mr-2"></i>Driver ' + (driverIndex + 1) + ' Information</h5>';
            html += '</div>';
            html += '<div class="card-body">';

            if (isSameAsRenter) {
                html += '<p class="alert alert-info"><i class="fas fa-info-circle mr-2"></i>This driver is the same as the renter</p>';
            } else {
                html += '<div class="row">';
                html += '<div class="col-md-6">';
                html += '<p><strong>Name:</strong> ' + $('input[name="Information.Drivers[' + driverIndex + '].FullName"]').val() + '</p>';
                html += '<p><strong>Email:</strong> ' + $('input[name="Information.Drivers[' + driverIndex + '].Email"]').val() + '</p>';
                html += '<p><strong>Phone:</strong> ' + $('input[name="Information.Drivers[' + driverIndex + '].PhoneNumber"]').val() + '</p>';
                html += '</div>';
                html += '<div class="col-md-6">';
                var driverDob = $('input[name="Information.Drivers[' + driverIndex + '].DateOfBirth"]').val();
                html += '<p><strong>Date of Birth:</strong> ' + (driverDob ? new Date(driverDob).toLocaleDateString() : '') + '</p>';
                html += '<p><strong>License Number:</strong> ' + $('input[name="Information.Drivers[' + driverIndex + '].LicenseNumber"]').val() + '</p>';
                html += '<p><strong>Address:</strong> ' + $('input[name="Information.Drivers[' + driverIndex + '].Address"]').val() + '</p>';
                html += '</div>';
                html += '</div>';

                html += '<div class="row">';
                html += '<div class="col-12">';
                html += '<p><strong>Location:</strong> ';

                // L?y thông tin location c?a driver
                var driverWardText = getSelectedText('#DriverWard_' + driverIndex);
                var driverDistrictText = getSelectedText('#DriverDistrict_' + driverIndex);
                var driverCityText = getSelectedText('#DriverCity_' + driverIndex);

                var driverLocationParts = [];
                if (driverWardText) driverLocationParts.push(driverWardText);
                if (driverDistrictText) driverLocationParts.push(driverDistrictText);
                if (driverCityText) driverLocationParts.push(driverCityText);

                html += driverLocationParts.length > 0 ? driverLocationParts.join(', ') : 'Not specified';
                html += '</p>';
                html += '</div>';
                html += '</div>';
            }

            html += '</div>';
            html += '</div>';
        });

        return html;
    }

    // S?A Ð?I: getSelectedText d? l?y value thay vì text vì bây gi? value dã là text name
    function getSelectedText(selector) {
        var $element = $(selector);
        var selectedValue = $element.val();
        return (selectedValue && selectedValue.indexOf('-- Select') === -1) ? selectedValue : '';
    }

    function saveChanges() {
        var button = $('#save-changes');
        button.prop('disabled', true).html('<i class="fas fa-spinner fa-spin mr-2"></i>Saving...');

        // S? d?ng FormData d? có th? g?i files
        var formData = new FormData();
        var form = document.getElementById('edit-information-form');

        // Add form data t? form HTML
        $(form).find('input, select, textarea').each(function () {
            var $this = $(this);
            var name = $this.attr('name');
            var type = $this.attr('type');

            if (!name) return; // Skip elements without name

            if (type === 'file') {
                // Add file n?u có
                if (this.files && this.files.length > 0) {
                    formData.append(name, this.files[0]);
                }
            } else if (type === 'checkbox') {
                // Add checkbox value
                formData.append(name, $this.is(':checked'));
            } else if (type === 'radio') {
                // Add radio value n?u du?c ch?n
                if ($this.is(':checked')) {
                    formData.append(name, $this.val());
                }
            } else {
                // Add regular input values
                formData.append(name, $this.val() || '');
            }
        });

        // Add booking ID
        formData.append('id', $('#BookingItemId').val());

        $.ajax({
            url: '/BookingList/UpdateInformation',
            type: 'POST',
            data: formData,
            processData: false, // Không process data
            contentType: false, // Không set content type, d? browser t? set v?i boundary
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    setTimeout(function () {
                        window.location.href = '/BookingList';
                    }, 1000);
                } else {
                    toastr.error(response.message);
                    button.prop('disabled', false).html('<i class="fas fa-save mr-2"></i>Save Changes');
                }
            },
            error: function () {
                toastr.error('An error occurred while saving changes.');
                button.prop('disabled', false).html('<i class="fas fa-save mr-2"></i>Save Changes');
            }
        });
    }

    function showStep(stepNumber) {
        $('.booking-step').addClass('d-none');
        $('#step-' + stepNumber).removeClass('d-none');
        currentStep = stepNumber;
        updateProgressBar(stepNumber);
    }

    function updateProgressBar(step) {
        $('.progress-step').removeClass('active completed');
        for (var i = 1; i <= step; i++) {
            if (i < step) {
                $('.progress-step[data-step="' + i + '"]').addClass('completed');
            } else if (i === step) {
                $('.progress-step[data-step="' + i + '"]').addClass('active');
            }
        }

        $('.progress-line').removeClass('completed');
        if (step > 1) {
            $('.progress-line').addClass('completed');
        }
    }

    // S?A Ð?I: Load provinces v?i text values thay vì IDs
    function loadProvinces() {
        $.get('https://open.oapi.vn/location/provinces?size=1000', function (cities) {
            var selectedCityId = null;

            $.each(cities.data, function (index, c) {
                if (selectedCity && c.name === selectedCity) {
                    selectedCityId = c.id;
                }
                // Luu text name thay vì ID
                $('#RenterCity').append(
                    '<option value="' + c.name + '"' + (c.name === selectedCity ? ' selected' : '') + '>' + c.name + '</option>'
                );
            });

            if (selectedCityId) {
                loadDistrictsByName(selectedCity);
            }
        });
    }

    // S?A Ð?I: Load districts b?ng city name
    function loadDistrictsByName(cityName) {
        // Tìm city ID d?a trên name
        $.get('https://open.oapi.vn/location/provinces?size=1000', function (cities) {
            var cityId = null;
            $.each(cities.data, function (index, c) {
                if (c.name === cityName) {
                    cityId = c.id;
                    return false; // break
                }
            });

            if (cityId) {
                loadDistricts(cityId);
            }
        });
    }

    function loadDistricts(cityId) {
        $('#RenterDistrict').empty().append('<option value="">-- Select District --</option>');
        $('#RenterWard').empty().append('<option value="">-- Select Ward --</option>');

        if (!cityId) return;

        $.get('https://open.oapi.vn/location/districts/' + cityId + '?size=1000', function (districts) {
            var selectedDistrictId = null;

            $.each(districts.data, function (index, d) {
                if (selectedDistrict && d.name === selectedDistrict) {
                    selectedDistrictId = d.id;
                }
                // Luu text name thay vì ID
                $('#RenterDistrict').append(
                    '<option value="' + d.name + '"' + (d.name === selectedDistrict ? ' selected' : '') + '>' + d.name + '</option>'
                );
            });

            if (selectedDistrictId) {
                loadWardsByName(cityId, selectedDistrict);
            }
        });
    }

    // S?A Ð?I: Load wards b?ng district name
    function loadWardsByName(cityId, districtName) {
        // Tìm district ID d?a trên name
        $.get('https://open.oapi.vn/location/districts/' + cityId + '?size=1000', function (districts) {
            var districtId = null;
            $.each(districts.data, function (index, d) {
                if (d.name === districtName) {
                    districtId = d.id;
                    return false;
                }
            });

            if (districtId) {
                loadWards(districtId);
            }
        });
    }

    function loadWards(districtId) {
        $('#RenterWard').empty().append('<option value="">-- Select Ward --</option>');
        if (!districtId) return;

        $.get('https://open.oapi.vn/location/wards/' + districtId + '?size=1000', function (wards) {
            $.each(wards.data, function (index, w) {
                // Luu text name thay vì ID
                $('#RenterWard').append(
                    '<option value="' + w.name + '"' + (w.name === selectedWard ? ' selected' : '') + '>' + w.name + '</option>'
                );
            });
        });
    }

    // S?A Ð?I: Change handlers d? x? lý text values
    $('#RenterCity').change(function () {
        var cityName = $(this).val();
        if (cityName && cityName !== '-- Select City --') {
            loadDistrictsByName(cityName);
        } else {
            $('#RenterDistrict').empty().append('<option value="">-- Select District --</option>');
            $('#RenterWard').empty().append('<option value="">-- Select Ward --</option>');
        }
    });

    $('#RenterDistrict').change(function () {
        var districtName = $(this).val();
        var cityName = $('#RenterCity').val();

        if (districtName && districtName !== '-- Select District --' && cityName) {
            // Tìm l?i cityId d? load wards
            $.get('https://open.oapi.vn/location/provinces?size=1000', function (cities) {
                var cityId = null;
                $.each(cities.data, function (index, c) {
                    if (c.name === cityName) {
                        cityId = c.id;
                        return false;
                    }
                });

                if (cityId) {
                    loadWardsByName(cityId, districtName);
                }
            });
        } else {
            $('#RenterWard').empty().append('<option value="">-- Select Ward --</option>');
        }
    });

    // Driver address functionality - GI? NGUYÊN VÌ ÐÃ ÐU?C S?A TRONG PH?N TRU?C
    function initializeDriverDropdowns() {
        $('.driver-city').each(function () {
            var driverIndex = $(this).data('driver-index');
            var selectedCity = $(this).data('selected-city');
            var selectedDistrict = $('#DriverDistrict_' + driverIndex).data('selected-district');
            var selectedWard = $('#DriverWard_' + driverIndex).data('selected-ward');

            loadDriverCities(driverIndex, selectedCity, selectedDistrict, selectedWard);
        });
    }

    function loadDriverCities(driverIndex, selectedCity, selectedDistrict, selectedWard) {
        $.get('https://open.oapi.vn/location/provinces?size=1000', function (cities) {
            var citySelect = $('#DriverCity_' + driverIndex);
            var selectedCityId = null;

            $.each(cities.data, (index, c) => {
                if (selectedCity && c.name === selectedCity) {
                    selectedCityId = c.id;
                }
                // Luu text name thay vì ID
                citySelect.append(
                    `<option value="${c.name}" ${c.name === selectedCity ? "selected" : ""}>${c.name}</option>`
                );
            });

            if (selectedCityId && selectedDistrict) {
                loadDriverDistrictsByName(driverIndex, selectedCity, selectedDistrict, selectedWard);
            }
        });
    }

    function loadDriverDistrictsByName(driverIndex, cityName, selectedDistrict, selectedWard) {
        $.get('https://open.oapi.vn/location/provinces?size=1000', function (cities) {
            var cityId = null;
            $.each(cities.data, (index, c) => {
                if (c.name === cityName) {
                    cityId = c.id;
                    return false; // break
                }
            });
            if (cityId) {
                loadDriverDistricts(driverIndex, cityId, selectedDistrict, selectedWard);
            }
        });
    }

    function loadDriverDistricts(driverIndex, cityId, selectedDistrict, selectedWard) {
        var districtSelect = $('#DriverDistrict_' + driverIndex);
        var wardSelect = $('#DriverWard_' + driverIndex);

        districtSelect.empty().append('<option value="">-- Select District --</option>');
        wardSelect.empty().append('<option value="">-- Select Ward --</option>');

        if (!cityId) return;

        $.get(`https://open.oapi.vn/location/districts/${cityId}?size=1000`, function (districts) {
            var selectedDistrictId = null;

            $.each(districts.data, (index, d) => {
                if (selectedDistrict && d.name === selectedDistrict) {
                    selectedDistrictId = d.id;
                }
                districtSelect.append(
                    `<option value="${d.name}" ${d.name === selectedDistrict ? "selected" : ""}>${d.name}</option>`
                );
            });

            if (selectedDistrictId && selectedWard) {
                loadDriverWardsByName(driverIndex, cityId, selectedDistrict, selectedWard);
            }
        });
    }

    function loadDriverWardsByName(driverIndex, cityId, districtName, selectedWard) {
        // Tìm district ID d?a trên name
        $.get(`https://open.oapi.vn/location/districts/${cityId}?size=1000`, function (districts) {
            var districtId = null;
            $.each(districts.data, (index, d) => {
                if (d.name === districtName) {
                    districtId = d.id;
                    return false;
                }
            });

            if (districtId) {
                loadDriverWards(driverIndex, districtId, selectedWard);
            }
        });
    }

    function loadDriverWards(driverIndex, districtId, selectedWard) {
        var wardSelect = $('#DriverWard_' + driverIndex);
        wardSelect.empty().append('<option value="">-- Select Ward --</option>');

        if (!districtId) return;

        $.get(`https://open.oapi.vn/location/wards/${districtId}?size=1000`, function (wards) {
            $.each(wards.data, (index, w) => {
                wardSelect.append(
                    `<option value="${w.name}" ${w.name === selectedWard ? "selected" : ""}>${w.name}</option>`
                );
            });
        });
    }

    // Driver address change handlers
    $(document).on('change', '.driver-city', function () {
        var driverIndex = $(this).data('driver-index');
        var cityName = $(this).val();

        if (cityName && cityName !== '-- Select City --') {
            loadDriverDistrictsByName(driverIndex, cityName);
        } else {
            $('#DriverDistrict_' + driverIndex).empty().append('<option value="">-- Select District --</option>');
            $('#DriverWard_' + driverIndex).empty().append('<option value="">-- Select Ward --</option>');
        }
    });

    $(document).on('change', '.driver-district', function () {
        var driverIndex = $(this).data('driver-index');
        var districtName = $(this).val();
        var cityName = $('#DriverCity_' + driverIndex).val();

        if (districtName && districtName !== '-- Select District --' && cityName) {
            // Tìm l?i cityId d? load wards
            $.get('https://open.oapi.vn/location/provinces?size=1000', function (cities) {
                var cityId = null;
                $.each(cities.data, (index, c) => {
                    if (c.name === cityName) {
                        cityId = c.id;
                        return false;
                    }
                });

                if (cityId) {
                    loadDriverWardsByName(driverIndex, cityId, districtName);
                }
            });
        } else {
            // Clear wards
            $('#DriverWard_' + driverIndex).empty().append('<option value="">-- Select Ward --</option>');
        }
    });

    // Initialize driver dropdowns after page load
    setTimeout(initializeDriverDropdowns, 1000);

    // License preview function
    function showLicensePreview(driverIndex, imageUrl) {
        var previewHtml = '<div class="license-preview mt-2" id="license-preview-' + driverIndex + '">';
        previewHtml += '<img src="' + imageUrl + '" alt="License" class="img-thumbnail" style="max-width: 200px;">';
        previewHtml += '<button type="button" class="btn btn-sm btn-danger ml-2 remove-license" data-driver-index="' + driverIndex + '">';
        previewHtml += '<i class="fas fa-trash"></i> Remove</button></div>';

        $('#license-preview-' + driverIndex).remove();
        $(`input[data-driver-index="${driverIndex}"].license-upload`).parent().append(previewHtml);
    }

    function showRenterLicensePreview(imageUrl) {
        var previewHtml = `
            <div class="license-preview mt-2" id="renter-license-preview">
                <img src="${imageUrl}" alt="License" class="img-thumbnail" style="max-width: 200px;">
                <button type="button" class="btn btn-sm btn-danger ml-2 remove-renter-license">
                    <i class="fas fa-trash"></i> Remove
                </button>
            </div>
        `;

        $('#renter-license-preview').remove();
        $('.renter-license-upload').parent().append(previewHtml);
    }

    // Handle file selection for preview (no immediate upload)
    $(document).on('change', '.renter-license-upload', function () {
        var file = this.files[0];
        if (file) {
            if (file.size > 5 * 1024 * 1024) { // 5MB limit
                alert('File size must be less than 5MB');
                $(this).val('');
                return;
            }

            // Show preview immediately
            var reader = new FileReader();
            reader.onload = function (e) {
                showRenterLicensePreview(e.target.result);
            };
            reader.readAsDataURL(file);
        }
    });

    $(document).on('change', '.license-upload', function () {
        var file = this.files[0];
        var driverIndex = $(this).data('driver-index');

        if (file) {
            if (file.size > 5 * 1024 * 1024) { // 5MB limit
                alert('File size must be less than 5MB');
                $(this).val('');
                return;
            }

            // Show preview immediately
            var reader = new FileReader();
            reader.onload = function (e) {
                showLicensePreview(driverIndex, e.target.result);
            };
            reader.readAsDataURL(file);
        }
    });

    // Remove license image (clear file input and hidden URL)
    $(document).on('click', '.remove-renter-license', function () {
        $('#renter-license-preview').remove();
        $('.renter-license-upload').val('');
        $('#renter-license-url').val(''); // Clear hidden URL field
        toastr.success('License image removed');
    });

    $(document).on('click', '.remove-license', function () {
        var driverIndex = $(this).data('driver-index');
        $('#license-preview-' + driverIndex).remove();
        $(`input[data-driver-index="${driverIndex}"].license-upload`).val('');
        $('#license-url-' + driverIndex).val(''); // Clear hidden URL field
        toastr.success('License image removed');
    });

    // Show existing previews on page load
    $(document).ready(function () {
        // Show renter license preview if exists
        var renterLicenseUrl = $('#renter-license-url').val();
        if (renterLicenseUrl) {
            showRenterLicensePreview(renterLicenseUrl);
        }

        // Show driver license previews if exist
        $('.driver-card').each(function (index) {
            var licenseUrl = $('#license-url-' + index).val();
            if (licenseUrl) {
                showLicensePreview(index, licenseUrl);
            }
        });
    });
});