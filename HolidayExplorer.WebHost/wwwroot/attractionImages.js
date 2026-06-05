const ATTRACTION_IMAGE_CACHE_PREFIX = "holidayExplorer.attractionImage.";
const WIKIMEDIA_IMAGE_WIDTH = 1200;

async function resolveAttractionImageUrl(holiday, attraction) {
    const cachedImage = getCachedAttractionImage(attraction.id);

    if (cachedImage) {
        return cachedImage.url;
    }

    const onlineImage = await findWikimediaImageForAttraction(holiday, attraction);

    if (!onlineImage) {
        return "";
    }

    cacheAttractionImage(attraction.id, onlineImage);

    return onlineImage.url;
}

async function findWikimediaImageForAttraction(holiday, attraction) {
    const searchQuery = `${attraction.name} ${holiday.name} landmark`;

    const parameters = new URLSearchParams({
        action: "query",
        format: "json",
        origin: "*",
        generator: "search",
        gsrnamespace: "6",
        gsrlimit: "8",
        gsrsearch: searchQuery,
        prop: "imageinfo",
        iiprop: "url|size|mime|extmetadata",
        iiurlwidth: String(WIKIMEDIA_IMAGE_WIDTH)
    });

    const requestUrl = `https://commons.wikimedia.org/w/api.php?${parameters}`;

    const response = await fetch(requestUrl);

    if (!response.ok) {
        console.warn("Wikimedia image lookup failed:", response.status, attraction);
        return null;
    }

    const payload = await response.json();
    const pages = payload?.query?.pages;

    if (!pages) {
        return null;
    }

    const candidates = Object.values(pages)
        .map(page => mapWikimediaPageToImageCandidate(page))
        .filter(Boolean)
        .filter(candidate => !isBadImageTitle(candidate.title))
        .sort((a, b) => b.width - a.width);

    return candidates[0] ?? null;
}

function mapWikimediaPageToImageCandidate(page) {
    const imageInfo = page?.imageinfo?.[0];

    if (!imageInfo) {
        return null;
    }

    const mime = imageInfo.mime;

    if (mime !== "image/jpeg" && mime !== "image/png" && mime !== "image/webp") {
        return null;
    }

    const url = imageInfo.thumburl || imageInfo.url;

    if (!url) {
        return null;
    }

    return {
        title: page.title ?? "",
        url,
        width: imageInfo.thumbwidth || imageInfo.width || 0,
        height: imageInfo.thumbheight || imageInfo.height || 0,
        sourcePageUrl: buildCommonsFilePageUrl(page.title ?? ""),
        provider: "Wikimedia Commons"
    };
}

function isBadImageTitle(title) {
    const lowerTitle = title.toLowerCase();

    return lowerTitle.includes("logo") ||
        lowerTitle.includes("map") ||
        lowerTitle.includes("icon") ||
        lowerTitle.includes("diagram") ||
        lowerTitle.includes("floor plan") ||
        lowerTitle.includes("locator") ||
        lowerTitle.includes("symbol");
}

function buildCommonsFilePageUrl(title) {
    const fileName = title.replace(/^File:/i, "");

    return `https://commons.wikimedia.org/wiki/File:${encodeURIComponent(fileName)}`;
}

function getCachedAttractionImage(attractionId) {
    try {
        const key = `${ATTRACTION_IMAGE_CACHE_PREFIX}${attractionId}`;
        const json = localStorage.getItem(key);

        if (!json) {
            return null;
        }

        return JSON.parse(json);
    }
    catch {
        return null;
    }
}

function cacheAttractionImage(attractionId, image) {
    try {
        const key = `${ATTRACTION_IMAGE_CACHE_PREFIX}${attractionId}`;

        localStorage.setItem(key, JSON.stringify({
            url: image.url,
            sourcePageUrl: image.sourcePageUrl,
            provider: image.provider,
            cachedAt: new Date().toISOString()
        }));
    }
    catch (error) {
        console.warn("Unable to cache attraction image:", error);
    }
}