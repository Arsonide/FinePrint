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

    public class EvacuateContract : Contract
    {
        private enum Flavors { Evacuate, ReliefCrew, Explosion };
        private Flavors flavor;
        private string vesselName;
        private string vesselType;
        private string vesselSituation;
        private CelestialBody vesselBody;
        public Guid vesselID;

        protected override bool Generate()
        {
            System.Random generator = new System.Random(this.MissionSeed);
            float fundsMultiplier = 1;
            float reputationMultiplier = 1;

            int offeredContracts = 0;
            int activeContracts = 0;
            List<Guid> contractedVesselIDs = new List<Guid>();
            foreach (EvacuateContract contract in ContractSystem.Instance.GetCurrentContracts<EvacuateContract>())
            {
                if (contract.ContractState == Contract.State.Offered)
                    offeredContracts++;
                else if (contract.ContractState == Contract.State.Active)
                    activeContracts++;
                contractedVesselIDs.Add(contract.vesselID);
            }
            if (offeredContracts >= FPConfig.Evacuate.MaximumAvailable || activeContracts >= FPConfig.Evacuate.MaximumActive)
                return false;

            Vessel vessel = FindAppropriateVessel(generator, contractedVesselIDs);
            if (vessel == null)
                return false;

            vesselID = vessel.id;
            vesselName = vessel.GetName();
            vesselType = Util.vesselTypeString(vessel.vesselType);
            vesselSituation = Util.vesselSituationString(vessel.situation);
            vesselBody = vessel.mainBody;

            vessel.GetVesselCrew().ForEach(k => {
                RecoverKerbal crewHome = new RecoverKerbal("Bring " + k.name + " home");
                crewHome.AddKerbal(k.name);
                this.AddParameter(crewHome);
                fundsMultiplier *= FPConfig.Evacuate.Funds.CrewMultiplier;
                reputationMultiplier *= FPConfig.Evacuate.Reputation.CrewMultiplier;
            });

            if (vesselInTrouble(vessel))
            {
                flavor = Flavors.Evacuate;
                fundsMultiplier *= FPConfig.Evacuate.Funds.TroubleMultiplier;
                reputationMultiplier *= FPConfig.Evacuate.Reputation.TroubleMultiplier;
            }
            else if (FPConfig.Evacuate.AllowDestroy && generator.Next(1, 100) < FPConfig.Evacuate.DestroyChance)
            {
                flavor = Flavors.Explosion;
                fundsMultiplier *= FPConfig.Evacuate.Funds.ExplodeMultiplier;
                reputationMultiplier *= FPConfig.Evacuate.Reputation.ExplodeMultiplier;
            }
            else
            {
                flavor = Flavors.ReliefCrew;
                this.AddParameter(new VesselIdentityParameter(vessel));
                this.AddParameter(new LocationAndSituationParameter(vessel.mainBody, vessel.situation, Util.vesselTypeString(vessel.vesselType)));
                this.AddParameter(new CrewCountParameter(vessel.GetVesselCrew().Count()));
                fundsMultiplier *= FPConfig.Evacuate.Funds.ReliefMultiplier;
                reputationMultiplier *= FPConfig.Evacuate.Reputation.ReliefMultiplier;
            }

            base.AddKeywords(new string[] { "evacuate" });
            base.SetExpiry(FPConfig.Evacuate.Expire.MinimumExpireDays, FPConfig.Evacuate.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.Evacuate.Expire.DeadlineDays, vesselBody);
            base.SetFunds(Mathf.Round(FPConfig.Evacuate.Funds.BaseAdvance * fundsMultiplier), Mathf.Round(FPConfig.Evacuate.Funds.BaseReward * fundsMultiplier), Mathf.Round(FPConfig.Evacuate.Funds.BaseFailure * fundsMultiplier), vesselBody);
            base.SetScience(Mathf.Round(FPConfig.Evacuate.Science.BaseReward), vesselBody);
            base.SetReputation(Mathf.Round(FPConfig.Evacuate.Reputation.BaseReward * reputationMultiplier), Mathf.Round(FPConfig.Evacuate.Reputation.BaseFailure * reputationMultiplier), vesselBody);
            return true;
        }

        private Vessel FindAppropriateVessel(System.Random generator, List<Guid> contractedVesselIds)
        {
            List<VesselDataAccumulator> vessels = FlightGlobals.Vessels
                .Where(v => !v.IsRecoverable && !v.isEVA && !v.isActiveVessel && !contractedVesselIds.Contains(v.id))
                .Select(v => new VesselDataAccumulator(v))
                .ToList();
            List<Vessel> mannedVessels = vessels.Where(v => v.crewCount > 0).Select(v => v.vessel).ToList();

            int nManned = mannedVessels.Count();
            if (nManned == 0)
                return null;
                

            return mannedVessels[generator.Next(0, nManned - 1)];

        }

        private bool vesselInTrouble(Vessel v)
        {
            //look for vessels that appear to be in trouble
            //TODO:
            //-out of electricity
            //-low on fuel relative to situation:
            //  Landed without enough fuel to get back to orbit
            //  Orbiting without fuel
            // Would require delta-v calc like MechJeb's
            // fail on multiple propulsion stages?
            //-no thrust
            if (v.situation == Vessel.Situations.SPLASHED)
                return true;
            return false;
        }

        protected override void OnAccepted()
        {
            //TODO "it's gonna blow" sets vessel to explode in one hour
            base.OnAccepted();
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
            if (flavor == Flavors.ReliefCrew)
                return "Bring a relief crew to " + vesselName + ".";
            else
                return "Rescue the crew of " + vesselName + ".";
        }

        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "rescuing crew", "safe return home", "heroism", new System.Random().Next());
        }

        protected override string GetSynopsys()
        {
            string whatWhere = vesselType + " " + vesselName + ", " + vesselSituation + " " + vesselBody.name;
            switch (flavor)
            {
            case Flavors.Explosion:
                return "Save the kerbonauts aboard the doomed " + whatWhere + ", before it explodes! Return them safely home.";
            case Flavors.ReliefCrew:
                return "Send a relief crew to the " + whatWhere + ", and bring the relieved crew home.";
            default:
                return "Get the kerbonauts aboard the " + whatWhere + ", safely home.";
            }
        }

        protected override string MessageCompleted()
        {
            if (flavor == Flavors.ReliefCrew)
                return "You have sent a new crew to " + vesselName + " and returned the old crew safely!";
            else
                return "You have returned the kerbonauts aboard " + vesselName + " home safely!";
        }
        protected override void OnLoad(ConfigNode node)
        {
            Util.CheckForPatchReset();
            Util.LoadNode(node, "EvacuateContract", "vesselName", ref vesselName, "unknown vessel");
            Util.LoadNode(node, "EvacuateContract", "vesselType", ref vesselType, "vessel");
            Util.LoadNode(node, "EvacuateContract", "vesselSituation", ref vesselSituation, "near");
            Util.LoadNode(node, "EvacuateContract", "vesselBody", ref vesselBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "EvacuateContract", "flavor", ref flavor, Flavors.Evacuate);
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("vesselName", vesselName);
            node.AddValue("vesselType", vesselType);
            node.AddValue("vesselSituation", vesselSituation);
            node.AddValue("vesselBody", vesselBody.flightGlobalsIndex);
            node.AddValue("flavor", flavor);
        }
        public override bool MeetRequirements()
        {
            return true;
        }
    }

}

