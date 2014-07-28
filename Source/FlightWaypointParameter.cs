using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using KSP;
using KSPAchievements;

namespace FinePrint.Contracts.Parameters
{
	public class FlightWaypointParameter : ContractParameter
	{
		private Waypoint wp;
		private double minAltitude;
		private double maxAltitude;
		private CelestialBody targetBody;
		private int waypointID;
		private bool submittedWaypoint;
		private bool outerWarning;

		// Game freaks out without a default constructor. I never use it.
		public FlightWaypointParameter()
		{
			targetBody = Planetarium.fetch.Home;
			minAltitude = 0.0;
			maxAltitude = 10000.0;
			wp = new Waypoint();
			submittedWaypoint = false;
			outerWarning = false;
		}

		public FlightWaypointParameter(int waypointID, CelestialBody targetBody, double minAltitude, double maxAltitude)
		{
			this.targetBody = targetBody;
			this.waypointID = waypointID;
			this.minAltitude = minAltitude;
			this.maxAltitude = maxAltitude;
			wp = new Waypoint();
			submittedWaypoint = false;
			outerWarning = false;
		}

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			return "Fly over " + Util.generateSiteName(Root.MissionSeed + waypointID, targetBody == Planetarium.fetch.Home);
		}

		protected override string GetMessageComplete()
		{
			return "You flew over " + Util.generateSiteName(Root.MissionSeed + waypointID, targetBody == Planetarium.fetch.Home) + ".";
		}

		protected override void OnUnregister()
		{
			if (submittedWaypoint)
				WaypointManager.RemoveWaypoint(wp);
		}

		protected override void OnSave(ConfigNode node)
		{
			int bodyID = targetBody.flightGlobalsIndex;
			node.AddValue("minAltitude", minAltitude);
			node.AddValue("maxAltitude", maxAltitude);
			node.AddValue("waypointID", waypointID);
			node.AddValue("targetBody", bodyID);
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "FlightWaypointParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
			Util.LoadNode(node, "FlightWaypointParameter", "minAltitude", ref minAltitude, 0.0);
			Util.LoadNode(node, "FlightWaypointParameter", "maxAltitude", ref maxAltitude, double.PositiveInfinity);
			Util.LoadNode(node, "FlightWaypointParameter", "waypointID", ref waypointID, 0);

			if (HighLogic.LoadedSceneIsFlight && this.Root.ContractState == Contract.State.Active && this.State == ParameterState.Incomplete)
			{
				wp.celestialName = targetBody.GetName();
				wp.RandomizePosition(true);
				wp.seed = Root.MissionSeed;
				wp.id = waypointID;
				wp.setName();
				wp.textureName = "plane";
				wp.altitude = calculateMidAltitude();
				WaypointManager.AddWaypoint(wp);
				submittedWaypoint = true;
			}
		}

		protected override void OnUpdate()
		{
			if (this.Root.ContractState == Contract.State.Active)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (FlightGlobals.ready)
					{
						Vessel v = FlightGlobals.ActiveVessel;
						float distanceToWP = float.PositiveInfinity;

						if (submittedWaypoint && v.mainBody == targetBody)
						{
							if (WaypointManager.Instance() != null)
							{
								distanceToWP = WaypointManager.Instance().DistanceToVessel(wp);

								if (distanceToWP > 30000 && outerWarning)
								{
									outerWarning = false;
									ScreenMessages.PostScreenMessage("You are leaving the area of " + wp.siteName + ".", 5.0f, ScreenMessageStyle.UPPER_LEFT);
								}

								if (distanceToWP <= 30000 && !outerWarning)
								{
									outerWarning = true;
									ScreenMessages.PostScreenMessage("Approaching " + wp.siteName + ", beginning aerial surveillance.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
								}

								if (v.altitude > minAltitude && v.altitude < maxAltitude)
								{
									if (distanceToWP < 15000)
									{
										ScreenMessages.PostScreenMessage("Transmitting aerial surveillance data on " + wp.siteName + ".", 5.0f, ScreenMessageStyle.UPPER_LEFT);
										wp.isExplored = true;
										WaypointManager.RemoveWaypoint(wp);
										submittedWaypoint = false;
										base.SetComplete();
									}
								}
							}
						}
					}
				}
			}
		}

		protected override string GetNotes()
		{
			if (targetBody.GetName() == "Jool")
				return "Warning: an unmanned probe is recommended as this location is on Jool. A cheap one.";
			else
				return null;
		}

		private double calculateMidAltitude()
		{
			return Math.Round((minAltitude + maxAltitude) / 2.0);
		}
	}
}