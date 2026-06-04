let currentHoliday = null;
let airports = [];

async function startHolidayExplorer() {

    const holidays = await fetchHolidays();
    airports = await fetchAirports();

    loadHolidayMarkers(holidays);
    loadAirportMarkers(airports);

    window.addEventListener("holiday-selected", event => {
        currentHoliday = event.detail;
        renderHoliday(currentHoliday);
    });

    window.addEventListener("attraction-hovered", event => {
        renderHoveredAttraction(event.detail);
    });

    window.addEventListener("attraction-hover-ended", () => {
        clearHoveredAttraction();
    });

    document.getElementById("flightPriceCard").addEventListener("click", () => {
        if (!currentHoliday) {
            return;
        }

        openFlightSearch(currentHoliday);
    });
}

async function fetchHolidays() {
    const response = await fetch("/api/holidays");

    if (!response.ok) {
        throw new Error("Failed to load holidays.");
    }

    return await response.json();
}

async function fetchAirports() {
    const response = await fetch("/api/airports");

    if (!response.ok) {
        throw new Error("Failed to load airports.");
    }

    return await response.json();
}

function renderHoliday(holiday) {
    document.getElementById("holidayName").textContent = holiday.name;
    document.getElementById("holidayCountry").textContent = holiday.country;

    document.getElementById("flightPrice").textContent =
        formatMoneyRange(holiday.estimatedFlightPriceForTwo);

    document.getElementById("flightDuration").textContent = holiday.flightDuration;
    document.getElementById("temperature").textContent = holiday.summerTemperature;
    document.getElementById("score").textContent = `${holiday.score}/10`;
    document.getElementById("verdict").textContent = holiday.verdict;

    renderAttractions(holiday.attractions ?? []);
}

function renderAttractions(attractions) {
    const container = document.getElementById("attractionList");

    container.innerHTML = "";

    attractions.forEach(attraction => {
        const card = document.createElement("article");
        card.className = "attraction-card";

        const image = document.createElement("img");
        image.alt = attraction.name;

        if (attraction.hasImage || attraction.imagePath) {
            image.src = buildAttractionImageUrl(attraction);

            image.onerror = () => {
                console.warn("Failed to load attraction image:", attraction);
                image.remove();
            };
        }
        else {
            image.remove();
        }

        const content = document.createElement("div");
        content.className = "attraction-card-content";

        const title = document.createElement("h3");
        title.textContent = attraction.name;

        const description = document.createElement("p");
        description.textContent = attraction.description;

        content.appendChild(title);
        content.appendChild(description);

        card.appendChild(image);
        card.appendChild(content);

        container.appendChild(card);
    });
}

function buildAttractionImageUrl(attraction) {
    return `/api/attractions/${encodeURIComponent(attraction.id)}/image?v=${Date.now()}`;
}

function renderHoveredAttraction(attraction) {
    document.getElementById("hoveredAttractionName").textContent = attraction.name;
    document.getElementById("hoveredAttractionDescription").textContent = attraction.description;
}

function clearHoveredAttraction() {
    document.getElementById("hoveredAttractionName").textContent = "None selected";
    document.getElementById("hoveredAttractionDescription").textContent = "";
}

function formatMoneyRange(range) {
    if (!range) {
        return "—";
    }

    const symbol = range.currencySymbol ?? "£";

    return `${symbol}${range.minimum}–${symbol}${range.maximum}`;
}

function buildAttractionImageUrl(attraction) {
    return `/api/attractions/${encodeURIComponent(attraction.id)}/image?v=${Date.now()}`;
}

function openFlightSearch(holiday) {
    const originCode = "ema";
    const destinationAirport = findBestAirportForHoliday(holiday);

    if (!destinationAirport) {
        alert("No airport could be found for this holiday.");
        return;
    }

    const destinationCode = destinationAirport.code.toLowerCase();

    const outboundDate = new Date();
    outboundDate.setMonth(outboundDate.getMonth() + 1);

    const returnDate = new Date(outboundDate);
    returnDate.setDate(returnDate.getDate() + 7);

    const outbound = formatSkyscannerDate(outboundDate);
    const inbound = formatSkyscannerDate(returnDate);

    const url =
        `https://www.skyscanner.net/transport/flights/${originCode}/${destinationCode}/${outbound}/${inbound}/` +
        "?adultsv2=2&cabinclass=economy&childrenv2=&ref=home&rtn=1" +
        "&outboundaltsenabled=false&inboundaltsenabled=false&preferdirects=false";

    window.open(url, "_blank", "noopener,noreferrer");
}

function findBestAirportForHoliday(holiday) {
    const preferredCityAirport = airports.find(airport =>
        airport.isPreferredForCity &&
        normaliseText(airport.city) === normaliseText(holiday.name));

    if (preferredCityAirport) {
        return preferredCityAirport;
    }

    return airports
        .map(airport => ({
            airport,
            distanceKm: calculateDistanceKm(
                holiday.latitude,
                holiday.longitude,
                airport.latitude,
                airport.longitude)
        }))
        .sort((a, b) => a.distanceKm - b.distanceKm)[0]?.airport ?? null;
}

function calculateDistanceKm(latitude1, longitude1, latitude2, longitude2) {
    const earthRadiusKm = 6371;

    const dLat = toRadians(latitude2 - latitude1);
    const dLon = toRadians(longitude2 - longitude1);

    const lat1 = toRadians(latitude1);
    const lat2 = toRadians(latitude2);

    const a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(lat1) * Math.cos(lat2) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);

    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

    return earthRadiusKm * c;
}

function toRadians(degrees) {
    return degrees * Math.PI / 180;
}

function normaliseText(value) {
    return value
        .toLowerCase()
        .trim();
}


function formatSkyscannerDate(date) {
    const year = String(date.getFullYear()).slice(2);
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    return `${year}${month}${day}`;
}

startHolidayExplorer().catch(error => {
    console.error(error);
    alert("Holiday Explorer failed to start.");
});