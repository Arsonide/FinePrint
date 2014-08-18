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
    public enum OrbitType
    {
        SYNCHRONOUS,
        STATIONARY,
        POLAR,
        EQUATORIAL,
        KOLNIYA,
        TUNDRA,
        RANDOM
    }

    public class SpecificOrbitParameter : ContractParameter
    {
        private float iconTimer;
        private const float maxIconTimer = 8f;
        public double deviationWindow;
        public OrbitRenderer orbitRenderer;
        public OrbitDriver orbitDriver;
        public List<Waypoint> iconWaypoints;
        private bool beenSetup;
        private int successCounter;
        private System.Random generator;
        public OrbitType orbitType;
        public double difficultyFactor;
        private double inclination;
        private double eccentricity;
        private double sma;
        private double lan;
        private double argumentOfPeriapsis;
        private double meanAnomalyAtEpoch;
        private double epoch;
        public CelestialBody targetBody;
        private const int numSpinners = 8;

        public SpecificOrbitParameter()
        {
            deviationWindow = 10;
            this.successCounter = 0;
            beenSetup = false;
            orbitType = OrbitType.RANDOM;
            difficultyFactor = 0.5;
            inclination = 0.0;
            eccentricity = 0.0;
            sma = 10000000.0;
            lan = 0.0;
            argumentOfPeriapsis = 0.0;
            meanAnomalyAtEpoch = 0.0;
            epoch = 0.0;
            targetBody = Planetarium.fetch.Home;
        }

        public SpecificOrbitParameter(OrbitType orbitType, double inclination, double eccentricity, double sma, double lan, double argumentOfPeriapsis, double meanAnomalyAtEpoch, double epoch, CelestialBody targetBody, double difficultyFactor, double deviationWindow)
        {
            this.deviationWindow = deviationWindow;
            this.successCounter = 0;
            beenSetup = false;
            this.orbitType = orbitType;
            this.difficultyFactor = difficultyFactor;
            this.inclination = inclination;
            this.eccentricity = eccentricity;
            this.sma = sma;
            this.lan = lan;
            this.argumentOfPeriapsis = argumentOfPeriapsis;
            this.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
            this.epoch = epoch;
            this.targetBody = targetBody;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            switch (orbitType)
            {
                case OrbitType.EQUATORIAL:
                    return "Reach the designated equatorial orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.POLAR:
                    return "Reach the designated polar orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.KOLNIYA:
                    return "Reach the designated Kolniya orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.TUNDRA:
                    return "Reach the designated tundra orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.STATIONARY:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "Reach keliostationary orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "Reach keostationary orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                    else
                        return "Reach stationary orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "Reach the designated keliosynchronous orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "Reach the designated keosynchronous orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                    else
                        return "Reach the designated synchronous orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                default:
                    return "Reach the designated orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
            }
        }

        protected override string GetNotes()
        {
            double PeA = (sma * (1 - eccentricity)) - targetBody.Radius;
            double ApA = (sma * (1 + eccentricity)) - targetBody.Radius;

            string notes = "";

            //In Gene's dialogue, the notes for the previous parameter and this one get smushed together, need to add a \n, but it looks dumb in flight.
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                notes += "\n";

            notes += "Orbit Specifics:\nApoapsis: " + Convert.ToDecimal(Math.Round(ApA)).ToString("#,###") + " meters\nPeriapsis: " + Convert.ToDecimal(Math.Round(PeA)).ToString("#,###") + " meters\nInclination: " + Math.Round(inclination, 1) + " degrees\nLongitude of Ascending Node: " + Math.Round(lan, 1) + " degrees";

            return notes;
        }

        // Fuck. You. State. Bugs.
        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
            GameEvents.onFlightReady.Add(FlightReady);
            GameEvents.onVesselChange.Add(VesselChange);
        }

        protected override void OnUnregister()
        {
            GameEvents.onFlightReady.Remove(FlightReady);
            GameEvents.onVesselChange.Remove(VesselChange);
            cleanup();
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
            node.AddValue("deviationWindow", deviationWindow);
            node.AddValue("difficultyFactor", difficultyFactor);
            node.AddValue("orbitType", (int)orbitType);
            node.AddValue("inclination", inclination);
            node.AddValue("eccentricity", eccentricity);
            node.AddValue("sma", sma);
            node.AddValue("lan", lan);
            node.AddValue("argumentOfPeriapsis", argumentOfPeriapsis);
            node.AddValue("meanAnomalyAtEpoch", meanAnomalyAtEpoch);
            node.AddValue("epoch", epoch);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "SpecificOrbitParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "SpecificOrbitParameter", "deviationWindow", ref deviationWindow, 10);
            Util.LoadNode(node, "SpecificOrbitParameter", "difficultyFactor", ref difficultyFactor, 0.5);
            Util.LoadNode(node, "SpecificOrbitParameter", "orbitType", ref orbitType, OrbitType.RANDOM);
            Util.LoadNode(node, "SpecificOrbitParameter", "inclination", ref inclination, 0.0);
            Util.LoadNode(node, "SpecificOrbitParameter", "eccentricity", ref eccentricity, 0.0);
            Util.LoadNode(node, "SpecificOrbitParameter", "sma", ref sma, 1000000.0);
            Util.LoadNode(node, "SpecificOrbitParameter", "lan", ref lan, 0.0);
            Util.LoadNode(node, "SpecificOrbitParameter", "argumentOfPeriapsis", ref argumentOfPeriapsis, 0.0);
            Util.LoadNode(node, "SpecificOrbitParameter", "meanAnomalyAtEpoch", ref meanAnomalyAtEpoch, 0.0);
            Util.LoadNode(node, "SpecificOrbitParameter", "epoch", ref epoch, 0.0);

            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.Root.ContractState == Contract.State.Active && this.State == ParameterState.Incomplete)
                {
                    if (!beenSetup)
                        setup();
                }
            }

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                if (this.Root.ContractState != Contract.State.Completed)
                {
                    if (!beenSetup)
                        setup();
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
                        bool withinDeviation = isActiveVesselWithinOrbitalDeviation();

                        if (this.State == ParameterState.Incomplete)
                        {
                            if (withinDeviation)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
                        }

                        if (this.State == ParameterState.Complete)
                        {
                            if (!withinDeviation)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }

        public void updateMapIcons(bool isFocused)
        {
            if (beenSetup && MapView.MapIsEnabled)
            {
                if (!isFocused)
                {
                    setVisible(false);
                    return;
                }
                else
                    setVisible(true);

                iconTimer = (iconTimer + Time.deltaTime) % maxIconTimer;

                float offset = -(1f / numSpinners);

                for (int x = 0; x < iconWaypoints.Count; x++)
                {
                    switch (iconWaypoints[x].waypointType)
                    {
                        case WaypointType.ORBITAL:
                            offset += (1f / numSpinners);
                            iconWaypoints[x].orbitPosition = getOrbitPositionAtRatio(offset + (iconTimer / maxIconTimer));
                            break;
                        case WaypointType.ASCENDINGNODE:
                            if (HighLogic.LoadedSceneIsFlight)
                            {
                                iconWaypoints[x].visible = checkWaypointVisibility(iconWaypoints[x]);
                                double angleOfAscendingNode = Util.angleOfAscendingNode(orbitDriver.orbit, FlightGlobals.ActiveVessel.orbit);
                                iconWaypoints[x].orbitPosition = orbitDriver.orbit.getPositionFromTrueAnomaly(angleOfAscendingNode * (Math.PI / 180));
                                iconWaypoints[x].tooltip = "Ascending Node: " + Math.Round(Util.getRelativeInclination(FlightGlobals.ActiveVessel.orbit, orbitDriver.orbit), 1) + "°";
                            }
                            break;
                        case WaypointType.DESCENDINGNODE:
                            if (HighLogic.LoadedSceneIsFlight)
                            {
                                iconWaypoints[x].visible = checkWaypointVisibility(iconWaypoints[x]);
                                double angleOfDescendingNode = Util.angleOfDescendingNode(orbitDriver.orbit, FlightGlobals.ActiveVessel.orbit);
                                iconWaypoints[x].orbitPosition = orbitDriver.orbit.getPositionFromTrueAnomaly(angleOfDescendingNode * (Math.PI / 180));
                                iconWaypoints[x].tooltip = "Descending Node: " + Math.Round(-Util.getRelativeInclination(FlightGlobals.ActiveVessel.orbit, orbitDriver.orbit), 1) + "°";
                            }
                            break;
                        case WaypointType.APOAPSIS:
                            double ApA = (sma * (1 + eccentricity)) - targetBody.Radius;
                            iconWaypoints[x].orbitPosition = Util.positionOfApoapsis(orbitDriver.orbit);
                            iconWaypoints[x].tooltip = targetBody.GetName() + " Apoapsis: " + Convert.ToDecimal(Math.Round(ApA)).ToString("#,###") + "m";
                            break;
                        case WaypointType.PERIAPSIS:
                            double PeA = (sma * (1 - eccentricity)) - targetBody.Radius;
                            iconWaypoints[x].orbitPosition = Util.positionOfPeriapsis(orbitDriver.orbit);
                            iconWaypoints[x].tooltip = targetBody.GetName() + " Periapsis: " + Convert.ToDecimal(Math.Round(PeA)).ToString("#,###") + "m";
                            break;
                    }
                }
            }
        }

        private Vector3d getOrbitPositionAtRatio(float ratio)
        {
            return orbitDriver.orbit.getPositionAtUT(orbitDriver.orbit.period * ratio);
        }

        private void cleanup()
        {
            if ((object)iconWaypoints != null)
            {
                foreach (Waypoint wp in iconWaypoints)
                    WaypointManager.RemoveWaypoint(wp);

                iconWaypoints.Clear();
            }

            //No need to nullify anything, it will fall out of scope on the next load.
            setVisible(false);

            beenSetup = false;
        }

        private void setup()
        {
            generator = new System.Random(this.Root.MissionSeed);

            iconWaypoints = new List<Waypoint>();

            for (int cardinals = 0; cardinals < 4; cardinals++)
            {
                bool addedWaypoint = false;

                switch (cardinals)
                {
                    case 0:
                        if (HighLogic.LoadedSceneIsFlight)
                        {
                            iconWaypoints.Add(new Waypoint());
                            iconWaypoints[iconWaypoints.Count - 1].waypointType = WaypointType.ASCENDINGNODE;
                            addedWaypoint = true;
                        }
                        break;
                    case 1:
                        if (HighLogic.LoadedSceneIsFlight)
                        {
                            iconWaypoints.Add(new Waypoint());
                            iconWaypoints[iconWaypoints.Count - 1].waypointType = WaypointType.DESCENDINGNODE;
                            addedWaypoint = true;
                        }
                        break;
                    case 2:
                        iconWaypoints.Add(new Waypoint());
                        iconWaypoints[iconWaypoints.Count - 1].waypointType = WaypointType.APOAPSIS;
                        addedWaypoint = true;
                        break;
                    case 3:
                        iconWaypoints.Add(new Waypoint());
                        iconWaypoints[iconWaypoints.Count - 1].waypointType = WaypointType.PERIAPSIS;
                        addedWaypoint = true;
                        break;
                }

                if (addedWaypoint)
                {
                    iconWaypoints[iconWaypoints.Count - 1].celestialName = targetBody.GetName();
                    iconWaypoints[iconWaypoints.Count - 1].isOnSurface = false;
                    iconWaypoints[iconWaypoints.Count - 1].isNavigatable = false;
                    iconWaypoints[iconWaypoints.Count - 1].seed = Root.MissionSeed;
                    WaypointManager.AddWaypoint(iconWaypoints[iconWaypoints.Count - 1]);
                }
            }

            for (int x = 0; x < numSpinners; x++)
            {
                iconWaypoints.Add(new Waypoint());
                iconWaypoints[iconWaypoints.Count - 1].celestialName = targetBody.GetName();
                iconWaypoints[iconWaypoints.Count - 1].waypointType = WaypointType.ORBITAL;
                iconWaypoints[iconWaypoints.Count - 1].isOnSurface = false;
                iconWaypoints[iconWaypoints.Count - 1].isNavigatable = false;
                iconWaypoints[iconWaypoints.Count - 1].seed = Root.MissionSeed;
                WaypointManager.AddWaypoint(iconWaypoints[iconWaypoints.Count - 1]);
            }

            orbitDriver = new OrbitDriver();
            orbitDriver.orbit = new Orbit();
            orbitDriver.orbit.referenceBody = targetBody;
            orbitDriver.orbit.semiMajorAxis = sma;
            orbitDriver.orbit.eccentricity = eccentricity;
            orbitDriver.orbit.argumentOfPeriapsis = argumentOfPeriapsis;
            orbitDriver.orbit.inclination = inclination;
            orbitDriver.orbit.LAN = lan;
            orbitDriver.orbit.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
            orbitDriver.orbit.epoch = epoch;
            orbitDriver.orbit.Init();
            //These lines unfortunately cannot be saved. They must be run after every load, or the orbit normals will be inaccurate.
            Vector3d pos = orbitDriver.orbit.getRelativePositionAtUT(0.0);
            Vector3d vel = orbitDriver.orbit.getOrbitalVelocityAtUT(0.0);
            orbitDriver.orbit.h = Vector3d.Cross(pos, vel);

            orbitDriver.orbitColor = WaypointManager.RandomColor(Root.MissionSeed);

            orbitRenderer = MapView.MapCamera.gameObject.AddComponent<OrbitRenderer>();
            orbitRenderer.driver = orbitDriver;
            orbitRenderer.drawIcons = OrbitRenderer.DrawIcons.NONE;
            orbitRenderer.drawNodes = false;
            orbitRenderer.orbitColor = WaypointManager.RandomColor(Root.MissionSeed);

            setVisible(true);

            orbitRenderer.celestialBody = targetBody;

            beenSetup = true;
        }

        private bool isActiveVesselWithinOrbitalDeviation()
        {
            if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready)
                return false;

            if ((object)orbitDriver == null)
                return false;

            if ((object)orbitDriver.orbit == null)
                return false;

            Vessel v = FlightGlobals.ActiveVessel;

            if (v.situation != Vessel.Situations.ORBITING)
                return false;

            if (v.mainBody != targetBody)
                return false;

            //Apoapsis and periapsis account for eccentricity and semi major axis, but are more reliable.
            bool APOMatch = withinDeviation(v.orbit.PeA, orbitDriver.orbit.PeA, deviationWindow);
            bool PERMatch = withinDeviation(v.orbit.ApA, orbitDriver.orbit.ApA, deviationWindow);
            //Sometimes inclinations go negative in KSP.
            bool INCMatch = Math.Abs(Math.Abs(v.orbit.inclination) - Math.Abs(orbitDriver.orbit.inclination)) <= (deviationWindow / 100) * 90;
            bool horizontal = (Math.Abs(orbitDriver.orbit.inclination) % 180 < 1);
            bool LANMatch = false;
            bool ARGMatch = false;

            float argDifference = (float)Math.Abs(v.orbit.argumentOfPeriapsis - orbitDriver.orbit.argumentOfPeriapsis) % 360;

            if (argDifference > 180)
                argDifference = 360 - argDifference;

            //Autopass argument of periapsis checks on circular orbits, they are stupid.
            if (orbitDriver.orbit.eccentricity <= 0.05)
                ARGMatch = true;
            else
                ARGMatch = (argDifference <= (deviationWindow / 100) * 360.0);

            //Autopass LAN checks on inclinations under one degree, they are stupid.
            if (!horizontal)
            {
                double vLAN = v.orbit.LAN;
                double oLAN = orbitDriver.orbit.LAN;
                //Fun fact, if KSP decides to have a negative inclination, LAN gets flipped but the orbit doesn't change!
                if (v.orbit.inclination < 0)
                    vLAN = (vLAN + 180) % 360;

                if (orbitDriver.orbit.inclination < 0)
                    oLAN = (oLAN + 180) % 360;

                float lanDifference = (float)Math.Abs(vLAN - oLAN) % 360;

                if (lanDifference > 180)
                    lanDifference = 360 - lanDifference;

                LANMatch = (lanDifference <= (deviationWindow / 100) * 360.0);
            }
            else
                LANMatch = true;

            if (APOMatch && PERMatch && INCMatch && LANMatch && ARGMatch)
                return true;
            else
                return false;
        }

        private bool withinDeviation(double v1, double v2, double deviation)
        {
            double difference = (Math.Abs(v1 - v2) / ((v1 + v2) / 2)) * 100;

            if (difference <= deviation)
                return true;
            else
                return false;
        }

        public void setVisible(bool visible)
        {
            if ((object)orbitRenderer != null)
            {
                if (visible)
                    orbitRenderer.drawMode = OrbitRenderer.DrawMode.REDRAW_AND_RECALCULATE;
                else
                    orbitRenderer.drawMode = OrbitRenderer.DrawMode.OFF;
            }
        }

        public bool checkWaypointVisibility(Waypoint wp)
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return false;

            if (FlightGlobals.ActiveVessel.mainBody.GetName() != wp.celestialName)
                return false;

            if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.ORBITING)
                return false;

            return true;
        }
    }
}