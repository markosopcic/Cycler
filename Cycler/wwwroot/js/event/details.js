﻿var viewer = null;
var users = {}
var names = {}
var userTracked = {}
var connection = null;
var trackedUser = null;

$(document).ready(function(){
    if($("#cesiumContainer").length == 0) return;
    
    $("#load-data-button").click(function(){
        loadHistoricalData()
    })
    $("#users-select").select2({
        multiple : true,
        placeholder : "Select users..."
    })
    
    Cesium.Ion.defaultAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiODg4MzNhYi00NGIyLTQ4NTctODljOC1kNTZjY2FkMWY0YjEiLCJpZCI6MTAxMjcsInNjb3BlcyI6WyJhc2wiLCJhc3IiLCJhc3ciLCJnYyJdLCJpYXQiOjE1NTk2NjMyMTZ9.-XK9KKFK9zYS9FiwpptglIjGNV9cXlR28LDcixQJG8k';
    viewer = new Cesium.Viewer("cesiumContainer", {
        terrainProvider: Cesium.createWorldTerrain(),
        infoBox: false,
        selectionIndicator: false,
        shadows: true,
        shouldAnimate: true
    });

    $("#user-search").on("input", function (e) {
        if (e.target.value.length === 0) {
            $(".user").show();
            return;
        }
        var users = $(".user");
        for (var i = 0; i < users.length; i++) {
            if (users[i].innerText.toLowerCase().includes(e.target.value.toLowerCase())) {
                $(users[i]).show();
            } else {
                $(users[i]).hide();
            }
        }
    });

    connection = new signalR.HubConnectionBuilder().withUrl("/locationHub").build();

    connection.on("Position", function (name,id, latitude, longitude) {
        if (users[id] === undefined || users[id] === null && (userTracked[id] === true || userTracked[id] === undefined || userTracked[id] === null)) {
            try {
                addToSearchBar(name,id)
                var entity = viewer.entities.add({
                    position: Cesium.Cartesian3.fromDegrees(latitude, longitude),
                    model: {
                        uri: "/js/Cesium_Man.js",
                        minimumPixelSize: 64,
                        maximumScale: 20000

                    }

                });
                var promise = Cesium.sampleTerrainMostDetailed(viewer.terrainProvider, [Cesium.Cartographic.fromDegrees(latitude, longitude)]);
                Cesium.when(promise, function (position) {
                    position = position[0];
                    entity.position = Cesium.Cartesian3.fromDegrees(latitude, longitude, position.height);

                });
                var lab = {
                    text: name,
                    font: 'bold 24px Segoe UI Semibold',
                    fillColor: Cesium.Color.SKYBLUE,
                    outlineColor: Cesium.Color.BLACK,
                    outlineWidth: 2,
                    style: Cesium.LabelStyle.FILL_AND_OUTLINE,
                    verticalOrigin: Cesium.VerticalOrigin.CENTER,
                    disableDepthTestDistance: Number.POSITIVE_INFINITY,
                    scale: 0.8,
                    scaleByDistance : new Cesium.NearFarScalar(3, 2.0, 3, 0.5)
                };
                entity.label = new Cesium.LabelGraphics(lab);
                users[id] = entity;
            } catch (err) {
                console.log(err);
            }
        } else {
            if (trackedUser === null && users[id]!== undefined) {
                trackedUser = name;
                users[id].viewFrom = new Cesium.Cartesian3(-90, 0, 70);
                viewer.flyTo(users[id]).then(e => { viewer.trackedEntity = users[id]; });
                return true;
            }
            promise = Cesium.sampleTerrainMostDetailed(viewer.terrainProvider, [Cesium.Cartographic.fromDegrees(latitude, longitude)]);
            Cesium.when(promise, function (position) {
                var model = users[id];
                position = position[0];
                model.position = Cesium.Cartesian3.fromDegrees(latitude, longitude, position.height);
            });
        }

    });

     connection.start().then(() => {
         var eventId = $("#event-id-input")[0].value;
         connection.invoke("AddNewFollowing", eventId);
     });

})


