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
    public class BaseContract : Contract
    {
        CelestialBody targetBody = null;
        int capacity = 0;

        protected override bool Generate()
        {
            if (AreFacilitiesUnlocked() == false)
                return false;

            //Facility fails generation on duplicates, so we can't have many out at once.
            int totalContracts = ContractSystem.Instance.GetCurrentContracts<BaseContract>().Count();
            if (totalContracts >= FPConfig.Base.MaximumExistent)
                return false;

            System.Random generator = new System.Random(this.MissionSeed);
            //I'll use this to determine how "difficult" the mission is, and adjust the pricing at the end.
            float fundsMultiplier = 1;
            float scienceMultiplier = 1;
            float reputationMultiplier = 1;

            if (this.prestige == Contract.ContractPrestige.Trivial)
            {
                List<CelestialBody> bodies;

                bodies = GetBodies_Reached(false, false);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[generator.Next(0, bodies.Count)];

                if (targetBody.GetName() == "Jool")
                {
                    targetBody = Util.RandomJoolianMoon();

                    if (targetBody.GetName() == "Jool" || targetBody == null)
                        return false;
                }

                this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.LANDED, "base"), null);
                this.AddParameter(new FacilitySystemsParameter("land base"), null);
                this.AddParameter(new CrewCapacityParameter(5), null);
                capacity = 5;

                if (Util.haveTechnology("cupola") && FPConfig.Base.AllowCupola)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.TrivialCupolaChance)
                    {
                        this.AddParameter(new PartNameParameter("Have a viewing cupola at the facility", "cupola"), null);
                        fundsMultiplier *= FPConfig.Base.Funds.CupolaMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.CupolaMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.CupolaMultiplier;
                    }
                }

                if (Util.haveTechnology("Large.Crewed.Lab") && FPConfig.Base.AllowLab)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.TrivialLabChance)
                    {
                        this.AddParameter(new FacilityLabParameter(), null);
                        fundsMultiplier *= FPConfig.Base.Funds.LabMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.LabMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.LabMultiplier;
                    }
                }

                if (AreWheelsUnlocked() && FPConfig.Base.AllowMobile)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.TrivialMobileChance)
                    {
                        this.AddParameter(new MobileBaseParameter(), null);
                        fundsMultiplier *= FPConfig.Base.Funds.MobileMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.MobileMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.MobileMultiplier;
                    }
                }
            }
            else if (this.prestige == Contract.ContractPrestige.Significant)
            {
                List<CelestialBody> bodies;

                bodies = GetBodies_Reached(false, false);

                if (bodies.Count == 0)
                    return false;

                targetBody = bodies[generator.Next(0, bodies.Count)];

                if (targetBody.GetName() == "Jool")
                {
                    targetBody = Util.RandomJoolianMoon();

                    if (targetBody.GetName() == "Jool" || targetBody == null)
                        return false;
                }

                this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.LANDED, "base"), null);
                this.AddParameter(new FacilitySystemsParameter("land base"), null);
                int contractCapacity = 5 + generator.Next(1, 8);
                this.AddParameter(new CrewCapacityParameter(contractCapacity), null);
                fundsMultiplier *= FPConfig.Base.Funds.SignificantMultiplier;
                scienceMultiplier *= FPConfig.Base.Science.SignificantMultiplier;
                reputationMultiplier *= FPConfig.Base.Reputation.SignificantMultiplier;
                capacity = contractCapacity;

                if (Util.haveTechnology("cupola") && FPConfig.Base.AllowCupola)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.SignificantCupolaChance)
                    {
                        this.AddParameter(new PartNameParameter("Have a viewing cupola at the facility", "cupola"), null);
                        fundsMultiplier *= FPConfig.Base.Funds.CupolaMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.CupolaMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.CupolaMultiplier;
                    }
                }

                if (Util.haveTechnology("Large.Crewed.Lab") && FPConfig.Base.AllowLab)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.SignificantLabChance)
                    {
                        this.AddParameter(new FacilityLabParameter(), null);
                        fundsMultiplier *= FPConfig.Base.Funds.LabMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.LabMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.LabMultiplier;
                    }
                }

                if (AreWheelsUnlocked() && FPConfig.Base.AllowMobile)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.SignificantMobileChance)
                    {
                        this.AddParameter(new MobileBaseParameter(), null);
                        fundsMultiplier *= FPConfig.Base.Funds.MobileMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.MobileMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.MobileMultiplier;
                    }
                }
            }
            else if (this.prestige == Contract.ContractPrestige.Exceptional)
            {
                int contractCapacity = 5 + generator.Next(1, 8);

                //Prefer unreached targets for this level of difficulty.
                targetBody = GetNextUnreachedTarget(1, true, true);

                // Player has reached all targets, use one we've reached, but bump up capacity to increase difficulty.
                if (targetBody == null)
                {
                    List<CelestialBody> bodies;

                    bodies = GetBodies_Reached(false, false);

                    if (bodies.Count == 0)
                        return false;

                    contractCapacity = 7 + generator.Next(1, 14);
                    targetBody = bodies[generator.Next(0, bodies.Count)];
                }

                if (targetBody.GetName() == "Jool")
                {
                    targetBody = Util.RandomJoolianMoon();

                    if (targetBody.GetName() == "Jool" || targetBody == null)
                        return false;
                }

                this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.LANDED, "base"), null);
                this.AddParameter(new FacilitySystemsParameter("land base"), null);
                this.AddParameter(new CrewCapacityParameter(contractCapacity), null);
                fundsMultiplier *= FPConfig.Base.Funds.ExceptionalMultiplier;
                scienceMultiplier *= FPConfig.Base.Science.ExceptionalMultiplier;
                reputationMultiplier *= FPConfig.Base.Reputation.ExceptionalMultiplier;

                if (Util.haveTechnology("cupola") && FPConfig.Base.AllowCupola)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.ExceptionalCupolaChance)
                    {
                        this.AddParameter(new PartNameParameter("Have a viewing cupola at the facility", "cupola"), null);
                        fundsMultiplier *= FPConfig.Base.Funds.CupolaMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.CupolaMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.CupolaMultiplier;
                    }
                }

                if (Util.haveTechnology("Large.Crewed.Lab") && FPConfig.Base.AllowLab)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.ExceptionalLabChance)
                    {
                        this.AddParameter(new FacilityLabParameter(), null);
                        fundsMultiplier *= FPConfig.Base.Funds.LabMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.LabMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.LabMultiplier;
                    }
                }

                if (AreWheelsUnlocked() && FPConfig.Base.AllowMobile)
                {
                    if (generator.Next(0, 100) < FPConfig.Base.ExceptionalMobileChance)
                    {
                        this.AddParameter(new MobileBaseParameter(), null);
                        fundsMultiplier *= FPConfig.Base.Funds.MobileMultiplier;
                        scienceMultiplier *= FPConfig.Base.Science.MobileMultiplier;
                        reputationMultiplier *= FPConfig.Base.Reputation.MobileMultiplier;
                    }
                }
            }

            base.AddKeywords(new string[] { "groundbase" });
            base.SetExpiry(FPConfig.Base.Expire.MinimumExpireDays, FPConfig.Base.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.Base.Expire.DeadlineDays, targetBody);
            base.SetFunds(Mathf.Round(FPConfig.Base.Funds.BaseAdvance * fundsMultiplier), Mathf.Round(FPConfig.Base.Funds.BaseReward * fundsMultiplier), Mathf.Round(FPConfig.Base.Funds.BaseFailure * fundsMultiplier), this.targetBody);
            base.SetScience(Mathf.Round(FPConfig.Base.Science.BaseReward * scienceMultiplier), this.targetBody);
            base.SetReputation(Mathf.Round(FPConfig.Base.Reputation.BaseReward * reputationMultiplier), Mathf.Round(FPConfig.Base.Reputation.BaseFailure * reputationMultiplier), this.targetBody);

            //Prevent duplicate contracts shortly before finishing up.
            foreach (BaseContract active in ContractSystem.Instance.GetCurrentContracts<BaseContract>())
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
            return "Build a new planetary base on " + targetBody.theName + ".";
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing

            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "building a base", "engineering", "planetary structures", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            return "Build a new planetary base for this agency that can support " + Util.integerToWord(capacity) + " kerbals on the surface of " + targetBody.theName + ".";
        }

        protected override string MessageCompleted()
        {
            return "You have finished construction of a new planetary base on the surface of " + targetBody.theName + ".";
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

        protected static bool AreWheelsUnlocked()
        {
            if (Util.haveTechnology("roverWheel1"))
                return true;

            if (Util.haveTechnology("roverWheel2"))
                return true;

            if (Util.haveTechnology("roverWheel3"))
                return true;

            if (Util.haveTechnology("wheelMed"))
                return true;

            return false;
        }
    }
}