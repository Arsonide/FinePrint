using System;
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

namespace FinePrint.Contracts
{
    public class SatelliteContract : Contract
    {
        CelestialBody targetBody = null;
        double deviation = 10;
        OrbitType orbitType = OrbitType.RANDOM;
        System.Random generator;
        double difficultyFactor = 0.5;

        protected override bool Generate()
        {
            if (AreSatellitesUnlocked() == false)
                return false;

            //Allow four contracts in pocket but only two on the board at a time.
            int offeredContracts = 0;
            int activeContracts = 0;
            foreach (SatelliteContract contract in ContractSystem.Instance.GetCurrentContracts<SatelliteContract>())
            {
                if (contract.ContractState == Contract.State.Offered)
                    offeredContracts++;
                else if (contract.ContractState == Contract.State.Active)
                    activeContracts++;
            }
            
            if (offeredContracts >= 2 || activeContracts >= 4)
                return false;

            generator = new System.Random(this.MissionSeed);
            float rewardMultiplier = 1.0f;
            int partChance = 10;

            if (this.prestige == Contract.ContractPrestige.Trivial)
            {
                List<CelestialBody> bodies = GetBodies_Reached(true, false);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[UnityEngine.Random.Range(0, bodies.Count)];

                //Prefer Kerbin
                if (generator.Next(0, 100) < 70)
                    targetBody = Planetarium.fetch.Home;

                deviation = 7;
                difficultyFactor = 0.2;
                pickEasy();
                rewardMultiplier = 2.0f;
                partChance = 20;
            }
            else if (this.prestige == Contract.ContractPrestige.Significant)
            {
                List<CelestialBody> bodies = GetBodies_Reached(true, false);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[UnityEngine.Random.Range(0, bodies.Count)];

                if (generator.Next(0, 100) > 90)
                    targetBody = Planetarium.fetch.Sun;

                //Prefer Kerbin
                if (generator.Next(0, 100) < 50)
                    targetBody = Planetarium.fetch.Home;

                deviation = 5;
                difficultyFactor = 0.4;
                pickMedium();
                rewardMultiplier = 2.5f;
                partChance = 40;
            }
            else if (this.prestige == Contract.ContractPrestige.Exceptional)
            {
                targetBody = GetNextUnreachedTarget(1, true, true);

                if (targetBody == null)
                {
                    List<CelestialBody> bodies = GetBodies_Reached(true, false);

                    if (bodies.Count == 0)
                        return false;

                    targetBody = bodies[UnityEngine.Random.Range(0, bodies.Count)];
                }

                if (generator.Next(0, 100) > 70)
                    targetBody = Planetarium.fetch.Sun;

                //Prefer Kerbin
                if (generator.Next(0, 100) < 30)
                    targetBody = Planetarium.fetch.Home;

                deviation = 3;
                difficultyFactor = 0.8;
                pickHard();
                rewardMultiplier = 3.0f;
                partChance = 25;
            }

            if (targetBody == null)
                targetBody = Planetarium.fetch.Home;

            if (orbitType == OrbitType.POLAR)
                rewardMultiplier += 0.25f;

            if (orbitType == OrbitType.STATIONARY || orbitType == OrbitType.SYNCHRONOUS)
                rewardMultiplier += 0.5f;

            if (orbitType == OrbitType.KOLNIYA || orbitType == OrbitType.TUNDRA)
                rewardMultiplier += 1.0f;

            this.AddParameter(new ProbeSystemsParameter(), null);

            double e = 0.0;
            if ( orbitType == OrbitType.SYNCHRONOUS )
                e = generator.NextDouble()*(difficultyFactor/2);
            Orbit o = Util.GenerateOrbit(orbitType, MissionSeed, targetBody, difficultyFactor, e);

            this.AddParameter(new SpecificOrbitParameter(orbitType, o.inclination, o.eccentricity, o.semiMajorAxis, o.LAN, o.argumentOfPeriapsis, o.meanAnomalyAtEpoch, o.epoch, targetBody, difficultyFactor, deviation), null);

            if (orbitType == OrbitType.STATIONARY)
            {
                double latitude = 0.0;
                double longitude = 0.0;
                WaypointManager.ChooseRandomPosition(out latitude, out longitude, targetBody.GetName(), false, true);
                this.AddParameter(new StationaryPointParameter(targetBody, longitude), null);
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("GooExperiment"))
                {
                    this.AddParameter(new PartNameParameter("Have a goo container on the satellite", "GooExperiment"));
                    rewardMultiplier += 0.1f;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorThermometer"))
                {
                    this.AddParameter(new PartNameParameter("Have a thermometer on the satellite", "sensorThermometer"));
                    rewardMultiplier += 0.1f;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorBarometer"))
                {
                    this.AddParameter(new PartNameParameter("Have a barometer on the satellite", "sensorBarometer"));
                    rewardMultiplier += 0.1f;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorGravimeter"))
                {
                    this.AddParameter(new PartNameParameter("Have a gravimeter on the satellite", "sensorGravimeter"));
                    rewardMultiplier += 0.1f;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorAccelerometer"))
                {
                    this.AddParameter(new PartNameParameter("Have an accelerometer on the satellite", "sensorAccelerometer"));
                    rewardMultiplier += 0.1f;
                }
            }

            this.AddParameter(new KillControlsParameter(), null);

            base.AddKeywords(new string[] { "deploysatellite" });
            base.SetExpiry();
            base.SetDeadlineYears(5.0f, targetBody);

            if (targetBody == Planetarium.fetch.Home)
            {
                rewardMultiplier *= 2.0f;
                base.SetScience(1f * rewardMultiplier, this.targetBody);
            }
            else
                base.SetScience(4f * rewardMultiplier, this.targetBody);

            base.SetFunds(3000.0f * rewardMultiplier, 15000.0f * rewardMultiplier, this.targetBody);
            base.SetReputation(50.0f * rewardMultiplier, 25.0f * rewardMultiplier, targetBody);
            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }

        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return (this.MissionSeed.ToString() + this.DateAccepted.ToString());
        }

        protected override string GetTitle()
        {
            switch (orbitType)
            {
                case OrbitType.EQUATORIAL:
                    return "Position satellite in an equatorial orbit of " + targetBody.theName + ".";
                case OrbitType.POLAR:
                    return "Position satellite in a polar orbit of " + targetBody.theName + ".";
                case OrbitType.KOLNIYA:
                    return "Position satellite in a Kolniya orbit around " + targetBody.theName + ".";
                case OrbitType.TUNDRA:
                    return "Position satellite in a tundra orbit around " + targetBody.theName + ".";
                case OrbitType.STATIONARY:
                    if ( targetBody == Planetarium.fetch.Sun )
                        return "Position satellite in keliostationary orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "Position satellite in keostationary orbit of " + targetBody.theName + ".";
                    else
                        return "Position satellite in stationary orbit of " + targetBody.theName + ".";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "Position satellite in a keliosynchronous orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "Position satellite in a keosynchronous orbit of " + targetBody.theName + ".";
                    else
                        return "Position satellite in a synchronous orbit of " + targetBody.theName + ".";
                default:
                    return "Position satellite in a specific orbit of " + targetBody.theName + ".";
            }
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "unmanned", "probes", "GPS", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            switch (orbitType)
            {
                case OrbitType.EQUATORIAL:
                    return "We need you to build a satellite to our specifications and deploy it into an equatorial orbit around " + targetBody.theName + ".";
                case OrbitType.POLAR:
                    return "We need you to build a satellite to our specifications and deploy it into a polar orbit around " + targetBody.theName + ".";
                case OrbitType.KOLNIYA:
                    return "We need you to build a satellite to our specifications and deploy it into a highly eccentric Kolniya \"lightning\" orbit around " + targetBody.theName + ".";
                case OrbitType.TUNDRA:
                    return "We need you to build a satellite to our specifications and deploy it into a highly eccentric tundra orbit around " + targetBody.theName + ".";
                case OrbitType.STATIONARY:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "We need you to build a satellite to our specifications and place it in keliostationary orbit around " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "We need you to build a satellite to our specifications and place it in keostationary orbit around " + targetBody.theName + ".";
                else
                        return "We need you to build a satellite to our specifications and place it in stationary orbit around " + targetBody.theName + ".";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "We need you to build a satellite to our specifications and place it in keliosynchronous orbit around " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "We need you to build a satellite to our specifications and place it in keosynchronous orbit around " + targetBody.theName + ".";
                else
                        return "We need you to build a satellite to our specifications and place it in synchronous orbit around " + targetBody.theName + ".";
                default:
                    return "We need you to build a satellite to our specifications and deploy it into a very specific orbit around " + targetBody.theName + ".";
            }
        }

        protected override string MessageCompleted()
        {
            switch (orbitType)
            {
                case OrbitType.EQUATORIAL:
                    return "You have successfully deployed our satellite into equatorial orbit around " + targetBody.theName + ".";
                case OrbitType.POLAR:
                    return "You have successfully deployed our satellite into polar orbit around " + targetBody.theName + ".";
                case OrbitType.KOLNIYA:
                    return "You have successfully deployed our satellite in a Kolniya orbit around " + targetBody.theName + ".";
                case OrbitType.TUNDRA:
                    return "You have successfully deployed our satellite in a tundra orbit around " + targetBody.theName + ".";
                case OrbitType.STATIONARY:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "You have successfully placed our satellite in keliostationary orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "You have successfully placed our satellite in keostationary orbit of " + targetBody.theName + ".";
                else
                        return "You have successfully placed our satellite in stationary orbit of " + targetBody.theName + ".";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "You have successfully placed our satellite in keliosynchronous orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "You have successfully placed our satellite in keosynchronous orbit of " + targetBody.theName + ".";
                else
                        return "You have successfully placed our satellite in synchronous orbit of " + targetBody.theName + ".";
                default:
                    return "You have successfully deployed our satellite in orbit of " + targetBody.theName + ".";
            }
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "SatelliteContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "SatelliteContract", "deviation", ref deviation, 10);
            Util.LoadNode(node, "SatelliteContract", "orbitType", ref orbitType, OrbitType.RANDOM);
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("deviation", deviation);
            node.AddValue("orbitType", (int)orbitType);
        }

        public override bool MeetRequirements()
        {
            return true;
        }

        protected static bool AreSatellitesUnlocked()
        {
            if (AreAntennaeUnlocked() && AreProbeCoresUnlocked() && ArePowerPartsUnlocked())
                return true;

            return false;
        }

        protected static bool ArePowerPartsUnlocked()
        {
            if (Util.haveTechnology("rtg"))
                return true;

            if (Util.haveTechnology("largeSolarPanel"))
                return true;

            if (Util.haveTechnology("solarPanels1"))
                return true;

            if (Util.haveTechnology("solarPanels2"))
                return true;

            if (Util.haveTechnology("solarPanels3"))
                return true;

            if (Util.haveTechnology("solarPanels4"))
                return true;

            if (Util.haveTechnology("solarPanels5"))
                return true;

            return false;
        }

        protected static bool AreAntennaeUnlocked()
        {
            if (Util.haveTechnology("commDish"))
                return true;

            if (Util.haveTechnology("longAntenna"))
                return true;

            if (Util.haveTechnology("mediumDishAntenna"))
                return true;

            return false;
        }

        protected static bool AreProbeCoresUnlocked()
        {
            if (Util.haveTechnology("probeCoreCube"))
                return true;

            if (Util.haveTechnology("probeCoreHex"))
                return true;

            if (Util.haveTechnology("probeCoreOcto"))
                return true;

            if (Util.haveTechnology("probeCoreOcto2"))
                return true;

            if (Util.haveTechnology("probeCoreSphere"))
                return true;

            if (Util.haveTechnology("probeStackLarge"))
                return true;

            if (Util.haveTechnology("probeStackSmall"))
                return true;

            return false;
        }

        protected static CelestialBody GetNextUnreachedTarget(int depth, bool removeSun, bool removeKerbin)
        {
            var bodies = Contract.GetBodies_NextUnreached(depth, null);
            if (bodies != null)
            {
                if (removeSun)
                    bodies.Remove(Planetarium.fetch.Sun);

                if (removeKerbin)
                    bodies.Remove(Planetarium.fetch.Home);

                if (bodies.Count > 0)
                    return bodies[UnityEngine.Random.Range(0, bodies.Count - 1)];
            }
            return null;
        }

        private void pickEasy()
        {
            if ((object)generator == null)
                return;

            int percentile = generator.Next(0, 101);

            if (percentile <= 33)
                setOrbitType(OrbitType.RANDOM, difficultyFactor);
            else if (percentile > 33 && percentile <= 66)
                setOrbitType(OrbitType.POLAR, difficultyFactor);
            else
                setOrbitType(OrbitType.EQUATORIAL, difficultyFactor);
        }

        private void pickMedium()
        {
            if ((object)generator == null)
                return;

            int percentile = generator.Next(0, 101);

            if (percentile <= 33)
                pickEasy();
            else if (percentile > 33 && percentile <= 66)
                setOrbitType(OrbitType.RANDOM, difficultyFactor);
            else
                setOrbitType(OrbitType.SYNCHRONOUS, difficultyFactor);
        }

        private void pickHard()
        {
            if ((object)generator == null)
                return;

            int percentile = generator.Next(0, 101);

            if (percentile <= 20)
                pickMedium();
            else if (percentile > 20 && percentile <= 40)
                setOrbitType(OrbitType.RANDOM, difficultyFactor);
            else if (percentile > 40 && percentile <= 60)
                setOrbitType(OrbitType.KOLNIYA, difficultyFactor);
            else if (percentile > 60 && percentile <= 80)
                setOrbitType(OrbitType.TUNDRA, difficultyFactor);
            else
                setOrbitType(OrbitType.STATIONARY, difficultyFactor);
        }

        private void setOrbitType(OrbitType targetType, double difficultyFactor)
        {
            if ((object)targetBody == null)
                return;

            if (targetType == OrbitType.SYNCHRONOUS)
            {
                if (Util.canBodyBeSynchronous(targetBody, difficultyFactor / 2))
                    orbitType = targetType;
                else
                    orbitType = OrbitType.RANDOM;
            }
            else if (targetType == OrbitType.STATIONARY)
            {
                if (Util.canBodyBeSynchronous(targetBody, difficultyFactor / 2))
                    orbitType = targetType;
                else
                    orbitType = OrbitType.RANDOM;
            }
            else if (targetType == OrbitType.KOLNIYA)
            {
                if (Util.canBodyBeKolniya(targetBody))
                    orbitType = targetType;
                else
                    orbitType = OrbitType.RANDOM;
            }
            else if (targetType == OrbitType.TUNDRA)
            {
                if (Util.canBodyBeTundra(targetBody))
                    orbitType = targetType;
                else
                    orbitType = OrbitType.RANDOM;
            }
            else
                orbitType = targetType;
        }
    }
}