// --- admin.js ---

// 1. GLOBAL DEĞİŞKENLER VE AYARLAR
let mapFix, markerFix, searchBox;
let mapRoute, dirService, dirRenderer;
let MapLib, MarkerLib, PlacesLib, RouteLib;
let currentLat = 37.2154, currentLng = 28.3636;

// 2. GOOGLE MAPS BOOTSTRAP LOADER (YENİ SİSTEM)
(g => { var h, a, k, p = "The Google Maps JavaScript API", c = "google", l = "importLibrary", q = "__ib__", m = document, b = window; b = b[c] || (b[c] = {}); var d = b.maps || (b.maps = {}); var r = new Set, e = new URLSearchParams, u = () => h || (h = new Promise(async (f, n) => { await (a = m.createElement("script")); e.set("libraries", [...r] + ""); for (k in g) e.set(k.replace(/[A-Z]/g, t => "_" + t[0].toLowerCase()), g[k]); e.set("callback", c + ".maps." + q); a.src = `https://maps.googleapis.com/maps/api/js?` + e; d[q] = f; a.onerror = () => h = n(Error(p + " could not load.")); a.nonce = m.querySelector("script[nonce]")?.nonce || ""; m.head.append(a) })); d[l] ? console.warn(p + " only loads once. Ignoring:", g) : d[l] = (f, ...n) => r.add(f) && u().then(() => d[l](f, ...n)) })({
    key: "AIzaSyD64r0VElvouQ6l9Z_L53QmJIC8tMTmcvM", // API KEY BURADA
    v: "weekly",
});

// 3. SAYFA YÜKLENDİĞİNDE ÇALIŞACAKLAR
document.addEventListener("DOMContentLoaded", function () {
    // Hash kontrolü (Sayfa yenilenince doğru sekmede kalmak için)
    var hash = window.location.hash.substring(1);
    showSection(hash || 'dashboard');

    // Chart.js Grafikleri (Veriyi window.deliveryData'dan alıyoruz)
    initCharts();

    // Google Maps Kütüphanelerini Başlat
    initGoogleLibraries();

    // Harita Modalı Event Listener
    setupMapModal();
});

// --- YARDIMCI FONKSİYONLAR ---

function showSection(id) {
    document.querySelectorAll('section').forEach(s => s.style.display = 'none');
    document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
    document.getElementById(id).style.display = 'block';
    document.getElementById('link-' + id).classList.add('active');
}

function toggleForm(id) {
    var el = document.getElementById(id);
    el.style.display = (el.style.display === 'none') ? 'block' : 'none';
}

function initCharts() {
    // Index.cshtml'den gelen veri var mı kontrol et
    if (window.deliveryData) {
        var types = {}, stats = {};
        window.deliveryData.forEach(d => {
            types[d.atikTuru] = (types[d.atikTuru] || 0) + d.miktarKG;
            stats[d.durum] = (stats[d.durum] || 0) + 1;
        });

        var clr = ['#3b82f6', '#eab308', '#10b981', '#ef4444', '#8b5cf6'];

        // Grafik elementleri var mı kontrol et (Hata almamak için)
        if (document.getElementById('atikChart')) {
            new Chart(document.getElementById('atikChart'), { type: 'doughnut', data: { labels: Object.keys(types), datasets: [{ data: Object.values(types), backgroundColor: clr }] } });
        }
        if (document.getElementById('durumChart')) {
            new Chart(document.getElementById('durumChart'), { type: 'polarArea', data: { labels: Object.keys(stats), datasets: [{ data: Object.values(stats), backgroundColor: clr }] } });
        }
    }
}

// --- GOOGLE MAPS FONKSİYONLARI ---

async function initGoogleLibraries() {
    MapLib = await google.maps.importLibrary("maps");
    MarkerLib = await google.maps.importLibrary("marker");
    PlacesLib = await google.maps.importLibrary("places");
    RouteLib = await google.maps.importLibrary("routes");
}

