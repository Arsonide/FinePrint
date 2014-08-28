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
    public class RoverWaypointParameter : ContractParameter
    {
        private Waypoint wp;
        private CelestialBody targetBody;
        private int waypointID;
        private double centerLatitude;
        private double centerLongitude;
        private bool submittedWaypoint;
        private bool outerWarning;
        private bool isSecret;
        private double range;

        // Game freaks out without a default constructor. I never use it.
        public RoverWaypointParameter()
        {
            targetBody = Planetarium.fetch.Home;
            centerLatitude = 0.0;
            centerLongitude = 0.0;
            range = 10000.0;
            wp = new Waypoint();
            submittedWaypoint = false;
            outerWarning = false;
            isSecret = false;
        }

        public RoverWaypointParameter(int waypointID, CelestialBody targetBody, double centerLatitude, double centerLongitude, double range, bool secret)
        {

            this.targetBody = targetBody;
            this.waypointID = waypointID;
            this.centerLatitude = centerLatitude;
            this.centerLongitude = centerLongitude;
            this.range = range;
            wp = new Waypoint();
            submittedWaypoint = false;
            outerWarning = false;
            isSecret = secret;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Drive by " + Util.integerToGreek(waypointID) + " Site";
        }

        protected override string GetMessageComplete()
        {
            return "You drove by " + Util.integerToGreek(waypointID) + " Site in " + Util.generateSiteName(Root.MissionSeed, (targetBody == Planetarium.fetch.Home)) + ".";
        }

        protected override void OnUnregister()
        {
            if (submittedWaypoint)
                WaypointManager.RemoveWaypoint(wp);
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("waypointID", waypointID);
            node.AddValue("targetBody", bodyID);
            node.AddValue("isSecret", isSecret);
            node.AddValue("centerLatitude", centerLatitude);
            node.AddValue("centerLongitude", centerLongitude);
            node.AddValue("range", range);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "RoverWaypointParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "RoverWaypointParameter", "isSecret", ref isSecret, false);
            Util.LoadNode(node, "RoverWaypointParameter", "waypointID", ref waypointID, 0);
            Util.LoadNode(node, "RoverWaypointParameter", "centerLatitude", ref centerLatitude, 0.0);
            Util.LoadNode(node, "RoverWaypointParameter", "centerLongitude", ref centerLongitude, 0.0);
            Util.LoadNode(node, "RoverWaypointParameter", "range", ref range, 10000);

            if (HighLogic.LoadedSceneIsFlight && this.Root.ContractState == Contract.State.Active && this.State == ParameterState.Incomplete)
            {
                wp.celestialName = targetBody.GetName();
                wp.seed = Root.MissionSeed;
                wp.id = waypointID;
                wp.RandomizeNear(centerLatitude, centerLongitude, targetBody.GetName(), range, false);
                wp.setName(false);
                wp.waypointType = WaypointType.ROVER;
                wp.altitude = 0.0;
                wp.isClustered = true;
                wp.isOnSurface = true;
                wp.isNavigatable = true;
                WaypointManager.AddWaypoint(wp);
                submittedWaypoint = true;
            }

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                if (this.Root.ContractState != Contract.State.Completed)
                {
                    wp.celestialName = targetBody.GetName();
                    wp.seed = Root.MissionSeed;
                    wp.id = waypointID;
                    wp.RandomizeNear(centerLatitude, centerLongitude, targetBody.GetName(), range, false);
                    wp.setName(false);
                    wp.waypointType = WaypointType.ROVER;
                    wp.altitude = 0.0;
                    wp.isClustered = true;
                    wp.isOnSurface = true;
                    wp.isNavigatable = false;
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
                        float distanceToWP = float.PositiveInfinity;

                        if (submittedWaypoint && v.mainBody == targetBody)
                        {
                            if (WaypointManager.Instance() != null)
                            {
                                distanceToWP = WaypointManager.Instance().LateralDistanceToVessel(wp);

                                if (distanceToWP > FPConfig.Rover.TriggerRange * 2 && outerWarning)
                                {
                                    outerWarning = false;
                                    ScreenMessages.PostScreenMessage("You are leaving the target area of " + wp.tooltip + ".", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                }

                                if (distanceToWP <= FPConfig.Rover.TriggerRange * 2 && !outerWarning)
                                {
                                    outerWarning = true;
                                    ScreenMessages.PostScreenMessage("Approaching target area of " + wp.tooltip + ", checking for anomalous data.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                }

                                if (distanceToWP < FPConfig.Rover.TriggerRange)
                                {
                                    if (Util.hasWheelsOnGround())
                                    {
                                        if (isSecret)
                                        {
                                            ScreenMessages.PostScreenMessage("This is the source of the anomalous data that we've been looking for in " + wp.tooltip + "!", 5.0f, ScreenMessageStyle.UPPER_LEFT);

                                            base.SetComplete();

                                            foreach (RoverWaypointParameter parameter in Root.AllParameters)
                                            {
                                                if (parameter != null)
                                                {
                                                    if (parameter.State != ParameterState.Complete)
                                                    {
                                                        parameter.wp.isExplored = true;
                                                        WaypointManager.deactivateNavPoint(wp);
                                                        WaypointManager.RemoveWaypoint(parameter.wp);
                                                        parameter.submittedWaypoint = false;
                                                    }
                                                }
                                            }

                                            Root.Complete();
                                        }
                                        else
                                        {
                                            ScreenMessages.PostScreenMessage(Util.generateRoverFailString(Root.MissionSeed, waypointID), 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                            wp.isExplored = true;
                                            WaypointManager.deactivateNavPoint(wp);
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
        }
    }
}