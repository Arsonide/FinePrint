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
        float fundsMultiplier = 1;
        float scienceMultiplier = 1;
        float reputationMultiplier = 1;

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

            if (offeredContracts >= FPConfig.Satellite.MaximumAvailable || activeContracts >= FPConfig.Satellite.MaximumActive)
                return false;

            generator = new System.Random(this.MissionSeed);
            float rewardMultiplier = 1.0f;
            float partChance = 10;

            if (this.prestige == Contract.ContractPrestige.Trivial)
            {
                List<CelestialBody> bodies = GetBodies_Reached(true, false);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[UnityEngine.Random.Range(0, bodies.Count)];

                if (generator.Next(0, 100) < FPConfig.Satellite.TrivialSolarChance && FPConfig.Satellite.AllowSolar)
                    targetBody = Planetarium.fetch.Sun;

                //Prefer Kerbin
                if (generator.Next(0, 100) < FPConfig.Satellite.TrivialHomeOverrideChance && FPConfig.Satellite.PreferHome)
                    targetBody = Planetarium.fetch.Home;

                deviation = FPConfig.Satellite.TrivialDeviation;
                difficultyFactor = FPConfig.Satellite.TrivialDifficulty;
                pickEasy();
                rewardMultiplier = 2.0f;
                partChance = FPConfig.Satellite.TrivialPartChance;
            }
            else if (this.prestige == Contract.ContractPrestige.Significant)
            {
                List<CelestialBody> bodies = GetBodies_Reached(true, false);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[UnityEngine.Random.Range(0, bodies.Count)];

                if (generator.Next(0, 100) < FPConfig.Satellite.SignificantSolarChance && FPConfig.Satellite.AllowSolar)
                    targetBody = Planetarium.fetch.Sun;

                //Prefer Kerbin
                if (generator.Next(0, 100) < FPConfig.Satellite.SignificantHomeOverrideChance && FPConfig.Satellite.PreferHome)
                    targetBody = Planetarium.fetch.Home;

                deviation = FPConfig.Satellite.SignificantDeviation;
                difficultyFactor = FPConfig.Satellite.SignificantDifficulty;
                pickMedium();
                rewardMultiplier = 2.5f;
                partChance = FPConfig.Satellite.SignificantPartChance;
                fundsMultiplier = FPConfig.Satellite.Funds.SignificantMultiplier;
                scienceMultiplier = FPConfig.Satellite.Science.SignificantMultiplier;
                reputationMultiplier = FPConfig.Satellite.Reputation.SignificantMultiplier;
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

                if (generator.Next(0, 100) < FPConfig.Satellite.ExceptionalSolarChance && FPConfig.Satellite.AllowSolar)
                    targetBody = Planetarium.fetch.Sun;

                //Prefer Kerbin
                if (generator.Next(0, 100) < FPConfig.Satellite.ExceptionalHomeOverrideChance && FPConfig.Satellite.PreferHome)
                    targetBody = Planetarium.fetch.Home;

                deviation = FPConfig.Satellite.ExceptionalDeviation;
                difficultyFactor = FPConfig.Satellite.ExceptionalDifficulty;
                pickHard();
                rewardMultiplier = 3.0f;
                partChance = FPConfig.Satellite.ExceptionalPartChance;
                fundsMultiplier = FPConfig.Satellite.Funds.ExceptionalMultiplier;
                scienceMultiplier = FPConfig.Satellite.Science.ExceptionalMultiplier;
                reputationMultiplier = FPConfig.Satellite.Reputation.ExceptionalMultiplier;
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
            Util.PostProcessOrbit(ref o);

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
                    fundsMultiplier *= FPConfig.Satellite.Funds.PartMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.PartMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.PartMultiplier;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorThermometer"))
                {
                    this.AddParameter(new PartNameParameter("Have a thermometer on the satellite", "sensorThermometer"));
                    fundsMultiplier *= FPConfig.Satellite.Funds.PartMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.PartMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.PartMultiplier;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorBarometer"))
                {
                    this.AddParameter(new PartNameParameter("Have a barometer on the satellite", "sensorBarometer"));
                    fundsMultiplier *= FPConfig.Satellite.Funds.PartMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.PartMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.PartMultiplier;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorGravimeter"))
                {
                    this.AddParameter(new PartNameParameter("Have a gravimeter on the satellite", "sensorGravimeter"));
                    fundsMultiplier *= FPConfig.Satellite.Funds.PartMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.PartMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.PartMultiplier;
                }
            }

            if (generator.Next(0, 101) <= partChance)
            {
                if (Util.haveTechnology("sensorAccelerometer"))
                {
                    this.AddParameter(new PartNameParameter("Have an accelerometer on the satellite", "sensorAccelerometer"));
                    fundsMultiplier *= FPConfig.Satellite.Funds.PartMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.PartMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.PartMultiplier;
                }
            }

            this.AddParameter(new KillControlsParameter(), null);

            switch (orbitType)
            {
                case OrbitType.POLAR:
                    fundsMultiplier *= FPConfig.Satellite.Funds.PolarMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.PolarMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.PolarMultiplier;
                    break;
                case OrbitType.SYNCHRONOUS:
                    fundsMultiplier *= FPConfig.Satellite.Funds.SynchronousMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.SynchronousMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.SynchronousMultiplier;
                    break;
                case OrbitType.STATIONARY:
                    fundsMultiplier *= FPConfig.Satellite.Funds.StationaryMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.StationaryMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.StationaryMultiplier;
                    break;
                case OrbitType.KOLNIYA:
                    fundsMultiplier *= FPConfig.Satellite.Funds.KolniyaMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.KolniyaMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.KolniyaMultiplier;
                    break;
                case OrbitType.TUNDRA:
                    fundsMultiplier *= FPConfig.Satellite.Funds.TundraMultiplier;
                    scienceMultiplier *= FPConfig.Satellite.Science.TundraMultiplier;
                    reputationMultiplier *= FPConfig.Satellite.Reputation.TundraMultiplier;
                    break;
            }

            if (targetBody == Planetarium.fetch.Home)
            {
                fundsMultiplier *= FPConfig.Satellite.Funds.HomeMultiplier;
                scienceMultiplier *= FPConfig.Satellite.Science.HomeMultiplier;
                reputationMultiplier *= FPConfig.Satellite.Reputation.HomeMultiplier;
            }

            base.AddKeywords(new string[] { "deploysatellite" });
            base.SetExpiry(FPConfig.Satellite.Expire.MinimumExpireDays, FPConfig.Satellite.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.Satellite.Expire.DeadlineDays, targetBody);
            base.SetFunds(FPConfig.Satellite.Funds.BaseAdvance * fundsMultiplier, FPConfig.Satellite.Funds.BaseReward * fundsMultiplier, FPConfig.Satellite.Funds.BaseFailure * fundsMultiplier, this.targetBody);
            base.SetScience(FPConfig.Satellite.Science.BaseReward * scienceMultiplier, this.targetBody);
            base.SetReputation(FPConfig.Satellite.Reputation.BaseReward * reputationMultiplier, FPConfig.Satellite.Reputation.BaseFailure * reputationMultiplier, targetBody);
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
                    return "Position satellite in a " + Util.TitleCase(FPConfig.MolniyaName) + " orbit around " + targetBody.theName + ".";
                case OrbitType.TUNDRA:
                    return "Position satellite in a tundra orbit around " + targetBody.theName + ".";
                case OrbitType.STATIONARY:
                    if ( targetBody == Planetarium.fetch.Sun )
                        return "Position satellite in " + FPConfig.SunStationaryName.ToLower() + " orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "Position satellite in " + FPConfig.HomeStationaryName.ToLower() + " orbit of " + targetBody.theName + ".";
                    else
                        return "Position satellite in " + FPConfig.OtherStationaryName.ToLower() + " orbit of " + targetBody.theName + ".";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "Position satellite in a " + FPConfig.SunSynchronousName.ToLower() + " orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "Position satellite in a " + FPConfig.HomeSynchronousName.ToLower() + " orbit of " + targetBody.theName + ".";
                    else
                        return "Position satellite in a " + FPConfig.OtherSynchronousName.ToLower() + " orbit of " + targetBody.theName + ".";
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
                    return "We need you to build a satellite to our specifications and deploy it into a highly eccentric " + Util.TitleCase(FPConfig.MolniyaName) + " \"lightning\" orbit around " + targetBody.theName + ".";
                case OrbitType.TUNDRA:
                    return "We need you to build a satellite to our specifications and deploy it into a highly eccentric tundra orbit around " + targetBody.theName + ".";
                case OrbitType.STATIONARY:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "We need you to build a satellite to our specifications and place it in " + FPConfig.SunStationaryName.ToLower() + " orbit around " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "We need you to build a satellite to our specifications and place it in " + FPConfig.HomeStationaryName.ToLower() + " orbit around " + targetBody.theName + ".";
                else
                        return "We need you to build a satellite to our specifications and place it in " + FPConfig.OtherStationaryName.ToLower() + " orbit around " + targetBody.theName + ".";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "We need you to build a satellite to our specifications and place it in " + FPConfig.SunSynchronousName.ToLower() + " orbit around " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "We need you to build a satellite to our specifications and place it in " + FPConfig.HomeSynchronousName.ToLower() + " orbit around " + targetBody.theName + ".";
                else
                        return "We need you to build a satellite to our specifications and place it in " + FPConfig.OtherSynchronousName.ToLower() + " orbit around " + targetBody.theName + ".";
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
                    return "You have successfully deployed our satellite in a " + Util.TitleCase(FPConfig.MolniyaName) + " orbit around " + targetBody.theName + ".";
                case OrbitType.TUNDRA:
                    return "You have successfully deployed our satellite in a tundra orbit around " + targetBody.theName + ".";
                case OrbitType.STATIONARY:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "You have successfully placed our satellite in " + FPConfig.SunStationaryName.ToLower() + " orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "You have successfully placed our satellite in " + FPConfig.HomeStationaryName.ToLower() + " orbit of " + targetBody.theName + ".";
                else
                        return "You have successfully placed our satellite in " + FPConfig.OtherStationaryName.ToLower() + " orbit of " + targetBody.theName + ".";
                case OrbitType.SYNCHRONOUS:
                    if (targetBody == Planetarium.fetch.Sun)
                        return "You have successfully placed our satellite in " + FPConfig.SunSynchronousName.ToLower() + " orbit of " + targetBody.theName + ".";
                    else if (targetBody == Planetarium.fetch.Home)
                        return "You have successfully placed our satellite in " + FPConfig.HomeSynchronousName.ToLower() + " orbit of " + targetBody.theName + ".";
                else
                        return "You have successfully placed our satellite in " + FPConfig.OtherSynchronousName.ToLower() + " orbit of " + targetBody.theName + ".";
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

            //Check if the configuration allows this orbit type.
            switch (targetType)
            {
                case OrbitType.EQUATORIAL:
                    if (!FPConfig.Satellite.AllowEquatorial)
                    {
                        orbitType = OrbitType.RANDOM;
                        return;
                    }
                    break;
                case OrbitType.POLAR:
                    if (!FPConfig.Satellite.AllowPolar)
                    {
                        orbitType = OrbitType.RANDOM;
                        return;
                    }
                    break;
                case OrbitType.STATIONARY:
                    if (!FPConfig.Satellite.AllowStationary)
                    {
                        orbitType = OrbitType.RANDOM;
                        return;
                    }
                    break;
                case OrbitType.SYNCHRONOUS:
                    if (!FPConfig.Satellite.AllowSynchronous)
                    {
                        orbitType = OrbitType.RANDOM;
                        return;
                    }
                    break;
                case OrbitType.KOLNIYA:
                    if (!FPConfig.Satellite.AllowKolniya)
                    {
                        orbitType = OrbitType.RANDOM;
                        return;
                    }
                    break;
                case OrbitType.TUNDRA:
                    if (!FPConfig.Satellite.AllowTundra)
                    {
                        orbitType = OrbitType.RANDOM;
                        return;
                    }
                    break;
            }

            //Check if this orbit type is possible on the target body.
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