function toRadians(degrees) {
    return degrees * Math.PI / 180;
};

// Converts from radians to degrees.
function toDegrees(radians) {
    return radians * 180 / Math.PI;
}


function bearing(startLat, startLng, destLat, destLng){
    startLat = toRadians(startLat);
    startLng = toRadians(startLng);
    destLat = toRadians(destLat);
    destLng = toRadians(destLng);

    y = Math.sin(destLng - startLng) * Math.cos(destLat);
    x = Math.cos(startLat) * Math.sin(destLat) -
        Math.sin(startLat) * Math.cos(destLat) * Math.cos(destLng - startLng);
    brng = Math.atan2(y, x);
    brng = toDegrees(brng);
    return (brng + 360) % 360;
}

function addToSearchBar(name,id){
    var exists = $("div[value='"+id+"']").length;
    if(exists) return;
    var newElement = document.createElement("div");
    newElement.className = "form-control user";
    newElement.style = "outline-style: none;";
    newElement.innerText = name;
    newElement.setAttribute("value", id);
    newElement.onclick = function (event) { userClicked(event); };
    $("#active-users")[0].appendChild(newElement);
    var newButton = document.createElement("button");
    newButton.className = "btn btn-info";
    newButton.style = "float:right;height:100%;display: inline-flex;text-align: center;font-size:x-small;z-index:999999";
    newButton.innerText = "Focus";
    newButton.onclick = function (event) { onUserFocused(event, viewer, users, id); };
    newElement.appendChild(newButton);
    newElement.click();
}

function userClicked(event) {
    var senderElementName = event.target.getAttribute("value");
    if (senderElementName === null || senderElementName === "button") {
        return;
    }
    senderElementName = senderElementName.toLowerCase();
    if ($(event.target).attr("selected") !== "selected") {
        $(event.target).attr("selected", true);
        $(event.target).css("background-color", "coral");
    } else {
        $(event.target).attr("selected", false);
        $(event.target).css("background-color", "");
    }
    if (event.target.getAttribute("selected") === "selected") {
        userTracked[senderElementName] = true;
    } else {
        userTracked[senderElementName] = false;
        viewer.entities.remove(users[senderElementName]);
        users[senderElementName] = null;
    }
}

function onUserFocused(event, viewer, users, id) {
    if (users[id] === undefined) return;
    viewer.flyTo(users[id]).then(e => { viewer.trackedEntity = users[id]; });
}

function loadHistoricalData() {
    if ($("#users-select").val().length === 0) {
        alert("Select at least one user!");
        return;
    }

    $("#load-data-button").attr("disabled", true);
    var eventId = $("#event-id-input")[0].value;
    var data = JSON.stringify($("#users-select").val());
    $.ajax({
        method: "POST",
        url: "/api/GetHistoricalData?eventId="+eventId,
        data: data,
        contentType:"application/json",
        success: function (result) { createPaths(result); },
        error: function (result) { alert("Something went wrong!");
            $("#load-data-button").attr("disabled", false);}
    });
}


