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

namespace FinePrint
{
    // Check that we are flying a particular vessel
    // This is experimental. I am not sure what sort of events might cause the vessel id to change
    // docking, undocking, decoupling, etc
    public class VesselIdentityParameter : ContractParameter
    {
        private Guid vesselID = Guid.Empty;
        private Vessel vesselMemo = null;
        private int successCounter = 0;
        private bool eventsAdded;

        public VesselIdentityParameter(Vessel vessel)
        {
            this.vesselMemo = vessel;
            this.vesselID = vessel.id;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            Vessel vessel = GetVessel();
            if (vessel == null)
            {
                return "Vessel could not be found";
            } else
            {
                return "Control " + vessel.GetName();
            }
        }

        protected Vessel GetVessel()
        {
            if (vesselMemo == null && vesselID != Guid.Empty && FlightGlobals.ready)
            {
                vesselMemo = FlightGlobals.Vessels.Find(v => v.id == vesselID);
                if (vesselMemo == null)
                {
                    // Our vessel is gone. Nothing can be done.
                    base.SetFailed();
                    vesselID = Guid.Empty; // Don't keep looking for a vessel that's gone
                }
            }
            return vesselMemo;
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
            eventsAdded = false;

            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onFlightReady.Add(FlightReady);
                GameEvents.onVesselDestroy.Add(VesselDestroy);
                eventsAdded = true;
            }
        }

        protected override void OnUnregister()
        {
            if (eventsAdded)
            {
                GameEvents.onFlightReady.Remove(FlightReady);
                GameEvents.onVesselDestroy.Remove(VesselDestroy);
            }
        }

        private void FlightReady()
        {
            base.SetIncomplete();
        }

        private void VesselDestroy(Vessel v)
        {
            if (v.id == vesselID)
                base.SetFailed();
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("VesselID", vesselID);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode<Guid>(node, "VesselID", "vesselID", ref vesselID, Guid.Empty);
            if (vesselID == Guid.Empty)
            {
                //no way to recover from that....
                base.SetFailed();
            }
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
            {
                if (FlightGlobals.ActiveVessel.id == this.vesselID)
                {
                    if (this.State == ParameterState.Incomplete)
                    {
                        successCounter++;
                        if (successCounter >= Util.frameSuccessDelay)
                            base.SetComplete();
                    }
                } else
                {
                    if (this.State != ParameterState.Incomplete)
                        base.SetIncomplete();
                    successCounter = 0;
                }
            }
        }
    }
}

