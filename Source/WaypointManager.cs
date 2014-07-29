using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FinePrint
{
	public class WaypointManager : MonoBehaviour
	{
		private static Texture2D mTexDefault;
		private static Texture2D mTexPlane;
		private static Texture2D mTexRover;
		private List<Waypoint> waypoints;
		private GUIStyle hoverStyle;
        static private NavWaypoint navWaypoint;
        static private Waypoint trackWP;

		public WaypointManager()
		{
			waypoints = new List<Waypoint>();
			mTexDefault = GameDatabase.Instance.GetTexture("FinePrint/Textures/default", false);
			mTexPlane = GameDatabase.Instance.GetTexture("FinePrint/Textures/plane", false);
			mTexRover = GameDatabase.Instance.GetTexture("FinePrint/Textures/rover", false);
			hoverStyle = new GUIStyle();
			hoverStyle.padding = new RectOffset(0, 0, 0, 0);
			hoverStyle.stretchWidth = true;
			hoverStyle.margin = new RectOffset(0, 0, 0, 0);
			hoverStyle.alignment = TextAnchor.MiddleLeft;
			hoverStyle.fontStyle = FontStyle.Bold;
			hoverStyle.normal.textColor = XKCDColors.ElectricLime;

			if (mTexDefault == null)
			{
				mTexDefault = new Texture2D(16, 16);
				mTexDefault.SetPixels32(Enumerable.Repeat((Color32)Color.magenta, 16 * 16).ToArray());
				mTexDefault.Apply();
			}

			if (mTexPlane == null)
			{
				mTexPlane = new Texture2D(16, 16);
				mTexPlane.SetPixels32(Enumerable.Repeat((Color32)Color.magenta, 16 * 16).ToArray());
				mTexPlane.Apply();
			}

			if (mTexRover == null)
			{
				mTexRover = new Texture2D(16, 16);
				mTexRover.SetPixels32(Enumerable.Repeat((Color32)Color.magenta, 16 * 16).ToArray());
				mTexRover.Apply();
			}
		}

		private bool IsOccluded(Vector3d loc, CelestialBody body)
		{
			Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

			if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
				return false;

			return true;
		}

		public static WaypointManager Instance()
		{
			return MapView.MapCamera.gameObject.GetComponent<WaypointManager>();
		}

		//Add a waypoint and handle attaching automatically.
		public static void AddWaypoint(Waypoint wp)
		{
			WaypointManager me = MapView.MapCamera.gameObject.GetComponent<WaypointManager>();

			if (me)
				me.waypoints.Add(wp);
			else
			{
				me = MapView.MapCamera.gameObject.AddComponent<WaypointManager>();
				me.waypoints.Add(wp);
			}
		}

		//Remove a waypoint and handle detaching automatically.
		public static void RemoveWaypoint(Waypoint wp)
		{
			WaypointManager me = MapView.MapCamera.gameObject.GetComponent<WaypointManager>();

			if (me)
			{
				me.waypoints.Remove(wp);

				if (me.waypoints.Count == 0)
					Destroy(me);
			}
		}

		public void UpdateWaypoint(Waypoint wp)
		{
			if (MapView.MapIsEnabled)
			{
				foreach (CelestialBody body in FlightGlobals.Bodies)
				{
					if (body.GetName() == wp.celestialName)
					{
						if (body.pqsController != null)
						{
							Vector3d pqsRadialVector = QuaternionD.AngleAxis(wp.longitude, Vector3d.down) * QuaternionD.AngleAxis(wp.latitude, Vector3d.forward) * Vector3d.right;
							wp.height = body.pqsController.GetSurfaceHeight(pqsRadialVector) - body.pqsController.radius;

							if (wp.height < 0)
								wp.height = 0;
						}

						Vector3d surfacePos = body.GetWorldSurfacePosition(wp.latitude, wp.longitude, wp.height + wp.altitude);
						Vector3d scaledPos = ScaledSpace.LocalToScaledSpace(surfacePos);
						wp.worldPosition = new Vector3((float)scaledPos.x, (float)scaledPos.y, (float)scaledPos.z);
						wp.isOccluded = IsOccluded(surfacePos, body);
					}
				}
			}
		}

		public void OnPreCull()
		{
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (navIsActive())
                {
                    if (trackWP == null)
                        navWaypoint.Deactivate();
                }
            }

			if (MapView.MapIsEnabled)
			{
				foreach (Waypoint wp in waypoints)
					UpdateWaypoint(wp);
			}
		}

		public void OnGUI()
		{
			if (MapView.MapIsEnabled)
			{
				//Simple flag that restricts to one tooltip even if you are hovering over more than one site.
				bool showingTooltip = false;
				Vector3 tooltipPosition = new Vector3();
				string tooltipString = "";

				foreach (Waypoint wp in waypoints)
				{
					MapObject target = PlanetariumCamera.fetch.target;

					//Don't show waypoints on Jool if I'm looking at Kerbin.
					switch (target.type)
					{
						case MapObject.MapObjectType.CELESTIALBODY:
							if (target.celestialBody.GetName() != wp.celestialName)
								continue;
							break;
						case MapObject.MapObjectType.MANEUVERNODE:
							if (target.maneuverNode.patch.referenceBody.GetName() != wp.celestialName)
								continue;
							break;
						case MapObject.MapObjectType.VESSEL:
							if (target.vessel.mainBody.GetName() != wp.celestialName)
								continue;
							break;
						default:
							continue;
					}

					//Check if the waypoint is off camera.
					if (MapView.MapCamera.transform.InverseTransformPoint(wp.worldPosition).z < 0f)
						continue;

					Vector3 pos = MapView.MapCamera.camera.WorldToScreenPoint(wp.worldPosition);
					Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
					float alpha = 1.0f;

					if (wp.isOccluded)
						alpha = 0.1f;
					else
						alpha = 0.6f;

					switch (wp.textureName)
					{
						case "plane":
							Graphics.DrawTexture(screenRect, mTexPlane, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, RandomColor(wp.seed, alpha));
							break;
						case "rover":
							Graphics.DrawTexture(screenRect, mTexRover, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, RandomColor(wp.seed, alpha));
							break;
						default:
							Graphics.DrawTexture(screenRect, mTexDefault, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, RandomColor(wp.seed, alpha));
							break;
					}

					// Label the waypoint.
					if (screenRect.Contains(Event.current.mousePosition) && !wp.isOccluded && !showingTooltip)
					{
						//Storing the information so we can render the label after the loop, text should overlap any other waypoints.
						tooltipPosition = pos;

						if (wp.isClustered)
							tooltipString = wp.siteName + " " + Util.integerToGreek(wp.id);
						else
							tooltipString = wp.siteName;

						showingTooltip = true;

                        if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
                        {
                            ScreenMessages.PostScreenMessage("Navigation set to " + wp.siteName + ".", 2.5f, ScreenMessageStyle.LOWER_CENTER);
                            setupNavPoint(wp);
                            activateNavPoint();
                        }
					}
				}

				if (showingTooltip)
					GUI.Label(new Rect((float)(tooltipPosition.x) + 16, (float)(Screen.height - tooltipPosition.y) + 5f, 50, 20), tooltipString, hoverStyle);
			}
		}

        static public void setupNavPoint(Waypoint wp)
        {
            if ( HighLogic.LoadedSceneIsFlight )
            {
                if (navWaypoint == null)
                    navWaypoint = (GameObject.FindObjectOfType(typeof(NavWaypoint)) as NavWaypoint);

                if (navWaypoint != null)
                {
                    CelestialBody body = Planetarium.fetch.Home;

					    foreach (CelestialBody cb in FlightGlobals.Bodies)
					    {
						    if (cb.GetName() == wp.celestialName)
							    body = cb;
					    }

                        HSBColor brighterRandom = HSBColor.FromColor(RandomColor(wp.seed));
                        brighterRandom.b = 1.0f;
                        navWaypoint.SetupNavWaypoint(body, wp.latitude, wp.longitude, wp.altitude, wp.textureName, brighterRandom.ToColor());
                        trackWP = wp;
                }
            }
        }

        static public void activateNavPoint()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (navWaypoint == null)
                    navWaypoint = (GameObject.FindObjectOfType(typeof(NavWaypoint)) as NavWaypoint);

                if (navWaypoint != null)
                {
                    navWaypoint.Activate();
                }
            }
        }

        static public void deactivateNavPoint()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (navWaypoint == null)
                    navWaypoint = (GameObject.FindObjectOfType(typeof(NavWaypoint)) as NavWaypoint);

                if (navWaypoint != null)
                {
                    navWaypoint.Deactivate();
                }
            }
        }

        static public bool navIsActive()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (navWaypoint == null)
                    navWaypoint = (GameObject.FindObjectOfType(typeof(NavWaypoint)) as NavWaypoint);

                if (navWaypoint != null)
                {
                    return navWaypoint.isActive();
                }
            }

            return false;
        }

		public List<Waypoint> WaypointsNearVessel(float distance)
		{
			List<Waypoint> nearPoints = new List<Waypoint>();

			if (HighLogic.LoadedSceneIsFlight)
			{
				foreach (Waypoint wp in waypoints)
				{
					if (DistanceToVessel(wp) < distance)
						nearPoints.Add(wp);
				}
			}

			return nearPoints;
		}

		public List<Waypoint> AllWaypoints()
		{
			return waypoints;
		}

		public float DistanceToVessel(Waypoint wp)
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				Vessel v = FlightGlobals.ActiveVessel;

				if (v.mainBody.GetName() == wp.celestialName)
					return Distance(v.latitude, v.longitude, v.altitude, wp.latitude, wp.longitude, wp.altitude, v.mainBody);
				else
					return float.PositiveInfinity;
			}
			else
				return float.PositiveInfinity;
		}

		public float Distance(double latitude1, double longitude1, double altitude1, double latitude2, double longitude2, double altitude2, CelestialBody body)
		{
			// I did use great circle distance, but now I'm actually thinking a straight line might be best.
			Vector3d position1 = body.GetWorldSurfacePosition(latitude1, longitude1, altitude1);
			Vector3d position2 = body.GetWorldSurfacePosition(latitude2, longitude2, altitude2);
			return (float)Vector3d.Distance(position1, position2);
		}

		public static UnityEngine.Color RandomColor(int seed)
		{
			System.Random generator = new System.Random(seed);
			HSBColor color;
			color.h = (float)generator.NextDouble();
			color.s = 1.0f;
			color.b = 0.5f;
			color.a = 1.0f;
			return color.ToColor();
		}

		public static UnityEngine.Color RandomColor(int seed, float alpha)
		{
			System.Random generator = new System.Random(seed);
			HSBColor color;
			color.h = (float)generator.NextDouble();
			color.s = 1.0f;
			color.b = 0.5f;
			color.a = alpha;
			return color.ToColor();
		}

		public static void ChooseRandomPosition(out double latitude, out double longitude, string celestialName, bool waterAllowed = true)
		{
			latitude = 0.0;
			longitude = 0.0;
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
	}
}
