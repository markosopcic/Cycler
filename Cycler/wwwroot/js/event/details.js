﻿var viewer = null;
var users = {}
var names = {}
var connection = null;
var trackedUser = null;

$(document).ready(function(){
    Cesium.Ion.defaultAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiODg4MzNhYi00NGIyLTQ4NTctODljOC1kNTZjY2FkMWY0YjEiLCJpZCI6MTAxMjcsInNjb3BlcyI6WyJhc2wiLCJhc3IiLCJhc3ciLCJnYyJdLCJpYXQiOjE1NTk2NjMyMTZ9.-XK9KKFK9zYS9FiwpptglIjGNV9cXlR28LDcixQJG8k';
    viewer = new Cesium.Viewer("cesiumContainer", {
        terrainProvider: Cesium.createWorldTerrain(),
        infoBox: false,
        selectionIndicator: false,
        shadows: true,
        shouldAnimate: true
    });

    connection = new signalR.HubConnectionBuilder().withUrl("/locationHub").build();

    connection.on("Position", function (name,id, longitude, latitude) {
        if (users[id] === undefined || users[id] === null) {
            try {
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