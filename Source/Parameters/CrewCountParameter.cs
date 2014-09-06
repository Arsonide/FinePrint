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
    public class CrewCountParameter : ContractParameter
    {
        private int targetCount;
        private int successCounter;
        bool eventsAdded;

        public CrewCountParameter()
        {
            targetCount = 8;
            this.successCounter = 0;
        }

        public CrewCountParameter(int targetCount)
        {
            this.targetCount = targetCount;
            this.successCounter = 0;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Have at least " + Util.integerToWord(targetCount) + " kerbals onboard";
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
            eventsAdded = false;

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
            node.AddValue("targetCount", targetCount);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "CrewCountParameter", "targetCount", ref targetCount, 8);
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        bool countReached = (FlightGlobals.ActiveVessel.GetCrewCount() >= targetCount);

                        if (this.State == ParameterState.Incomplete)
                        {
                            if (countReached)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
                        }

                        if (this.State == ParameterState.Complete)
                        {
                            if (!countReached)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }
    }
}