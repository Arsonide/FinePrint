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

            System.Random generator = new System.Random(this.MissionSeed);
            float fundsMultiplier = 1.0f;

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
                fundsMultiplier = 1.0f;
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
                fundsMultiplier = 1.25f;
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
                fundsMultiplier = 1.5f;
            }

            if (targetBody == null)
                targetBody = Planetarium.fetch.Home;

            this.AddParameter(new ProbeSystemsParameter(), null);
            this.AddParameter(new SpecificOrbitParameter(deviation, targetBody), null);
            this.AddParameter(new KillControlsParameter(), null);


            base.AddKeywords(new string[] { "deploysatellite" });
            base.SetExpiry();
            base.SetDeadlineYears(5.0f, targetBody);

            if (targetBody == Planetarium.fetch.Home)
            {
                fundsMultiplier *= 3.0f;
                base.SetScience(1f, this.targetBody);
            }
            else
                base.SetScience(3f, this.targetBody);

            base.SetFunds(3000.0f * fundsMultiplier, 15000.0f * fundsMultiplier, this.targetBody);
            base.SetReputation(50.0f * fundsMultiplier, 25.0f * fundsMultiplier, targetBody);
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
            return "Position satellite in a specific orbit of " + targetBody.GetName() + ".";
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "unmanned", "probes", "GPS", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            return "We need you to build a satellite and deploy it into a very specific orbit around " + targetBody.GetName() + ".";
        }

        protected override string MessageCompleted()
        {
            return "You have successfully placed our satellite in orbit of " + targetBody.GetName() + ".";
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "SatelliteContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "SatelliteContract", "deviation", ref deviation, 10);
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("deviation", deviation);
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
    }
}