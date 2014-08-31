using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FinePrint
{
    public enum WaypointType
    {
        NONE,
        ORBITAL,
        ASCENDINGNODE,
        DESCENDINGNODE,
        APOAPSIS,
        PERIAPSIS,
        PLANE,
        ROVER,
        DISH
    }

	public class Waypoint
	{
		public string celestialName;
		public double latitude;
		public double longitude;
		public double height;
		public bool isExplored;
		public Vector3d worldPosition;
        public Vector3 orbitPosition;
		public int seed;
		public double altitude = 0;
		public bool isOccluded = false;
		public string tooltip = "Site";
		public int id;
		public bool isClustered = false;
        public bool isOnSurface = false;
        public bool isNavigatable = false;
        public bool visible = true;

        public WaypointType waypointType = WaypointType.NONE;

		public Waypoint()
		{

		}

		public void setName(bool uniqueSites = true)
		{
            CelestialBody myPlanet = null;

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.GetName() == celestialName)
                    myPlanet = body;
            }

            if (myPlanet == null)
                return;

			if (uniqueSites)
			{
				if (myPlanet == Planetarium.fetch.Home)
					this.tooltip = Util.generateSiteName(seed + id, true);
				else
					this.tooltip = Util.generateSiteName(seed + id, false);
			}
			else
			{
                if (myPlanet == Planetarium.fetch.Home)
					this.tooltip = Util.generateSiteName(seed, true);
				else
					this.tooltip = Util.generateSiteName(seed, false);
			}
		}

		public void RandomizePosition(bool waterAllowed = true)
		{
            System.Random generator = new System.Random(seed + id);

			CelestialBody myPlanet = null;

			foreach (CelestialBody body in FlightGlobals.Bodies)
			{
				if (body.GetName() == celestialName)
					myPlanet = body;
			}

			if (myPlanet != null)
			{
				if (myPlanet.ocean && !waterAllowed )
				{
					if (myPlanet.pqsController != null)
					{
						while (true)
						{
                            double rand = generator.NextDouble();
                            rand = 1.0 - (rand * 2);
                            latitude = Math.Asin(rand) * UnityEngine.Mathf.Rad2Deg;
                            longitude = generator.NextDouble() * 360 - 180;
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
                    double rand = generator.NextDouble();
                    rand = 1.0 - (rand * 2);
                    latitude = Math.Asin(rand) * UnityEngine.Mathf.Rad2Deg;
                    longitude = generator.NextDouble() * 360 - 180;
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
            System.Random generator = new System.Random(this.seed + this.id);

			foreach (CelestialBody body in FlightGlobals.Bodies)
			{
				if (body.GetName() == celestialName)
				{
					myPlanet = body;
				}
			}

			if (myPlanet != null)
			{
				if (myPlanet.ocean && !waterAllowed)
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
                            latitude = lat_min + generator.NextDouble() * (lat_max - lat_min);
                            longitude = lng_min + generator.NextDouble() * (lng_max - lng_min);
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
                    latitude = lat_min + generator.NextDouble() * (lat_max - lat_min);
                    longitude = lng_min + generator.NextDouble() * (lng_max - lng_min);
				}
			}
		}
	}
}