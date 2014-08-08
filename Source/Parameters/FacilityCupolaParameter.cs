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
    public class FacilityCupolaParameter : ContractParameter
    {
        private int successCounter;

        public FacilityCupolaParameter()
        {
            this.successCounter = 0;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Have a viewing cupola at the facility";
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
            GameEvents.onFlightReady.Add(FlightReady);
            GameEvents.onVesselChange.Add(VesselChange);
        }

        protected override void OnUnregister()
        {
            GameEvents.onFlightReady.Remove(FlightReady);
            GameEvents.onVesselChange.Remove(VesselChange);
        }

        private void FlightReady()
        {
            base.SetIncomplete();
        }

        private void VesselChange(Vessel v)
        {
            base.SetIncomplete();
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        bool cupola = (Util.shipHasPartName("cupola"));

                        if (this.State == ParameterState.Incomplete)
                        {
                            if (cupola)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
                        }

                        if (this.State == ParameterState.Complete)
                        {
                            if (!cupola)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }
    }
}