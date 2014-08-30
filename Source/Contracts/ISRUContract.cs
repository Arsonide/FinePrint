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
    public class ISRUContract : Contract
    {
        public CelestialBody targetBody = null;
        private CelestialBody deliveryBody = null;
        private Vessel.Situations deliverySituation = Vessel.Situations.ORBITING;
        private bool isDelivering = false;
        private float gatherGoal = 1000;
        private string targetResource = "Karbonite";

        protected override bool Generate()
        {
            //ISRU fails generation on duplicates, so we can't have many out at once.
            int totalContracts = ContractSystem.Instance.GetCurrentContracts<ISRUContract>().Count();
            if (totalContracts >= FPConfig.ISRU.MaximumExistent)
                return false;

            if (TechUnlocked() == false)
                return false;

            System.Random generator = new System.Random(this.MissionSeed);
            List<CelestialBody> bodies = GetBodies_Reached(false, true);

            if (bodies.Count == 0)
                return false;

            while (true)
            {
                targetBody = bodies[generator.Next(0, bodies.Count)];

                if (CelestialIsForbidden(targetBody))
                {
                    bodies.Remove(targetBody);

                    if (bodies.Count == 0)
                        return false;

                    continue;
                }
                else
                    break;
            }

            float fundsMultiplier = 1;
            float scienceMultiplier = 1;
            float reputationMultiplier = 1;

            switch (this.prestige)
            {
                case ContractPrestige.Trivial:
                    gatherGoal = FPConfig.ISRU.TrivialExtractAmount;

                    if (generator.Next(0, 100) < FPConfig.ISRU.TrivialDeliveryChance)
                    {
                        deliveryBody = Util.RandomNeighbor(MissionSeed, targetBody, false);
                        isDelivering = true;
                    }

                    break;
                case ContractPrestige.Significant:
                    gatherGoal = FPConfig.ISRU.SignificantExtractAmount;
                    fundsMultiplier = FPConfig.ISRU.Funds.SignificantMultiplier;
                    scienceMultiplier = FPConfig.ISRU.Science.SignificantMultiplier;
                    reputationMultiplier = FPConfig.ISRU.Reputation.SignificantMultiplier;

                    if (generator.Next(0, 100) < FPConfig.ISRU.SignificantDeliveryChance)
                    {
                        deliveryBody = Util.RandomNeighbor(MissionSeed, targetBody, false);
                        isDelivering = true;
                    }

                    break;
                case ContractPrestige.Exceptional:
                    gatherGoal = FPConfig.ISRU.ExceptionalExtractAmount;
                    fundsMultiplier = FPConfig.ISRU.Funds.ExceptionalMultiplier;
                    scienceMultiplier = FPConfig.ISRU.Science.ExceptionalMultiplier;
                    reputationMultiplier = FPConfig.ISRU.Reputation.ExceptionalMultiplier;

                    if (generator.Next(0, 100) < FPConfig.ISRU.ExceptionalDeliveryChance)
                    {
                        deliveryBody = Util.RandomNeighbor(MissionSeed, targetBody, false);
                        isDelivering = true;
                    }

                    break;
            }

            if (deliveryBody == null)
                isDelivering = false;

            targetResource = ChooseResource();

            if (isDelivering)
            {
                fundsMultiplier *= FPConfig.ISRU.Funds.DeliveryMultiplier;
                scienceMultiplier *= FPConfig.ISRU.Science.DeliveryMultiplier;
                reputationMultiplier *= FPConfig.ISRU.Reputation.DeliveryMultiplier;
            }

            //Spice this up just a tad, to make contracts more varied.
            float wobble = (float)generator.NextDouble() * 0.25f;

            if (generator.Next(0, 100) > 50)
                wobble = 1 + wobble;
            else
                wobble = 1 - wobble;

            fundsMultiplier *= wobble;
            scienceMultiplier *= wobble;
            reputationMultiplier *= wobble;

            gatherGoal *= wobble;
            gatherGoal = Mathf.Round(gatherGoal / 50) * 50;

            this.AddParameter(new ResourceExtractionParameter(targetResource, gatherGoal, targetBody), null);

            if (isDelivering)
            {
                deliverySituation = Util.ApplicableSituation(MissionSeed, deliveryBody);
                this.AddParameter(new ResourcePossessionParameter(targetResource, gatherGoal), null);
                this.AddParameter(new LocationAndSituationParameter(deliveryBody, deliverySituation,targetResource), null);
            }

            base.AddKeywords(new string[] { "ISRU" });
            base.SetExpiry(FPConfig.ISRU.Expire.MinimumExpireDays, FPConfig.ISRU.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.ISRU.Expire.DeadlineDays, targetBody);
            base.SetFunds(Mathf.Round(FPConfig.ISRU.Funds.BaseAdvance * fundsMultiplier), Mathf.Round(FPConfig.ISRU.Funds.BaseReward * fundsMultiplier), Mathf.Round(FPConfig.ISRU.Funds.BaseFailure * fundsMultiplier), targetBody);
            base.SetScience(Mathf.Round(FPConfig.ISRU.Science.BaseReward * scienceMultiplier), targetBody);
            base.SetReputation(Mathf.Round(FPConfig.ISRU.Reputation.BaseReward * reputationMultiplier), Mathf.Round(FPConfig.ISRU.Reputation.BaseFailure * reputationMultiplier), targetBody);

            //Prevent duplicate contracts shortly before finishing up.
            foreach (ISRUContract active in ContractSystem.Instance.GetCurrentContracts<ISRUContract>())
            {
                if (active.targetBody == this.targetBody)
                    return false;
            }

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
            if (isDelivering)
                return "Extract " +  targetResource + " from " + targetBody.theName + " and deliver it to " + deliveryBody.theName + ".";
            else
                return "Extract " + Mathf.Round(gatherGoal) + " units of " + targetResource + " from " + targetBody.theName + ".";
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "ISRU", "resource extraction", "sustainability", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            if (isDelivering)
                return "We've detected some " + targetResource + " on " + targetBody.theName + ". Extract " + Mathf.Round(gatherGoal) + " units of it and deliver it to " + deliveryBody.theName + ".";
            else
                return "We've detected some " + targetResource + " on " + targetBody.theName + ". Extract " + Mathf.Round(gatherGoal) + " units of it.";
        }

        protected override string MessageCompleted()
        {
            if ( isDelivering )
                return "You have extracted " + Mathf.Round(gatherGoal) + " units of " + targetResource + " and delivered it to " + deliveryBody.theName + " for " + this.Agent.Name + ".";
            else
                return "You have extracted " + Mathf.Round(gatherGoal) + " units of " + targetResource + " for " + this.Agent.Name + ".";
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.CheckForPatchReset();
            Util.LoadNode(node, "HarvestContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "HarvestContract", "gatherGoal", ref gatherGoal, 1);
            Util.LoadNode(node, "HarvestContract", "targetResource", ref targetResource, "Karbonite");
            Util.LoadNode(node, "HarvestContract", "isDelivering", ref isDelivering, false);

            if (isDelivering)
            {
                Util.LoadNode(node, "HarvestContract", "deliveryBody", ref deliveryBody, Planetarium.fetch.Home);
                Util.LoadNode(node, "HarvestContract", "deliverySituation", ref deliverySituation, Vessel.Situations.ORBITING);
            }
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("gatherGoal", gatherGoal);
            node.AddValue("targetResource", targetResource);
            node.AddValue("isDelivering", isDelivering);

            if (isDelivering)
            {
                bodyID = deliveryBody.flightGlobalsIndex;
                node.AddValue("deliveryBody", bodyID);
                node.AddValue("deliverySituation", (int)deliverySituation);
            }
        }

        //for testing purposes
        public override bool MeetRequirements()
        {
            return true;
        }

        protected static bool TechUnlocked()
        {
            List<string> techList = FPConfig.ISRU.TechnologyUnlocks.Replace(" ", "").Split(',').ToList();

            foreach (string tech in techList)
            {
                if (Util.haveTechnology(tech))
                    return true;
            }

            return false;
        }

        protected static bool CelestialIsForbidden(CelestialBody body)
        {
            if (body == null)
                return true;

            List<string> forbidList = FPConfig.ISRU.ForbiddenCelestials.Replace(" ", "").Split(',').ToList();

            foreach (string forbidden in forbidList)
            {
                if (body.GetName() == forbidden)
                    return true;
            }

            return false;
        }

        protected string ChooseResource()
        {
            System.Random generator = new System.Random(MissionSeed);

            List<string> resourceList = FPConfig.ISRU.AllowableResources.Replace(" ", "").Split(',').ToList();

            if (resourceList.Count > 0)
                return resourceList[generator.Next(0, resourceList.Count)];
            else
                return "";
        }
    }
}