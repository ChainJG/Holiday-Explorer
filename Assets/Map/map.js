let holidayMap;
let holidayMarkers = [];
let attractionMarkers = [];
let startingPointMarker;
let activeTravelRoute;
let loadedHolidays = [];

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
        maxZoom: 16,
        zoomControl: true,
        worldCopyJump: true
    });

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 16,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(holidayMap);

    holidayMap.createPane("travel-routes");
    holidayMap.getPane("travel-routes").style.zIndex = 450;

    holidayMap.createPane("attractions");
    holidayMap.getPane("attractions").style.zIndex = 650;

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
            zIndexOffset: 1000,
            title: startingPoint.location
        })
        .addTo(holidayMap);
}

function loadHolidayMarkers(holidays) {
    loadedHolidays = holidays ?? [];

    clearHolidayMarkers();
    clearAttractionMarkers();

    loadedHolidays.forEach(holiday => {
        const marker = L.marker(
            [holiday.latitude, holiday.longitude],
            {
                title: `${holiday.name}, ${holiday.country}`
            })
            .addTo(holidayMap);

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

function selectHolidayById(holidayId) {
    const holiday = loadedHolidays.find(item => item.id === holidayId);

    if (!holiday) {
        return;
    }

    clearTravelRoute();
    clearAttractionMarkers();

    holidayMap.flyTo(
        [holiday.latitude, holiday.longitude],
        13,
        {
            animate: true,
            duration: 1.2
        });

    window.setTimeout(() => {
        loadAttractionMarkers(holiday);
    }, 650);
}

function loadAttractionMarkers(holiday) {
    clearAttractionMarkers();

    if (!holiday.attractions || holiday.attractions.length === 0) {
        return;
    }

    holiday.attractions.forEach(attraction => {
        const attractionIcon = L.divIcon({
            className: "attraction-marker",
            html: `<div class="attraction-pin">★</div>`,
            iconSize: [30, 30],
            iconAnchor: [15, 15],
            popupAnchor: [0, -15]
        });

        const marker = L.marker(
            [attraction.latitude, attraction.longitude],
            {
                icon: attractionIcon,
                pane: "attractions",
                title: attraction.name
            })
            .addTo(holidayMap);

        marker.on('mouseover', () => {
            sendAttractionHoveredMessage(attraction.id);
        });

        marker.on('mouseout', () => {
            sendAttractionHoverEndedMessage();
        });

        attractionMarkers.push(marker);
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
        pane: "travel-routes",
        color: "#2f80ed",
        weight: 3,
        opacity: 0.9,
        dashArray: "8, 12",
        lineCap: "round",
        lineJoin: "round",
        interactive: false
    }).addTo(holidayMap);

    activeTravelRoute.bindTooltip(getTravelTimeText(holiday), {
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

function clearHolidayMarkers() {
    holidayMarkers.forEach(marker => {
        holidayMap.removeLayer(marker);
    });

    holidayMarkers = [];
}

function clearAttractionMarkers() {
    attractionMarkers.forEach(marker => {
        holidayMap.removeLayer(marker);
    });

    attractionMarkers = [];
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

function getTravelTimeText(holiday) {
    const destinationName = holiday.name ?? "Destination";
    const flightDuration = holiday.flightDuration ?? "Flight time unavailable";

    return `✈ Derby → ${destinationName} • ${flightDuration}`;
}

function sendHolidaySelectedMessage(holidayId) {
    postWebViewMessage({
        type: 'holiday-selected',
        holidayId: holidayId
    });
}

function sendAttractionHoveredMessage(attractionId) {
    postWebViewMessage({
        type: 'attraction-hovered',
        attractionId: attractionId
    });
}

function sendAttractionHoverEndedMessage() {
    postWebViewMessage({
        type: 'attraction-hover-ended'
    });
}

function postWebViewMessage(message) {
    if (!window.chrome || !window.chrome.webview) {
        console.warn('WebView2 messaging is not available.');
        return;
    }

    window.chrome.webview.postMessage(message);
}

document.addEventListener('DOMContentLoaded', initialiseMap);