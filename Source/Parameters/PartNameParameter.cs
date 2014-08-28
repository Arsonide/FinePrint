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
    public class PartNameParameter : ContractParameter
    {
        private int successCounter;
        private string title;
        private string partName;
        bool eventsAdded;

        public PartNameParameter()
        {
            this.successCounter = 0;
            this.title = "Have a potato";
            this.partName = "potato";
        }

        public PartNameParameter(string title, string partName)
        {
            this.successCounter = 0;
            this.title = title;
            this.partName = partName;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return title;
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;

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
            node.AddValue("title", title);
            node.AddValue("partName", partName);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "PartNameParameter", "title", ref title, "Have a potato");
            Util.LoadNode(node, "PartNameParameter", "partName", ref partName, "potato");
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        bool hasPart = (Util.shipHasPartName(partName));

                        if (this.State == ParameterState.Incomplete)
                        {
                            if (hasPart)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
                        }

                        if (this.State == ParameterState.Complete)
                        {
                            if (!hasPart)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }

        public static string PartName(ContractParameter parameter)
        {
            PartNameParameter Instance = (PartNameParameter)parameter;
            return Instance.partName;
        }
    }
}