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
    public class ProbeSystemsParameter : ContractParameter
    {
        private bool hasAntenna;
        private bool hasPowerGenerator;
        private int successCounter;
        bool eventsAdded;

        public ProbeSystemsParameter()
        {
            this.hasAntenna = false;
            this.hasPowerGenerator = false;
            this.successCounter = 0;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Launch a new unmanned probe that has power and an antenna";
        }

        protected override string GetNotes()
        {
            return "Please note that this must be a new unmanned probe built for the agency after the contract is accepted.";
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
            hasAntenna = false;
            hasPowerGenerator = false;

            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onFlightReady.Add(FlightReady);
                GameEvents.onVesselChange.Add(VesselChange);
                eventsAdded = true;
            }
        }

        protected override void OnUnregister()
        {
            if (eventsAdded)
            {
                GameEvents.onFlightReady.Remove(FlightReady);
                GameEvents.onVesselChange.Remove(VesselChange);
            }
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
            node.AddValue("hasAntenna", hasAntenna);
            node.AddValue("hasPowerGenerator", hasPowerGenerator);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "FacilitySystemsParameter", "hasAntenna", ref hasAntenna, false);
            Util.LoadNode(node, "FacilitySystemsParameter", "hasPowerGenerator", ref hasPowerGenerator, false);
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        bool probe = (isProbeValid(FlightGlobals.ActiveVessel));

                        if (base.State == ParameterState.Incomplete)
                        {
                            if (probe)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
                        }

                        if (base.State == ParameterState.Complete)
                        {
                            if (!probe)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }

        private bool isProbeValid(Vessel v)
        {
            if (v == null)
                return false;

            if (!v.IsControllable)
                return false;

            if (v.GetCrewCount() > 0)
                return false;

            if (v.launchTime < this.Root.DateAccepted)
                return false;

            hasAntenna = Util.shipHasModuleList(new List<string> { "ModuleDataTransmitter", "ModuleLimitedDataTransmitter", "ModuleRTDataTransmitter", "ModuleRTAntenna" });
            hasPowerGenerator = Util.shipHasModuleList(new List<string> { "ModuleGenerator", "ModuleDeployableSolarPanel", "FNGenerator", "FNAntimatterReactor", "FNNuclearReactor", "FNFusionReactor", "KolonyConverter", "FissionGenerator", "ModuleCurvedSolarPanel" });

            if (hasPowerGenerator && hasAntenna)
                return true;
            else
                return false;
        }
    }
}