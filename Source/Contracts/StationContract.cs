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
    public class StationContract : Contract
    {
        CelestialBody targetBody = null;
        int capacity = 0;

        protected override bool Generate()
        {
            if (AreFacilitiesUnlocked() == false)
                return false;

            //Facility fails generation on duplicates, so we can't have many out at once.
            int totalContracts = ContractSystem.Instance.GetCurrentContracts<StationContract>().Count();
            if (totalContracts >= FPConfig.Station.MaximumExistent)
                return false;

            System.Random generator = new System.Random(this.MissionSeed);
            //I'll use this to determine how "difficult" the mission is, and adjust the pricing at the end.
            float fundsMultiplier = 1;
            float scienceMultiplier = 1;
            float reputationMultiplier = 1;

            if (this.prestige == Contract.ContractPrestige.Trivial)
            {
                List<CelestialBody> bodies;

                bodies = GetBodies_Reached(true, FPConfig.Station.AllowSolar);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[generator.Next(0, bodies.Count)];

                this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "station"), null);

                this.AddParameter(new FacilitySystemsParameter("orbital station"), null);
                this.AddParameter(new CrewCapacityParameter(5), null);
                capacity = 5;

                if (Util.haveTechnology("cupola") && FPConfig.Station.AllowCupola)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.TrivialCupolaChance)
                    {
                        this.AddParameter(new PartNameParameter("Have a viewing cupola at the facility", "cupola"), null);
                        fundsMultiplier *= FPConfig.Station.Funds.CupolaMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.CupolaMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.CupolaMultiplier;
                    }
                }

                if (Util.haveTechnology("Large.Crewed.Lab") && FPConfig.Station.AllowLab)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.TrivialLabChance)
                    {
                        this.AddParameter(new FacilityLabParameter(), null);
                        fundsMultiplier *= FPConfig.Station.Funds.LabMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.LabMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.LabMultiplier;
                    }
                }

                if (Util.haveTechnology("GrapplingDevice") && FPConfig.Station.AllowAsteroid)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.TrivialAsteroidChance)
                    {
                        string size;
                        if (generator.Next(0, 101) > 50)
                            size = "A";
                        else
                            size = "B";

                        this.AddParameter(new AsteroidParameter(size, true), null);
                        fundsMultiplier *= FPConfig.Station.Funds.AsteroidMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.AsteroidMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.AsteroidMultiplier;
                    }
                }
            }
            else if (this.prestige == Contract.ContractPrestige.Significant)
            {
                List<CelestialBody> bodies;

                bodies = GetBodies_Reached(false, FPConfig.Station.AllowSolar);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[generator.Next(0, bodies.Count)];

                this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "station"), null);

                this.AddParameter(new FacilitySystemsParameter("orbital station"), null);
                int contractCapacity = 5 + generator.Next(1, 8);
                this.AddParameter(new CrewCapacityParameter(contractCapacity), null);
                fundsMultiplier *= FPConfig.Station.Funds.SignificantMultiplier;
                scienceMultiplier *= FPConfig.Station.Science.SignificantMultiplier;
                reputationMultiplier *= FPConfig.Station.Reputation.SignificantMultiplier;
                capacity = contractCapacity;

                if (Util.haveTechnology("cupola") && FPConfig.Station.AllowCupola)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.SignificantCupolaChance)
                    {
                        this.AddParameter(new PartNameParameter("Have a viewing cupola at the facility", "cupola"), null);
                        fundsMultiplier *= FPConfig.Station.Funds.CupolaMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.CupolaMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.CupolaMultiplier;
                    }
                }

                if (Util.haveTechnology("Large.Crewed.Lab") && FPConfig.Station.AllowLab)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.SignificantLabChance)
                    {
                        this.AddParameter(new FacilityLabParameter(), null);
                        fundsMultiplier *= FPConfig.Station.Funds.LabMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.LabMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.LabMultiplier;
                    }
                }

                if (Util.haveTechnology("GrapplingDevice") && FPConfig.Station.AllowAsteroid)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.SignificantAsteroidChance)
                    {
                        string size;
                        if (generator.Next(0, 101) > 50)
                            size = "B";
                        else
                            size = "C";

                        this.AddParameter(new AsteroidParameter(size, true), null);
                        fundsMultiplier *= FPConfig.Station.Funds.AsteroidMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.AsteroidMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.AsteroidMultiplier;
                    }
                }
            }
            else if (this.prestige == Contract.ContractPrestige.Exceptional)
            {
                int contractCapacity = 5 + generator.Next(1, 8);

                //Prefer unreached targets for this level of difficulty.
                targetBody = GetNextUnreachedTarget(1, !FPConfig.Station.AllowSolar, true);

                // Player has reached all targets, use one we've reached, but bump up capacity to increase difficulty.
                if (targetBody == null)
                {
                    List<CelestialBody> bodies;

                    bodies = GetBodies_Reached(false, FPConfig.Station.AllowSolar);

                    if (bodies.Count == 0)
                        return false;

                    contractCapacity = 7 + generator.Next(1, 14);
                    targetBody = bodies[generator.Next(0, bodies.Count)];
                }

                this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "station"), null);
                this.AddParameter(new FacilitySystemsParameter("orbital station"), null);
                this.AddParameter(new CrewCapacityParameter(contractCapacity), null);
                fundsMultiplier *= FPConfig.Station.Funds.ExceptionalMultiplier;
                scienceMultiplier *= FPConfig.Station.Science.ExceptionalMultiplier;
                reputationMultiplier *= FPConfig.Station.Reputation.ExceptionalMultiplier;
                capacity = contractCapacity;

                if (Util.haveTechnology("cupola") && FPConfig.Station.AllowCupola)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.ExceptionalCupolaChance)
                    {
                        this.AddParameter(new PartNameParameter("Have a viewing cupola at the facility", "cupola"), null);
                        fundsMultiplier *= FPConfig.Station.Funds.CupolaMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.CupolaMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.CupolaMultiplier;
                    }
                }

                if (Util.haveTechnology("Large.Crewed.Lab") && FPConfig.Station.AllowLab)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.ExceptionalLabChance)
                    {
                        this.AddParameter(new FacilityLabParameter(), null);
                        fundsMultiplier *= FPConfig.Station.Funds.LabMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.LabMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.LabMultiplier;
                    }
                }

                if (Util.haveTechnology("GrapplingDevice") && FPConfig.Station.AllowAsteroid)
                {
                    if (generator.Next(0, 100) < FPConfig.Station.ExceptionalAsteroidChance)
                    {
                        string size;
                        if (generator.Next(0, 101) > 50)
                            size = "D";
                        else
                            size = "E";

                        this.AddParameter(new AsteroidParameter(size, true), null);
                        fundsMultiplier *= FPConfig.Station.Funds.AsteroidMultiplier;
                        scienceMultiplier *= FPConfig.Station.Science.AsteroidMultiplier;
                        reputationMultiplier *= FPConfig.Station.Reputation.AsteroidMultiplier;
                    }
                }
            }

            base.AddKeywords(new string[] { "spacestation" });

            base.SetExpiry(FPConfig.Station.Expire.MinimumExpireDays, FPConfig.Station.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.Station.Expire.DeadlineDays, targetBody);
            base.SetFunds(Mathf.Round(FPConfig.Station.Funds.BaseAdvance * fundsMultiplier), Mathf.Round(FPConfig.Station.Funds.BaseReward * fundsMultiplier), Mathf.Round(FPConfig.Station.Funds.BaseFailure * fundsMultiplier), this.targetBody);
            base.SetScience(Mathf.Round(FPConfig.Station.Science.BaseReward * scienceMultiplier), this.targetBody);
            base.SetReputation(Mathf.Round(FPConfig.Station.Reputation.BaseReward * reputationMultiplier), Mathf.Round(FPConfig.Station.Reputation.BaseFailure * reputationMultiplier), this.targetBody);

            //Prevent duplicate contracts shortly before finishing up.
            foreach (StationContract active in ContractSystem.Instance.GetCurrentContracts<StationContract>())
            {
                if (active.targetBody == this.targetBody)
                    return false;
            }

            this.AddParameter(new KillControlsParameter(10), null);

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
            if (targetBody == Planetarium.fetch.Sun)
                return "Build a new orbital station on a solar orbit.";
            else
                return "Build a new orbital station around " + targetBody.theName + ".";
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "building a station", "orbital structures", "engineering", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            if (targetBody == Planetarium.fetch.Sun)
                return "Build a new orbital station for this agency that can support " + Util.integerToWord(capacity) + " kerbals in a solar orbit.";
            else
                return "Build a new orbital station for this agency that can support " + Util.integerToWord(capacity) + " kerbals in orbit of " + targetBody.theName + ".";
        }

        protected override string MessageCompleted()
        {
            if (targetBody == Planetarium.fetch.Sun)
                return "You have finished construction of a new orbital station on it's own orbit around the sun.";
            else
                return "You have finished construction of a new orbital station around " + targetBody.theName + ".";
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.CheckForPatchReset();
            Util.LoadNode(node, "FacilityContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "FacilityContract", "capacity", ref capacity, 8);
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("capacity", capacity);
        }

        public override bool MeetRequirements()
        {
            return true;
        }

        protected CelestialBody GetNextUnreachedTarget(int depth, bool removeSun, bool removeKerbin)
        {
            System.Random generator = new System.Random(MissionSeed);
            var bodies = Contract.GetBodies_NextUnreached(depth, null);

            if (bodies != null)
            {
                if (removeSun)
                    bodies.Remove(Planetarium.fetch.Sun);

                if (removeKerbin)
                    bodies.Remove(Planetarium.fetch.Home);

                if (bodies.Count > 0)
                    return bodies[generator.Next(0, bodies.Count - 1)];
            }
            return null;
        }

        protected static bool AreFacilitiesUnlocked()
        {
            if (AreAntennaeUnlocked() && ArePortsUnlocked() && ArePowerPartsUnlocked())
                return true;

            return false;
        }

        protected static bool ArePortsUnlocked()
        {
            if (Util.haveTechnology("dockingPort1"))
                return true;

            if (Util.haveTechnology("dockingPort2"))
                return true;

            if (Util.haveTechnology("dockingPort3"))
                return true;

            if (Util.haveTechnology("dockingPortLarge"))
                return true;

            if (Util.haveTechnology("dockingPortLateral"))
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
    }
}