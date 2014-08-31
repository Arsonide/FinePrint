/*using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using KSP;
using KSPAchievements;
using FinePrint.Contracts.Parameters;

namespace FinePrint
{
    class WorldObject
    {
        private PQSCity PQSCityComponent;
        private PQSCity.LODRange PQSCityLOD;
        private GameObject WorldGameObject;
        private GameObject WorldModel;
        private CelestialBody ParentBody;
        private double Latitude;
        private double Longitude;
        private double Altitude;
        private float Angle;
        private float VisRange;

        public WorldObject(double latitude, double longitude, double altitude, float angle, float visibleRange)
        {
            this.ParentBody = Planetarium.fetch.Home;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Angle = angle;
            this.VisRange = visibleRange;

            WorldGameObject = new GameObject();
            WorldGameObject.name = "WorldGameObject";
            WorldModel = GameDatabase.Instance.GetModel("FinePrint/Models/model");
            WorldModel.transform.parent = WorldGameObject.transform;
            WorldGameObject.transform.parent = ParentBody.transform;
            PQSCityComponent = WorldGameObject.AddComponent<PQSCity>();
            PQSCityComponent.repositionToSphere = true;
            PQSCityComponent.repositionToSphereSurface = true;
            PQSCityComponent.reorientToSphere = true;
            PQSCityComponent.sphere = ParentBody.pqsController;
            PQSCityComponent.frameDelta = 1;
            PQSCityComponent.modEnabled = true;
            PQSCityLOD = new PQSCity.LODRange();
            PQSCityLOD.objects = new GameObject[0];
            PQSCityLOD.visibleRange = VisRange;
            PQSCityComponent.lod = new PQSCity.LODRange[] { PQSCityLOD };
            PQSCityComponent.lod[0].renderers = new GameObject[1];
            PQSCityComponent.lod[0].renderers[0] = WorldModel;

            SetScale(1f);
            SetColor(Color.red);

            PQSCityLOD.Setup();
            PQSCityComponent.OnSetup();
            Reorientate();
        }
                
        public void Reorientate()
        {
            if (PQSCityComponent == null)
                return;

            PQSCityComponent.repositionRadial = getRadialPosition();
            PQSCityComponent.repositionRadiusOffset = Altitude;
            PQSCityComponent.reorientFinalAngle = Angle;

            Vector3 initialUp = ParentBody.GetSurfaceNVector(Latitude, Longitude);
            Quaternion rotation = Quaternion.Euler(0, -90, 0);
            initialUp = rotation * initialUp;
            PQSCityComponent.reorientInitialUp = initialUp;

            PQSCityComponent.Orientate();
        }

        public void SetScale(float scale)
        {
            WorldModel.transform.localScale = new Vector3(scale, scale, scale);
        }

        public void SetColor(Color color)
        {
            WorldModel.renderer.material.SetColor("_Color", color);
        }

        public void Update()
        {
            PQSCityComponent.OnUpdateFinished();
        }

        private Vector3 getRadialPosition()
        {
            return Util.LLAtoECEF(Latitude, Longitude, Altitude, ParentBody.Radius);
        }
    }
}
*/