function setupMapModal() {
    var modalEl = document.getElementById('modalMap');
    if (modalEl) {
        modalEl.addEventListener('shown.bs.modal', async function () {
            if (!MapLib) await initGoogleLibraries();

            var center = { lat: currentLat, lng: currentLng };

            if (!mapFix) {
                mapFix = new MapLib.Map(document.getElementById("mapFix"), {
                    center: center,
                    zoom: 15,
                    mapId: "DEMO_MAP_ID"
                });

                markerFix = new MarkerLib.AdvancedMarkerElement({
                    map: mapFix,
                    position: center,
                    title: "Konum"
                });

                // Arama Kutusu
                var input = document.getElementById("pac-input");
                searchBox = new PlacesLib.SearchBox(input);
                mapFix.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

                searchBox.addListener("places_changed", () => {
                    var places = searchBox.getPlaces();
                    if (places.length == 0) return;
                    var bounds = new google.maps.LatLngBounds();
                    places.forEach((place) => {
                        if (!place.geometry) return;
                        if (place.geometry.viewport) bounds.union(place.geometry.viewport); else bounds.extend(place.geometry.location);
                        markerFix.position = place.geometry.location;
                    });
                    mapFix.fitBounds(bounds);
                });

                mapFix.addListener("click", (e) => {
                    markerFix.position = e.latLng;
                });
            } else {
                mapFix.setCenter(center);
                markerFix.position = center;
                google.maps.event.trigger(mapFix, "resize");
            }
        });
    }
}

function openMap(btnElement) {
    var id = btnElement.getAttribute('data-id');
    var latStr = btnElement.getAttribute('data-lat');
    var lngStr = btnElement.getAttribute('data-lng');

    document.getElementById('fixId').value = id;

    if (!latStr || latStr.trim() === "") latStr = "37.2154";
    if (!lngStr || lngStr.trim() === "") lngStr = "28.3636";

    currentLat = parseFloat(latStr);
    currentLng = parseFloat(lngStr);

    new bootstrap.Modal(document.getElementById('modalMap')).show();
}

function saveLoc() {
    var pos = markerFix.position;
    var lat = (typeof pos.lat === 'function') ? pos.lat() : pos.lat;
    var lng = (typeof pos.lng === 'function') ? pos.lng() : pos.lng;

    var latStr = lat.toString().replace('.', ',');
    var lngStr = lng.toString().replace('.', ',');

    $.post('/Admin/UpdateDeliveryLocation', { id: document.getElementById('fixId').value, lat: latStr, lng: lngStr }, function () { location.reload(); });
}

function showRoute(id) {
    new bootstrap.Modal(document.getElementById('modalRoute')).show();

    setTimeout(async function () {
        if (!RouteLib) await initGoogleLibraries();

        var mapElement = document.getElementById("mapRoute");

        if (!dirService) dirService = new RouteLib.DirectionsService();
        if (!dirRenderer) dirRenderer = new RouteLib.DirectionsRenderer();

        if (!mapRoute) {
            mapRoute = new MapLib.Map(mapElement, {
                zoom: 10,
                center: { lat: 37.2154, lng: 28.3636 },
                mapId: "DEMO_MAP_ID"
            });
            dirRenderer.setMap(mapRoute);
        }

        $.get('/Admin/GetRouteDetails?id=' + id, function (r) {
            if (r.success && r.data.length > 0) {
                var waypts = [];
                r.data.forEach(p => { if (p.enlem != 0) waypts.push({ location: new google.maps.LatLng(p.enlem, p.boylam), stopover: true }); });
                if (waypts.length > 0) {
                    dirService.route({
                        origin: waypts[0].location, destination: waypts[waypts.length - 1].location,
                        waypoints: waypts.slice(1, -1), optimizeWaypoints: true, travelMode: google.maps.TravelMode.DRIVING,
                    }, (response, status) => { if (status === "OK") dirRenderer.setDirections(response); });
                }
            }
        });
    }, 500);
}

function editDel(id, s) {
    document.getElementById('editId').value = id;
    document.getElementById('editStat').value = s;
    new bootstrap.Modal(document.getElementById('modalEdit')).show();
}