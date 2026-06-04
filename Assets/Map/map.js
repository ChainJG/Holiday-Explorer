let holidayMap;
let holidayMarkers = [];
let startingPointMarker;
let activeTravelRoute;

const startingPoint = {
    id: "starting-point",
    name: "Starting Point",
    location: "Derby, UK",
    latitude: 52.9225,
    longitude: -1.4746
};

function initialiseMap() {
    holidayMap = L.map('map', {
        center: [startingPoint.latitude, startingPoint.longitude],
        zoom: 4,
        minZoom: 2,
        maxZoom: 8,
        zoomControl: true,
        worldCopyJump: true
    });

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 8,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(holidayMap);

    addStartingPointMarker();
}

function addStartingPointMarker() {
    const startingPointIcon = L.divIcon({
        className: "starting-point-marker",
        html: `<div class="starting-point-pin">🏠</div>`,
        iconSize: [36, 36],
        iconAnchor: [18, 18],
        popupAnchor: [0, -18]
    });

    startingPointMarker = L.marker(
        [startingPoint.latitude, startingPoint.longitude],
        {
            icon: startingPointIcon,
            zIndexOffset: 1000
        })
        .addTo(holidayMap)
        .bindPopup(`
            <strong>${startingPoint.name}</strong><br/>
            ${startingPoint.location}
        `);

    startingPointMarker.on('click', () => {
        sendStartingPointSelectedMessage();
    });
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

        marker.on('mouseover', () => {
            showTravelRouteToHoliday(holiday);
        });

        marker.on('mouseout', () => {
            clearTravelRoute();
        });

        holidayMarkers.push(marker);
    });
}

function showTravelRouteToHoliday(holiday) {
    clearTravelRoute();

    const from = {
        latitude: startingPoint.latitude,
        longitude: startingPoint.longitude
    };

    const to = {
        latitude: holiday.latitude,
        longitude: holiday.longitude
    };

    const routePoints = createCurvedRoutePoints(from, to);

    activeTravelRoute = L.polyline(routePoints, {
        color: "#2f80ed",
        weight: 3,
        opacity: 0.9,
        dashArray: "8, 12",
        lineCap: "round",
        lineJoin: "round",
        interactive: false
    }).addTo(holidayMap);

    const travelTimeText = getTravelTimeText(holiday);

    activeTravelRoute.bindTooltip(travelTimeText, {
        permanent: true,
        direction: "center",
        className: "travel-time-tooltip",
        opacity: 1
    }).openTooltip();
}

function clearTravelRoute() {
    if (!activeTravelRoute) {
        return;
    }

    holidayMap.removeLayer(activeTravelRoute);
    activeTravelRoute = null;
}

function createCurvedRoutePoints(from, to) {
    const points = [];

    const startLat = from.latitude;
    const startLng = from.longitude;
    const endLat = to.latitude;
    const endLng = to.longitude;

    const latDifference = endLat - startLat;
    const lngDifference = endLng - startLng;

    const distance = Math.sqrt(
        latDifference * latDifference +
        lngDifference * lngDifference
    );

    const curveStrength = Math.min(distance * 0.25, 12);

    const controlLat = (startLat + endLat) / 2 + curveStrength;
    const controlLng = (startLng + endLng) / 2;

    const totalSegments = 80;

    for (let i = 0; i <= totalSegments; i++) {
        const t = i / totalSegments;

        const lat = quadraticBezier(startLat, controlLat, endLat, t);
        const lng = quadraticBezier(startLng, controlLng, endLng, t);

        points.push([lat, lng]);
    }

    return points;
}

function quadraticBezier(start, control, end, t) {
    return (
        (1 - t) * (1 - t) * start +
        2 * (1 - t) * t * control +
        t * t * end
    );
}

function clearHolidayMarkers() {
    holidayMarkers.forEach(marker => {
        holidayMap.removeLayer(marker);
    });

    holidayMarkers = [];
    clearTravelRoute();
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

function sendStartingPointSelectedMessage() {
    if (!window.chrome || !window.chrome.webview) {
        console.warn('WebView2 messaging is not available.');
        return;
    }

    window.chrome.webview.postMessage({
        type: 'starting-point-selected',
        location: startingPoint.location
    });
}

function getTravelTimeText(holiday) {
    const destinationName = holiday.name ?? "Destination";
    const flightDuration = holiday.flightDuration ?? "Flight time unavailable";

    return `✈ Derby → ${destinationName} • ${flightDuration}`;
}

document.addEventListener('DOMContentLoaded', initialiseMap);