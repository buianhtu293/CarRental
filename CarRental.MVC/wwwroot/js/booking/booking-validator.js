// BookingValidator.js - Comprehensive validation for booking process
window.BookingValidator = (function() {
    'use strict';

    // Error messages
    const MESSAGES = {
        REQUIRED_FIELD: 'This field is required',
        INVALID_EMAIL: 'Please enter a valid email address',
        INVALID_PHONE: 'Please enter a valid phone number',
        INVALID_DATE: 'Please select a valid date',
        PICKUP_DATE_PAST: 'Pickup date cannot be in the past',
        RETURN_DATE_INVALID: 'Return date must be after pickup date',
        MIN_AGE: 'You must be at least 18 years old',
        DUPLICATE_LICENSE: 'Driver license numbers must be unique',
        PAYMENT_METHOD_REQUIRED: 'Please select a payment method',
        TERMS_REQUIRED: 'You must agree to the terms and conditions',
        INSUFFICIENT_BALANCE: 'Insufficient wallet balance for this payment method'
    };

    // Validation rules
    const RULES = {
        email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
        phone: /^[0-9+\-\s()]{10,15}$/,
        minAge: 18
    };

    function validateStep(stepNumber) {
        switch(stepNumber) {
            case 1:
                return validateInformationStep();
            case 2:
                return validatePaymentStep();
            case 3:
                return true; // Confirmation step doesn't need validation
            default:
                return false;
        }
    }

    function validateInformationStep() {
        clearValidationErrors();
        let isValid = true;

        // Validate dates
        const pickupDate = document.getElementById('PickupDate');
        const returnDate = document.getElementById('ReturnDate');
        
        if (pickupDate && returnDate) {
            const pickup = new Date(pickupDate.value);
            const returnDt = new Date(returnDate.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (pickup < today) {
                showFieldError(pickupDate, MESSAGES.PICKUP_DATE_PAST);
                isValid = false;
            }

            if (returnDt <= pickup) {
                showFieldError(returnDate, MESSAGES.RETURN_DATE_INVALID);
                isValid = false;
            }
        }

        // Validate renter information
        const renterFields = [
            { id: 'RenterFullName', required: true },
            { id: 'RenterEmail', required: true, type: 'email' },
            { id: 'RenterPhone', required: true, type: 'phone' },
            { id: 'RenterNationalId', required: true },
            { id: 'RenterLicense', required: true },
            { id: 'RenterDateOfBirth', required: true, type: 'age' }
        ];

        renterFields.forEach(field => {
            const element = document.getElementById(field.id);
            if (element) {
                if (!validateField(element, field)) {
                    isValid = false;
                }
            }
        });

        // Validate drivers
        const driverCards = document.querySelectorAll('.driver-card');
        const allLicenseNumbers = [];

        // Add renter license to check for duplicates
        const renterLicense = document.getElementById('RenterLicense');
        if (renterLicense && renterLicense.value.trim()) {
            allLicenseNumbers.push(renterLicense.value.trim().toUpperCase());
        }

        driverCards.forEach((card, index) => {
            const sameAsRenterCheckbox = card.querySelector('.same-as-renter');
            const isSameAsRenter = sameAsRenterCheckbox && sameAsRenterCheckbox.checked;

            if (!isSameAsRenter) {
                const licenseInput = card.querySelector('input[name$=".LicenseNumber"]');
                if (licenseInput) {
                    if (!licenseInput.value.trim()) {
                        showFieldError(licenseInput, MESSAGES.REQUIRED_FIELD);
                        isValid = false;
                    } else {
                        const licenseNumber = licenseInput.value.trim().toUpperCase();
                        if (allLicenseNumbers.includes(licenseNumber)) {
                            showFieldError(licenseInput, MESSAGES.DUPLICATE_LICENSE);
                            isValid = false;
                        } else {
                            allLicenseNumbers.push(licenseNumber);
                        }
                    }
                }
            }
        });

        return isValid;
    }

    function validatePaymentStep() {
        clearValidationErrors();
        let isValid = true;

        // Validate payment method selection
        const paymentMethods = document.querySelectorAll('input[name="Payment.SelectedPaymentMethod"]');
        const selectedPaymentMethod = Array.from(paymentMethods).find(radio => radio.checked);

        if (!selectedPaymentMethod) {
            const errorElement = document.getElementById('payment-method-error');
            if (errorElement) {
                errorElement.style.display = 'block';
                errorElement.textContent = MESSAGES.PAYMENT_METHOD_REQUIRED;
            }
            isValid = false;
        } else {
            // Check wallet balance if wallet is selected
            if (selectedPaymentMethod.value === '1') {
                const walletCard = selectedPaymentMethod.closest('.payment-method-card');
                const insufficientBadge = walletCard.querySelector('.badge-warning');
                if (insufficientBadge && insufficientBadge.textContent.includes('Insufficient')) {
                    showValidationMessage(MESSAGES.INSUFFICIENT_BALANCE, 'error');
                    isValid = false;
                }
            }
        }

        // Validate terms acceptance
        const termsCheckbox = document.getElementById('agree-terms');
        if (termsCheckbox && !termsCheckbox.checked) {
            showFieldError(termsCheckbox, MESSAGES.TERMS_REQUIRED);
            isValid = false;
        }

        return isValid;
    }

    function validateField(element, fieldConfig) {
        const value = element.value.trim();
        
        // Check required
        if (fieldConfig.required && !value) {
            showFieldError(element, MESSAGES.REQUIRED_FIELD);
            return false;
        }

        if (value) {
            // Type-specific validation
            switch (fieldConfig.type) {
                case 'email':
                    if (!RULES.email.test(value)) {
                        showFieldError(element, MESSAGES.INVALID_EMAIL);
                        return false;
                    }
                    break;
                    
                case 'phone':
                    if (!RULES.phone.test(value)) {
                        showFieldError(element, MESSAGES.INVALID_PHONE);
                        return false;
                    }
                    break;
                    
                case 'age':
                    const birthDate = new Date(value);
                    const today = new Date();
                    const age = today.getFullYear() - birthDate.getFullYear();
                    const monthDiff = today.getMonth() - birthDate.getMonth();
                    
                    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
                        age--;
                    }
                    
                    if (age < RULES.minAge) {
                        showFieldError(element, MESSAGES.MIN_AGE);
                        return false;
                    }
                    break;
            }
        }

        // If we get here, field is valid
        showFieldSuccess(element);
        return true;
    }

    function showFieldError(element, message) {
        element.classList.remove('is-valid');
        element.classList.add('is-invalid');
        
        // Find or create feedback element
        let feedback = element.parentNode.querySelector('.invalid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            element.parentNode.appendChild(feedback);
        }
        feedback.textContent = message;
        feedback.style.display = 'block';
    }

    function showFieldSuccess(element) {
        element.classList.remove('is-invalid');
        element.classList.add('is-valid');
        
        const feedback = element.parentNode.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.style.display = 'none';
        }
    }

    function clearValidationErrors() {
        document.querySelectorAll('.is-invalid').forEach(element => {
            element.classList.remove('is-invalid');
        });
        
        document.querySelectorAll('.is-valid').forEach(element => {
            element.classList.remove('is-valid');
        });
        
        document.querySelectorAll('.invalid-feedback').forEach(element => {
            element.style.display = 'none';
        });

        const paymentError = document.getElementById('payment-method-error');
        if (paymentError) {
            paymentError.style.display = 'none';
        }
    }

    function showValidationSummary(errors) {
        let errorMessage = 'Please correct the following errors:\n';
        errors.forEach(error => {
            errorMessage += `• ${error.message}\n`;
        });
        
        showValidationMessage(errorMessage, 'error');
    }

    // Real-time validation setup
    function setupRealTimeValidation() {
        // Email validation
        document.getElementById('RenterEmail')?.addEventListener('blur', function() {
            validateField(this, { required: true, type: 'email' });
        });

        // Phone validation
        document.getElementById('RenterPhone')?.addEventListener('blur', function() {
            validateField(this, { required: true, type: 'phone' });
        });

        // Age validation
        document.getElementById('RenterDateOfBirth')?.addEventListener('change', function() {
            validateField(this, { required: true, type: 'age' });
        });

        // Required field validation
        document.querySelectorAll('input[required], select[required]').forEach(element => {
            element.addEventListener('blur', function() {
                if (!this.value.trim()) {
                    showFieldError(this, MESSAGES.REQUIRED_FIELD);
                } else {
                    showFieldSuccess(this);
                }
            });
        });

        // License number validation
        document.querySelectorAll('input[name$=".LicenseNumber"]').forEach(input => {
            input.addEventListener('blur', function() {
                const card = this.closest('.driver-card');
                const sameAsRenterCheckbox = card?.querySelector('.same-as-renter');
                const isSameAsRenter = sameAsRenterCheckbox?.checked;

                if (!isSameAsRenter) {
                    validateField(this, { required: true });
                }
            });
        });

        // Date validation
        document.getElementById('PickupDate')?.addEventListener('change', function() {
            const pickup = new Date(this.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (pickup < today) {
                showFieldError(this, MESSAGES.PICKUP_DATE_PAST);
            } else {
                showFieldSuccess(this);
                // Also validate return date
                const returnDate = document.getElementById('ReturnDate');
                if (returnDate && returnDate.value) {
                    const returnDt = new Date(returnDate.value);
                    if (returnDt <= pickup) {
                        showFieldError(returnDate, MESSAGES.RETURN_DATE_INVALID);
                    } else {
                        showFieldSuccess(returnDate);
                    }
                }
            }
        });

        document.getElementById('ReturnDate')?.addEventListener('change', function() {
            const returnDt = new Date(this.value);
            const pickupDate = document.getElementById('PickupDate');
            
            if (pickupDate && pickupDate.value) {
                const pickup = new Date(pickupDate.value);
                if (returnDt <= pickup) {
                    showFieldError(this, MESSAGES.RETURN_DATE_INVALID);
                } else {
                    showFieldSuccess(this);
                }
            }
        });
    }

    // Public API
    return {
        validateStep: validateStep,
        validateInformationStep: validateInformationStep,
        validatePaymentStep: validatePaymentStep,
        validateField: validateField,
        showFieldError: showFieldError,
        showFieldSuccess: showFieldSuccess,
        clearValidationErrors: clearValidationErrors,
        showValidationSummary: showValidationSummary,
        setupRealTimeValidation: setupRealTimeValidation,
        MESSAGES: MESSAGES
    };
})();

// Initialize real-time validation when document is ready
document.addEventListener('DOMContentLoaded', function() {
    BookingValidator.setupRealTimeValidation();
});

// Global validation message function
function showValidationMessage(message, type) {
    // Remove existing notifications
    document.querySelectorAll('.validation-notification').forEach(el => el.remove());
    
    const alertClass = type === 'success' ? 'alert-success' : 
                     type === 'error' ? 'alert-danger' : 'alert-info';
    
    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed validation-notification`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px; max-width: 500px;';
    notification.innerHTML = `
        ${message.replace(/\n/g, '<br>')}
        <button type="button" class="close" data-dismiss="alert" aria-label="Close">
            <span aria-hidden="true">&times;</span>
        </button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}