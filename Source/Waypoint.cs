using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FinePrint
{
    public class Waypoint
    {
        public string celestialName;
        public double latitude;
        public double longitude;
        public double height;
        public bool isExplored;
        public Vector3 worldPosition;
        public int seed;
        public string textureName = "default";
        public double altitude = 0;
        public bool isOccluded = false;
        public string siteName = "Site";
        public int id;
        public bool isClustered = false;

        public Waypoint()
        {

        }

        public void setName(bool uniqueSites = true)
        {
            if (uniqueSites)
            {
                if (celestialName == "Kerbin")
                    this.siteName = Util.generateSiteName(seed + id, true);
                else
                    this.siteName = Util.generateSiteName(seed + id, false);
            }
            else
            {
                if (celestialName == "Kerbin")
                    this.siteName = Util.generateSiteName(seed, true);
                else
                    this.siteName = Util.generateSiteName(seed, false);
            }
        }

        public void RandomizePosition(bool waterAllowed = true)
        {
            CelestialBody myPlanet = null;

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.GetName() == celestialName)
                    myPlanet = body;
            }

            if (myPlanet != null)
            {
                if (myPlanet.ocean)
                {
                    if (myPlanet.pqsController != null)
                    {
                        while (true)
                        {
                            latitude = UnityEngine.Random.value * 180 - 90;
                            longitude = UnityEngine.Random.value * 360 - 180;
                            Vector3d pqsRadialVector = QuaternionD.AngleAxis(longitude, Vector3d.down) * QuaternionD.AngleAxis(latitude, Vector3d.forward) * Vector3d.right;
                            double chosenHeight = myPlanet.pqsController.GetSurfaceHeight(pqsRadialVector) - myPlanet.pqsController.radius;

                            if (chosenHeight < 0)
                                continue;
                            else
                                break;
                        }
                    }
                }
                else
                {
                    latitude = UnityEngine.Random.value * 180 - 90;
                    longitude = UnityEngine.Random.value * 360 - 180;
                }
            }
        }

        public void RandomizeNearWaypoint(Waypoint center, double searchRadius, bool waterAllowed = true)
        {
            RandomizeNear(center.latitude, center.longitude, center.celestialName, searchRadius, waterAllowed);
        }

        public void RandomizeNear(double centerLatitude, double centerLongitude, string celestialName, double searchRadius, bool waterAllowed = true)
        {
            CelestialBody myPlanet = null;

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.GetName() == celestialName)
                {
                    myPlanet = body;
                }
            }

            if (myPlanet != null)
            {
                if (myPlanet.ocean)
                {
                    if (myPlanet.pqsController != null)
                    {
                        while (true)
                        {
                            double distancePerDegree = (myPlanet.Radius * 2 * UnityEngine.Mathf.PI) / 360.0;
                            double lng_min = centerLongitude - searchRadius / UnityEngine.Mathf.Abs(UnityEngine.Mathf.Cos(UnityEngine.Mathf.Deg2Rad * (float)centerLatitude) * (float)distancePerDegree);
                            double lng_max = centerLongitude + searchRadius / UnityEngine.Mathf.Abs(UnityEngine.Mathf.Cos(UnityEngine.Mathf.Deg2Rad * (float)centerLatitude) * (float)distancePerDegree);
                            double lat_min = centerLatitude - (searchRadius / distancePerDegree);
                            double lat_max = centerLatitude + (searchRadius / distancePerDegree);
                            latitude = UnityEngine.Random.Range((float)lat_min, (float)lat_max);
                            longitude = UnityEngine.Random.Range((float)lng_min, (float)lng_max);
                            Vector3d pqsRadialVector = QuaternionD.AngleAxis(longitude, Vector3d.down) * QuaternionD.AngleAxis(latitude, Vector3d.forward) * Vector3d.right;
                            double chosenHeight = myPlanet.pqsController.GetSurfaceHeight(pqsRadialVector) - myPlanet.pqsController.radius;

                            if (chosenHeight < 0)
                                continue;
                            else
                                break;
                        }
                    }
                }
                else
                {
                    double distancePerDegree = (myPlanet.Radius * 2 * UnityEngine.Mathf.PI) / 360.0;
                    double lng_min = centerLongitude - searchRadius / UnityEngine.Mathf.Abs(UnityEngine.Mathf.Cos(UnityEngine.Mathf.Deg2Rad * (float)centerLatitude) * (float)distancePerDegree);
                    double lng_max = centerLongitude + searchRadius / UnityEngine.Mathf.Abs(UnityEngine.Mathf.Cos(UnityEngine.Mathf.Deg2Rad * (float)centerLatitude) * (float)distancePerDegree);
                    double lat_min = centerLatitude - (searchRadius / distancePerDegree);
                    double lat_max = centerLatitude + (searchRadius / distancePerDegree);
                    latitude = UnityEngine.Random.Range((float)lat_min, (float)lat_max);
                    longitude = UnityEngine.Random.Range((float)lng_min, (float)lng_max);
                }
            }
        }
    }
}