function createPaths(result) {
    let czml = JSON.parse(JSON.stringify(czmlTemplate));
    let maxTime, minTime = undefined;
    let hasAny = false;
    for (const [key, value] of Object.entries(result)) {
        if (value.length !== 0) {
            hasAny = true;
        }
    }

    if (!hasAny) {
        alert("No stored coordinates have been found for the given input!");
        $("#load-data-button").attr("disabled", false);
        return;
    }
    let maxPaths = 0;

    for (const [key, value] of Object.entries(result)) {
        if (value.length !== 0) {
            maxPaths += 1;
            var date = new Date(value[0].time);
            if (minTime === undefined || minTime > date) {
                minTime = date;
            }
            date = new Date(value[value.length - 1].time);
            if (maxTime === undefined || maxTime < date) {
                maxTime = date;
            }
            console.log(minTime.toISOString());
            console.log(maxTime.toISOString());
        }
    }


    czml[0].clock.interval = minTime.toISOString() + "/" + maxTime.toISOString();
    czml[0].clock.currentTime = minTime.toISOString();
    for (const [key, value] of Object.entries(result)) {
        if (value.length === 0) {
            continue;
        }
        let czmlPath = JSON.parse(JSON.stringify(pathTemplate));
        for (var i = 0; i < 3; i++) {
            czmlPath.path.material.polylineOutline.color.rgba.push(Math.floor(Math.random() * 256));
        }
        czmlPath.id = key;
        czmlPath.path.material.polylineOutline.color.rgba.push(255);
        czmlPath.path.material.polylineOutline.outlineColor.rgba = czmlPath.path.material.polylineOutline.color.rgba;
        czmlPath.availability = minTime.toISOString() + "/" + maxTime.toISOString();
        czmlPath.label.text = $("option[value='"+key+"']")[0].label;
        czmlPath.label.show.interval = minTime.toISOString() + "/" + maxTime.toISOString();
        czmlPath.label.fillColor.interval = minTime.toISOString() + "/" + maxTime.toISOString();
        czmlPath.label.outlineColor = czmlPath.path.material.polylineOutline.color.rgba;
        czmlPath.label.fillColor = czmlPath.path.material.polylineOutline.color.rgba;
        let positions = [];
        for (i = 0; i < value.length; i++) {
            positions.push(Cesium.Cartographic.fromDegrees(value[i].longitude, value[i].latitude));
        }
        let promise = Cesium.sampleTerrainMostDetailed(viewer.terrainProvider, positions).then(function (updated) {
            for (let x = 0; x < updated.length; x++) {
                czmlPath.position.cartographicDegrees.push(new Date(value[x].time).toISOString(), value[x].longitude, value[x].latitude, updated[x].height + 0.2);
            }
            czml.push(czmlPath);
            if (czml.length === maxPaths + 1) {
                viewer.entities.removeAll();
                viewer.dataSources.removeAll();
                $("#load-data-button").attr("disabled", false);
                viewer.dataSources.add(Cesium.CzmlDataSource.load(czml)).then(function (ds) {
                    viewer.zoomTo(ds.entities.getById(key));
                });

            }
        });
    }
}



var pathTemplate = {
    "id": "path",
    "availability": "2012-08-04T10:00:00Z/2012-08-04T15:00:00Z",
    "path": {
        "material": {
            "polylineOutline": {
                "color": {
                    "rgba": []
                },
                "outlineColor": {
                    "rgba": []
                },
                "outlineWidth": 5
            }
        },
        "width": 8,
        "leadTime": 10,
        "trailTime": 1000,
        "resolution": 5
    },
    "label": {
        "minimumPixelSize": 128,
        "maximumScale": 20,
        "fillColor": [
            {
                "interval": "2012-08-04T16:00:00Z/2012-08-04T18:00:00Z",
                "rgba": [
                    255, 255, 0, 255
                ]
            }
        ],
        "font": "bold 10pt Segoe UI Semibold",
        "horizontalOrigin": "CENTER",
        "outlineColor": {
            "rgba": [
                0, 0, 0, 255
            ]
        },
        "pixelOffset": {
            "cartesian2": [0,-50]
        },
        "scale": 1.0,
        "show": [
            {
                "interval": "2012-08-04T16:00:00Z/2012-08-04T18:00:00Z",
                "boolean": true
            }
        ],
        "style": "FILL",
        "verticalOrigin": "CENTER"
    },
    "model": {
        "gltf": "/js/Cesium_Man.js",
        "minimumPixelSize": 64,
        "maximumScale": 20000,
        "scale":1.0
    },
    "orientation": {"velocityReference": "#position" },
    "position": {
        "interpolationAlgorithm": "LAGRANGE",
        "interpolationDegree": 1,
        "cartographicDegrees": []
    }
};

var czmlTemplate = [{
    "id": "document",
    "name": "CZML Path",
    "version": "1.0",
    "clock": {
        "interval": "2012-08-04T10:00:00Z/2012-08-04T15:00:00Z",
        "currentTime": "2012-08-04T10:00:00Z",
        "multiplier": 2
    }
}];