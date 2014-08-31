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
    public class StationaryPointParameter : ContractParameter
    {
        private Waypoint wp;
        private CelestialBody targetBody;
        private bool submittedWaypoint;
        private double longitude;
        private int successCounter;
        bool eventsAdded;

        // Game freaks out without a default constructor. I never use it.
        public StationaryPointParameter()
        {
            targetBody = Planetarium.fetch.Home;
            longitude = 0.0;
            wp = new Waypoint();
            submittedWaypoint = false;
            this.successCounter = 0;
        }

        public StationaryPointParameter(CelestialBody targetBody, double longitude)
        {
            this.targetBody = targetBody;
            wp = new Waypoint();
            submittedWaypoint = false;
            this.successCounter = 0;
            this.longitude = longitude;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Keep line of sight with " + Util.generateSiteName(Root.MissionSeed, targetBody == Planetarium.fetch.Home);
        }

        protected override string GetMessageComplete()
        {
            return "You are in line of sight with " + Util.generateSiteName(Root.MissionSeed, targetBody == Planetarium.fetch.Home) + ".";
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;

            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onFlightReady.Add(FlightReady);
                GameEvents.onVesselChange.Add(VesselChange);
                eventsAdded = true;
            }
        }

        protected override void OnUnregister()
        {
            if (eventsAdded)
            {
                GameEvents.onFlightReady.Remove(FlightReady);
                GameEvents.onVesselChange.Remove(VesselChange);
            }

            if (submittedWaypoint)
                WaypointManager.RemoveWaypoint(wp);
        }

        private void FlightReady()
        {
            base.SetIncomplete();
        }

        private void VesselChange(Vessel v)
        {
            base.SetIncomplete();
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("longitude", longitude);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "FlightWaypointParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "FlightWaypointParameter", "longitude", ref longitude, 0.0);

            if (HighLogic.LoadedSceneIsFlight && this.Root.ContractState == Contract.State.Active)
            {
                wp.celestialName = targetBody.GetName();
                wp.latitude = 0.0;
                wp.longitude = longitude;
                wp.seed = Root.MissionSeed;
                wp.id = 0;
                wp.setName();
                wp.waypointType = WaypointType.DISH;
                wp.altitude = 0;
                wp.isOnSurface = true;
                wp.isNavigatable = true;
                WaypointManager.AddWaypoint(wp);
                submittedWaypoint = true;
            }

            // Load all current missions in the tracking station.
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                if (this.Root.ContractState != Contract.State.Completed)
                {
                    wp.celestialName = targetBody.GetName();
                    wp.latitude = 0.0;
                    wp.longitude = longitude;
                    wp.seed = Root.MissionSeed;
                    wp.id = 0;
                    wp.setName();
                    wp.waypointType = WaypointType.DISH;
                    wp.altitude = 0;
                    wp.isOnSurface = true;
                    wp.isNavigatable = true;
                    WaypointManager.AddWaypoint(wp);
                    submittedWaypoint = true;
                }
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

                        if (submittedWaypoint && v.mainBody == targetBody)
                        {
                            double anglediff = 180 - Math.Abs(Math.Abs(v.longitude - longitude) - 180); 
                            bool longitudeMatch = (anglediff <= 45);

                            if (this.State == ParameterState.Incomplete)
                            {
                                if (longitudeMatch)
                                    successCounter++;
                                else
                                    successCounter = 0;

                                if (successCounter >= Util.frameSuccessDelay)
                                    base.SetComplete();
                            }

                            if (this.State == ParameterState.Complete)
                            {
                                if (!longitudeMatch)
                                    base.SetIncomplete();
                            }
                        }
                    }
                }
            }
        }
    }
}