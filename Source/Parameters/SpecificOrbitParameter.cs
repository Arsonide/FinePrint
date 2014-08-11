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
        MOLNIYA,
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
        public CelestialBody targetBody;
        public List<Waypoint> iconWaypoints;
        private bool beenSetup;
        private int successCounter;
        private System.Random generator;
        private OrbitType orbitType;
        private double difficultyFactor;

        public SpecificOrbitParameter()
        {
            deviationWindow = 10;
            targetBody = Planetarium.fetch.Home;
            this.successCounter = 0;
            beenSetup = false;
            orbitType = OrbitType.RANDOM;
            difficultyFactor = 0.5;
        }

        public SpecificOrbitParameter(double deviationWindow, CelestialBody targetBody, OrbitType orbitType, double difficultyFactor)
        {
            this.deviationWindow = deviationWindow;
            this.targetBody = targetBody;
            this.successCounter = 0;
            beenSetup = false;
            this.orbitType = orbitType;
            this.difficultyFactor = difficultyFactor;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            switch ( orbitType )
            {
                case OrbitType.EQUATORIAL:
                    return "Reach the designated equatorial orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.POLAR:
                    return "Reach the designated polar orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.MOLNIYA:
                    return "Reach the designated Molniya orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.TUNDRA:
                    return "Reach the designated tundra orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.STATIONARY:
                    if ( targetBody == Planetarium.fetch.Sun )
                        return "Reach heliostationary orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                    else
                        return "Reach geostationary orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                case OrbitType.SYNCHRONOUS:
                    if ( targetBody == Planetarium.fetch.Sun )
                        return "Reach the designated heliosynchronous orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                    else
                        return "Reach the designated geosynchronous orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
                default:
                    return "Reach the designated orbit around " + targetBody.theName + " with a deviation of less than " + Math.Round(deviationWindow) + "%";
            }
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
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "SpecificOrbitParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "SpecificOrbitParameter", "deviationWindow", ref deviationWindow, 10);
            Util.LoadNode(node, "SpecificOrbitParameter", "difficultyFactor", ref difficultyFactor, 0.5);
            Util.LoadNode(node, "SpecificOrbitParameter", "orbitType", ref orbitType, OrbitType.RANDOM);

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
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.Root.ContractState == Contract.State.Active)
                {
                    if (!beenSetup)
                        setup();
                }
                else
                {
                    if (beenSetup)
                        cleanup();
                }
            }

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                if (this.Root.ContractState != Contract.State.Completed)
                {
                    if (!beenSetup)
                        setup();
                }
                else
                {
                    if (beenSetup)
                        cleanup();
                }
            }

            if (beenSetup && MapView.MapIsEnabled)
            {
                if (!isFocused)
                {
                    setVisible(false);
                    return;
                }
                else
                {
                    setVisible(true);
                }

                iconTimer = (iconTimer + Time.deltaTime) % maxIconTimer;

                for (int x = 0; x < iconWaypoints.Count; x++)
                {
                    float offset = 0;

                    if (iconWaypoints.Count > 0)
                        offset = (float)x / (float)iconWaypoints.Count;

                    iconWaypoints[x].orbitPosition = getOrbitPositionAtRatio(offset + (iconTimer / maxIconTimer));
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

            for (int x = 0; x < 4; x++)
            {
                iconWaypoints.Add(new Waypoint());
                iconWaypoints[iconWaypoints.Count - 1].celestialName = targetBody.GetName();
                iconWaypoints[iconWaypoints.Count - 1].textureName = "orbit";
                iconWaypoints[iconWaypoints.Count - 1].isOrbital = true;
                iconWaypoints[iconWaypoints.Count - 1].seed = Root.MissionSeed;
                WaypointManager.AddWaypoint(iconWaypoints[iconWaypoints.Count - 1]);
            }

            orbitDriver = new OrbitDriver();

            //Start with periapsis at a safe distance and a random eccentricity, weighted towards the bottom.
            //Derive SMA from eccentricity and desired periapsis.
            orbitDriver.orbit = new Orbit();

            switch (this.orbitType)
            {
                case OrbitType.POLAR:
                    setRandomOrbit(difficultyFactor, false, true);
                    break;
                case OrbitType.EQUATORIAL:
                    setRandomOrbit(difficultyFactor, true, false);
                    break;
                case OrbitType.STATIONARY:
                    setSynchronousOrbit(difficultyFactor, true, 0.0);
                    break;
                case OrbitType.SYNCHRONOUS:
                    setSynchronousOrbit(difficultyFactor, false, generator.NextDouble()*(difficultyFactor/2));
                    break;
                case OrbitType.MOLNIYA:
                    setMolniyaOrbit();
                    break;
                case OrbitType.TUNDRA:
                    setTundraOrbit();
                    break;
                default:
                    setRandomOrbit(difficultyFactor, false, false);
                    break;
            }

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

            if (APOMatch && PERMatch && INCMatch && LANMatch)
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

        private double weightedDouble(double min, double max, double rand, double exponent)
        {
            return min + (max - min) * Math.Pow(rand, exponent);
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

        private void setRandomOrbit(double difficultyFactor, bool equatorial, bool polar)
        {
            if ((object)orbitDriver == null)
                return;

            if ((object)orbitDriver.orbit == null)
                return;

            if ((object)targetBody == null)
                return;

            if ((object)generator == null)
                return;

            //I need this for my sanity.
            double easeFactor = 1.0 - difficultyFactor;

            orbitDriver.orbit.referenceBody = targetBody;
            double minimumAltitude = Util.getMinimumOrbitalAltitude(targetBody);

            double inc = 0;
            double desiredPeriapsis = 0.0;
            double desiredApoapsis = 0.0;
            double pointA = 0.0;
            double pointB = 0.0;

            float maximumAltitude = 0f;

            //If it chooses the sun, the infinite SOI can cause NAN, so choose Eeloo's altitude instead.
            //Use 90% of the SOI to give a little leeway for error correction.
            if (targetBody == Planetarium.fetch.Sun)
                maximumAltitude = 113549713200f;
            else
                maximumAltitude = Math.Max((float)minimumAltitude, (float)targetBody.sphereOfInfluence * (float)difficultyFactor);

            pointA = minimumAltitude + ((maximumAltitude - minimumAltitude) * generator.NextDouble());
            pointB = minimumAltitude + ((maximumAltitude - minimumAltitude) * generator.NextDouble());

            pointA = UnityEngine.Mathf.Lerp((float)pointA, (float)pointB, (float)easeFactor);

            desiredApoapsis = Math.Max(pointA, pointB);
            desiredPeriapsis = Math.Min(pointA, pointB);
            inc = generator.NextDouble() * 90.0;
            inc *= difficultyFactor;

            if (polar)
                inc = 90;
            else if (equatorial)
                inc = 0;

            if (generator.Next(0, 100) > 50)
                inc *= -1;

            if (generator.Next(0, 100) > 50)
                inc += 180;

            orbitDriver.orbit.inclination = inc;

            orbitDriver.orbit.semiMajorAxis = (desiredApoapsis + desiredPeriapsis) / 2.0;
            orbitDriver.orbit.eccentricity = (desiredApoapsis - desiredPeriapsis) / (desiredApoapsis + desiredPeriapsis);
            orbitDriver.orbit.LAN = generator.NextDouble() * 360.0;
            orbitDriver.orbit.argumentOfPeriapsis = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            orbitDriver.orbit.meanAnomalyAtEpoch = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            orbitDriver.orbit.epoch = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            orbitDriver.orbit.Init();
        }

        private void setSynchronousOrbit(double difficultyFactor, bool stationary, double eccentricity)
        {
            if ((object)orbitDriver == null)
                return;

            if ((object)orbitDriver.orbit == null)
                return;

            if ((object)targetBody == null)
                return;

            if ((object)generator == null)
                return;

            orbitDriver.referenceBody = targetBody;

            if (stationary)
            {
                orbitDriver.orbit.eccentricity = 0.0;
                orbitDriver.orbit.inclination = 0.0;
            }
            else
            {
                orbitDriver.orbit.eccentricity = eccentricity;
                orbitDriver.orbit.inclination = weightedDouble(0, 90, generator.NextDouble(), 4 - 4 * difficultyFactor);
            }

            orbitDriver.orbit.semiMajorAxis = Util.synchronousSMA(targetBody);
            orbitDriver.orbit.LAN = generator.NextDouble() * 360.0;
            orbitDriver.orbit.argumentOfPeriapsis = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.meanAnomalyAtEpoch = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.epoch = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.Init();
        }

        private void setMolniyaOrbit()
        {
            if ((object)orbitDriver == null)
                return;

            if ((object)orbitDriver.orbit == null)
                return;

            if ((object)targetBody == null)
                return;

            if ((object)generator == null)
                return;

            orbitDriver.referenceBody = targetBody;

            double inc = 63.4;

            if (generator.Next(0, 100) > 50)
                inc *= -1;

            if (generator.Next(0, 100) > 50)
                inc += 180;

            orbitDriver.orbit.inclination = inc;
            double semiMajorAxis = Util.molniyaSMA(targetBody);
            orbitDriver.orbit.semiMajorAxis = semiMajorAxis;
            double periapsis = Util.getMinimumOrbitalAltitude(targetBody) * 1.05;
            double apoapsis = (semiMajorAxis * 2) - periapsis;
            orbitDriver.orbit.eccentricity = (apoapsis - periapsis) / (apoapsis + periapsis);
            orbitDriver.orbit.LAN = generator.NextDouble() * 360.0;
            orbitDriver.orbit.argumentOfPeriapsis = -90.0;
            orbitDriver.orbit.meanAnomalyAtEpoch = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.epoch = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.Init();
        }

        private void setTundraOrbit()
        {
            if ((object)orbitDriver == null)
                return;

            if ((object)orbitDriver.orbit == null)
                return;

            if ((object)targetBody == null)
                return;

            if ((object)generator == null)
                return;

            orbitDriver.referenceBody = targetBody;

            double inc = 63.4;

            if (generator.Next(0, 100) > 50)
                inc *= -1;

            if (generator.Next(0, 100) > 50)
                inc += 180;

            orbitDriver.orbit.inclination = inc;
            double semiMajorAxis = Util.synchronousSMA(targetBody);
            orbitDriver.orbit.semiMajorAxis = semiMajorAxis;
            double periapsis = Util.getMinimumOrbitalAltitude(targetBody) * 1.05;
            double apoapsis = (semiMajorAxis * 2) - periapsis;
            orbitDriver.orbit.eccentricity = (apoapsis - periapsis) / (apoapsis + periapsis);
            orbitDriver.orbit.LAN = generator.NextDouble() * 360.0;
            orbitDriver.orbit.argumentOfPeriapsis = -90.0;
            orbitDriver.orbit.meanAnomalyAtEpoch = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.epoch = (double)UnityEngine.Random.Range(0.999f, 1.001f);
            orbitDriver.orbit.Init();
        }
    }
}