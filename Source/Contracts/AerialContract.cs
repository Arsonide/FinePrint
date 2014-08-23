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
    public class AerialContract : Contract
    {
        protected CelestialBody targetBody = null;
        protected double minAltitude = 0.0;
        protected double maxAltitude = 2000.0;
        protected double centerLatitude = 0.0;
        protected double centerLongitude = 0.0;

        protected virtual bool AllowContract()
        {
            //Allow four contracts in pocket but only two on the board at a time.
            var acs = Util.GetContracts<AerialContract>();
            int offeredContracts = Util.CountContractState(acs, Contract.State.Offered);
            int activeContracts = Util.CountContractState(acs, Contract.State.Active);

            if (offeredContracts >= 2 || activeContracts >= 4)
                return false;
            return true;
        }

        protected override bool Generate()
        {
            if (AreWingsUnlocked() == false)
                return false;

            if (!AllowContract()) return false;

            double range = 10000.0;
            System.Random generator = new System.Random(this.MissionSeed);
            int additionalWaypoints = 0;
            List<CelestialBody> allBodies = GetBodies_Reached(true, false);
            List<CelestialBody> atmosphereBodies = new List<CelestialBody>();

            foreach (CelestialBody body in allBodies)
            {
                if (body.atmosphere)
                    atmosphereBodies.Add(body);
            }

            if (atmosphereBodies.Count == 0)
                return false;

            targetBody = atmosphereBodies[UnityEngine.Random.Range(0, atmosphereBodies.Count)];
            additionalWaypoints = SetAltitudeRange(additionalWaypoints);

            int waypointCount = 0;
            double altitudeHalfQuarterRange = Math.Abs(maxAltitude - minAltitude) * 0.125;
            double upperMidAltitude = ((maxAltitude + minAltitude) / 2.0) + altitudeHalfQuarterRange;
            double lowerMidAltitude = ((maxAltitude + minAltitude) / 2.0) - altitudeHalfQuarterRange;
            minAltitude = Math.Round((minAltitude + (generator.NextDouble() * (lowerMidAltitude - minAltitude))) / 100.0) * 100.0;
            maxAltitude = Math.Round((upperMidAltitude + (generator.NextDouble() * (maxAltitude - upperMidAltitude))) / 100.0) * 100.0;

            if (this.prestige == Contract.ContractPrestige.Trivial)
            {
                waypointCount = 1;
                waypointCount += additionalWaypoints;
                range = 100000.0;
            }
            else if (this.prestige == Contract.ContractPrestige.Significant)
            {
                waypointCount = 2;
                waypointCount += additionalWaypoints;
                range = 200000.0;
            }
            else if (this.prestige == Contract.ContractPrestige.Exceptional)
            {
                waypointCount = 3;
                waypointCount += additionalWaypoints;
                range = 300000.0;
            }

            WaypointManager.ChooseRandomPosition(out centerLatitude, out centerLongitude, targetBody.GetName(), true, false);

            for (int x = 0; x < waypointCount; x++)
            {
                ContractParameter newParameter;
                newParameter = this.AddParameter(new FlightWaypointParameter(x, targetBody, minAltitude, maxAltitude, centerLatitude, centerLongitude, range), null);
                newParameter.SetFunds(3750.0f, targetBody);
                newParameter.SetReputation(7.5f, targetBody);
                newParameter.SetScience(7.5f, targetBody);
            }

            base.AddKeywords(new string[] { "surveyflight" });
            base.SetExpiry();
            base.SetDeadlineYears(5.0f, targetBody);
            base.SetFunds(4000.0f, 17500.0f, targetBody);
            base.SetReputation(50.0f, 25.0f, targetBody);
            return true;
        }

        protected virtual int SetAltitudeRange(int additionalWaypoints)
        {

            switch (targetBody.GetName())
            {
                case "Jool":
                    additionalWaypoints = 0;
                    minAltitude = 15000.0;
                    maxAltitude = 30000.0;
                    break;
                case "Duna":
                    additionalWaypoints = 1;
                    minAltitude = 8000.0;
                    maxAltitude = 16000.0;
                    break;
                case "Laythe":
                    additionalWaypoints = 1;
                    minAltitude = 15000.0;
                    maxAltitude = 30000.0;
                    break;
                case "Eve":
                    additionalWaypoints = 1;
                    minAltitude = 20000.0;
                    maxAltitude = 40000.0;
                    break;
                case "Kerbin":
                    additionalWaypoints = 2;
                    minAltitude = 12500.0;
                    maxAltitude = 25000.0;
                    break;
                default:
                    additionalWaypoints = 0;
                    minAltitude = 0.0;
                    maxAltitude = 10000.0;
                    break;
            }
            return additionalWaypoints;
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
            return "Perform aerial surveys of " + targetBody.theName + " at an altitude of " + (int)minAltitude + " to " + (int)maxAltitude + ".";
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "flying", "not crashing", "aerial", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            return "There are places on " + targetBody.theName + " that we don't know much about, fly over them and see what you can see.";
        }

        protected override string MessageCompleted()
        {
            return "You have successfully performed aerial surveys at all of the points of interest on " + targetBody.theName + ".";
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, this.GetType().Name, "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, this.GetType().Name, "minAltitude", ref minAltitude, 0.0);
            Util.LoadNode(node, this.GetType().Name, "maxAltitude", ref maxAltitude, 10000);
            Util.LoadNode(node, this.GetType().Name, "centerLatitude", ref centerLatitude, 0.0);
            Util.LoadNode(node, this.GetType().Name, "centerLongitude", ref centerLongitude, 0.0);
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("minAltitude", minAltitude);
            node.AddValue("maxAltitude", maxAltitude);
            node.AddValue("centerLatitude", centerLatitude);
            node.AddValue("centerLongitude", centerLongitude);
        }

        public override bool MeetRequirements()
        {
            return true;
        }

        protected static bool AreWingsUnlocked()
        {
            if (Util.haveTechnology("AdvancedCanard"))
                return true;
            if (Util.haveTechnology("StandardCtrlSrf"))
                return true;
            if (Util.haveTechnology("airplaneTail"))
                return true;
            if (Util.haveTechnology("CanardController"))
                return true;
            if (Util.haveTechnology("deltaWing"))
                return true;
            if (Util.haveTechnology("noseConeAdapter"))
                return true;
            if (Util.haveTechnology("rocketNoseCone"))
                return true;
            if (Util.haveTechnology("smallCtrlSrf"))
                return true;
            if (Util.haveTechnology("standardNoseCone"))
                return true;
            if (Util.haveTechnology("sweptWing"))
                return true;
            if (Util.haveTechnology("tailfin"))
                return true;
            if (Util.haveTechnology("wingConnector"))
                return true;
            if (Util.haveTechnology("winglet3"))
                return true;

            return false;
        }
    }
}