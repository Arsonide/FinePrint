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
    public class KillControlsParameter : ContractParameter
    {
        private float holdSeconds;
        private float holdTimer;

        public KillControlsParameter()
        {
            holdSeconds = 10;
            holdTimer = 10;
        }

        public KillControlsParameter(float holdSeconds)
        {
            this.holdSeconds = holdSeconds;
            holdTimer = holdSeconds;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Neutralize controls for " + Math.Round(holdSeconds) + " seconds";
        }

        // Fuck. You. State. Bugs.
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

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("holdSeconds", holdSeconds);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "HoldParameter", "holdSeconds", ref holdSeconds, 10);
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        Vessel v = FlightGlobals.ActiveVessel;
                        FlightCtrlState controls = v.ctrlState;

                        if (this.State == ParameterState.Incomplete)
                        {
                            if (!controls.isNeutral)
                                holdTimer = holdSeconds;
                            else
                                holdTimer -= Time.deltaTime;

                            if (holdTimer <= 0.0f)
                                base.SetComplete();
                        }

                        if (this.State == ParameterState.Complete)
                        {
                            if (!controls.isNeutral)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }
    }
}