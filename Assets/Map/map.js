let holidayMap;
let holidayMarkers = [];

function initialiseMap() {
    holidayMap = L.map('map', {
        center: [25, 0],
        zoom: 2,
        minZoom: 2,
        maxZoom: 8,
        zoomControl: true,
        worldCopyJump: true
    });

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 8,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(holidayMap);
}

function loadHolidayMarkers(holidays) {
    clearHolidayMarkers();

    holidays.forEach(holiday => {
        const marker = L.marker([holiday.latitude, holiday.longitude])
            .addTo(holidayMap)
            .bindPopup(`
                <strong>${holiday.name}</strong><br/>
                ${holiday.country}<br/>
                Score: ${holiday.score}/10
            `);

        marker.on('click', () => {
            sendHolidaySelectedMessage(holiday.id);
        });

        holidayMarkers.push(marker);
    });
}

function clearHolidayMarkers() {
    holidayMarkers.forEach(marker => {
        holidayMap.removeLayer(marker);
    });

    holidayMarkers = [];
}

function sendHolidaySelectedMessage(holidayId) {
    if (!window.chrome || !window.chrome.webview) {
        console.warn('WebView2 messaging is not available.');
        return;
    }

    window.chrome.webview.postMessage({
        type: 'holiday-selected',
        holidayId: holidayId
    });
}

document.addEventListener('DOMContentLoaded', initialiseMap);