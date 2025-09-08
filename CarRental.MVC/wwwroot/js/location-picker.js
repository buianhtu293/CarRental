document.addEventListener('DOMContentLoaded', function () {
    const provinceSelect = document.getElementById('province-select');
    const districtSelect = document.getElementById('district-select');
    const wardSelect = document.getElementById('ward-select');

    const provinceNameHidden = document.querySelector('input[name="ProvinceName"]');
    const districtNameHidden = document.querySelector('input[name="DistrictName"]');
    const wardNameHidden = document.querySelector('input[name="WardName"]');

    const pickupDateEl = document.getElementById('pickup-date');
    const pickupTimeEl = document.getElementById('pickup-time');
    const returnDateEl = document.getElementById('return-date');
    const returnTimeEl = document.getElementById('return-time');
    const dateTimeError = document.getElementById('date-time-error');
    const form = document.querySelector('form');

    const API_HOST = "https://open.oapi.vn/location";

    async function fetchApi(endpoint) {
        try {
            const response = await fetch(`${API_HOST}${endpoint}`);
            if (!response.ok) {
                throw new Error(`API call failed with status: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error("API Fetch Error:", error);
            return [];
        }
    }

    // Hàm đổ dữ liệu vào select
    function populateSelect(selectElement, items, defaultOptionText) {
        selectElement.innerHTML = `<option value="">-- ${defaultOptionText} --</option>`;
        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item.id;
            option.textContent = item.name;
            selectElement.appendChild(option);
        });
        selectElement.disabled = items.length === 0;
    }

    async function loadProvinces() {
        const provinces = await fetchApi("/provinces?page=0&size=63");
        populateSelect(provinceSelect, provinces.data, 'Chọn Tỉnh/Thành phố');

        const selectedProvinceOption = provinceSelect.options[provinceSelect.selectedIndex];
        if (provinceNameHidden) {
            provinceNameHidden.value = selectedProvinceOption ? selectedProvinceOption.textContent : '';
        }
    }

    provinceSelect.addEventListener('change', async function () {
        const provinceCode = this.value;
        const selectedOption = this.options[this.selectedIndex];
        if (provinceNameHidden) {
            provinceNameHidden.value = selectedOption ? selectedOption.textContent : '';
        }

        populateSelect(districtSelect, [], 'Chọn Quận/Huyện');
        populateSelect(wardSelect, [], 'Chọn Phường/Xã');
        districtSelect.disabled = true;
        wardSelect.disabled = true;

        if (districtNameHidden) {
            districtNameHidden.value = '';
        }
        if (wardNameHidden) {
            wardNameHidden.value = '';
        }

        if (provinceCode) {
            const districts = await fetchApi(`/districts/${provinceCode}`);
            populateSelect(districtSelect, districts.data, 'Chọn Quận/Huyện');
        }
    });

    districtSelect.addEventListener('change', async function () {
        const districtCode = this.value;
        const selectedOption = this.options[this.selectedIndex];
        if (districtNameHidden) {
            districtNameHidden.value = selectedOption ? selectedOption.textContent : '';
        }

        populateSelect(wardSelect, [], 'Chọn Phường/Xã');
        wardSelect.disabled = true;

        if (wardNameHidden) {
            wardNameHidden.value = '';
        }

        if (districtCode) {
            const wards = await fetchApi(`/wards/${districtCode}`);
            populateSelect(wardSelect, wards.data, 'Chọn Phường/Xã');
        }
    });

    wardSelect.addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        if (wardNameHidden) {
            wardNameHidden.value = selectedOption ? selectedOption.textContent : '';
        }
    });

    const todayStr = (() => {
        const now = new Date();
        const m = String(now.getMonth() + 1).padStart(2, '0');
        const d = String(now.getDate()).padStart(2, '0');
        return `${now.getFullYear()}-${m}-${d}`;
    })();

    // Chặn chọn ngày quá khứ ngay từ UI
    pickupDateEl.min = todayStr;
    returnDateEl.min = todayStr;

    function firstEnabledValue(selectEl) {
        for (const opt of selectEl.options) if (!opt.disabled && !opt.hidden) return opt.value;
        return null;
    }

    function updatePickupTimeOptions() {
        const now = new Date();
        const isToday = pickupDateEl.value === todayStr;

        const minHour = isToday ? (now.getMinutes() > 0 ? now.getHours() + 1 : now.getHours()) : 0;

        Array.from(pickupTimeEl.options).forEach(opt => {
            const h = Number(opt.value);
            const disable = h < minHour;
            opt.disabled = disable;
            opt.hidden = disable;
        });

        // Nếu giá trị hiện tại bị vô hiệu thì auto nhảy tới giờ hợp lệ đầu tiên
        if (!pickupTimeEl.value || pickupTimeEl.selectedOptions[0]?.disabled) {
            const v = firstEnabledValue(pickupTimeEl);
            if (v !== null) pickupTimeEl.value = v;
        }
    }

    function updateReturnDateAndTimeOptions() {
        // return-date không được trước pickup-date
        if (pickupDateEl.value) {
            returnDateEl.min = pickupDateEl.value;
            if (returnDateEl.value && returnDateEl.value < pickupDateEl.value) {
                returnDateEl.value = pickupDateEl.value;
            }
        }

        const sameDay = returnDateEl.value && pickupDateEl.value && returnDateEl.value === pickupDateEl.value;
        const minReturnHour = sameDay ? Number(pickupTimeEl.value) + 1 : 0;

        Array.from(returnTimeEl.options).forEach(opt => {
            const h = Number(opt.value);
            const disable = h < minReturnHour;
            opt.disabled = disable;
            opt.hidden = disable;
        });

        if (!returnTimeEl.value || returnTimeEl.selectedOptions[0]?.disabled) {
            const v = firstEnabledValue(returnTimeEl);
            if (v !== null) returnTimeEl.value = v;
        }
    }

    function validateDateTime() {
        const pickupDate = pickupDateEl.value;
        const pickupTime = pickupTimeEl.value;
        const returnDate = returnDateEl.value;
        const returnTime = returnTimeEl.value;

        if (!pickupDate || !pickupTime || !returnDate || !returnTime) {
            dateTimeError.style.display = 'none';
            return true;
        }

        const now = new Date();
        const pickupDateTime = new Date(`${pickupDate}T${pickupTime.padStart(2, '0')}:00:00`);
        const returnDateTime = new Date(`${returnDate}T${returnTime.padStart(2, '0')}:00:00`);

        if (pickupDateTime < now) {
            dateTimeError.textContent = 'Thời gian nhận xe phải từ thời điểm hiện tại trở đi.';
            dateTimeError.style.display = 'block';
            return false;
        }

        if (returnDateTime <= pickupDateTime) {
            dateTimeError.textContent = 'Thời gian trả xe phải sau thời gian nhận xe.';
            dateTimeError.style.display = 'block';
            return false;
        }

        dateTimeError.style.display = 'none';
        return true;
    }

    updatePickupTimeOptions();
    updateReturnDateAndTimeOptions();

    pickupDateEl.addEventListener('change', () => {
        updatePickupTimeOptions();
        updateReturnDateAndTimeOptions();
        validateDateTime();
    });

    pickupTimeEl.addEventListener('change', () => {
        updateReturnDateAndTimeOptions();
        validateDateTime();
    });

    [pickupDateEl, pickupTimeEl, returnDateEl, returnTimeEl].forEach(el => {
        el.addEventListener('change', validateDateTime);
    });

    form.addEventListener('submit', function (event) {
        const selectedProvinceText = provinceSelect.value ? provinceSelect.options[provinceSelect.selectedIndex].textContent : '';
        const selectedDistrictText = districtSelect.value ? districtSelect.options[districtSelect.selectedIndex].textContent : '';
        const selectedWardText = wardSelect.value ? wardSelect.options[wardSelect.selectedIndex].textContent : '';

        // Gán giá trị vào các trường ẩn
        provinceNameHidden.value = selectedProvinceText;
        districtNameHidden.value = selectedDistrictText;
        wardNameHidden.value = selectedWardText;
        if (!validateDateTime()) {
            event.preventDefault();
            alert('Thời gian trả xe phải sau thời gian nhận xe. Vui lòng kiểm tra lại.');
        }
    });

    loadProvinces();
});