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
        private string vesselName = "";
        private int successCounter = 0;
        private bool eventsAdded;

        //mandatory default constructor
        public VesselIdentityParameter()
        {
            vesselMemo = null;
            vesselID = Guid.Empty;
            successCounter = 0;
            eventsAdded = false;
        }

        public VesselIdentityParameter(Vessel vessel)
        {
            this.vesselMemo = vessel;
            this.vesselID = vessel.id;
            this.vesselName = vessel.GetName();
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Control " + vesselName;
        }

        protected Vessel GetVessel()
        {
            if (vesselMemo == null && vesselID != Guid.Empty)
            {
                vesselMemo = FlightGlobals.Vessels.Find(v => v.id == vesselID);
                if (vesselMemo == null)
                {
                    Debug.Log("FinePrint.VesselIdentityParameter.GetVessel Could not find our vessel in the vessel list.");
                    // Our vessel is gone. 
                    base.Root.Cancel();
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
                GameEvents.onVesselRecovered.Add(VesselRecovered);
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
            {
                Debug.Log("FinePrint.VesselIdentityParameter.VesselDestroy(" + v.GetName() + ")");
                base.SetFailed();
            }
        }
        private void VesselRecovered(ProtoVessel v)
        {
            if (v.vesselID == vesselID)
            {
                Debug.Log("FinePrint.VesselIdentityParameter.VesselRecovered(" + v.vesselName + ")");
                base.SetFailed();
            }
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("vesselID", vesselID);
        }

        protected override void OnLoad(ConfigNode node)
        {
            vesselID = LoadGuidNode(node, "vesselID");
            if (vesselID == Guid.Empty)
            {
                Debug.Log("FinePrint.VesselIdentityParameter.OnLoad: Empty vessel ID.");
                //no way to recover from that....
                base.Root.Cancel();
            }
            else
            {
                vesselName = GetVessel().GetName();
            }
        }

        // Tried real hard to make it work with the parameterized type.
        // Somehow Guid is an object at compile time and not an object at runtime.
        // I'm guessing it's some sort of value type for 128 bit integer?
        // So this is the workaround.
        Guid LoadGuidNode(ConfigNode node, string key) {
            try {
                Guid id = new Guid(node.GetValue(key));
                if(id == null) { // Docs say it will throw rather than giving us a null, but out of an abundance of caution...
                    Debug.LogError("Fine Print: LoadGuidNode: Guid constructor returned null");
                    id = Guid.Empty;
                }
                return id;
            } catch (Exception e) {
                Debug.LogError("Fine Print: LoadGuidNode: " + e.Message);
                return Guid.Empty;
            }
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && FlightGlobals.ActiveVessel.id == this.vesselID)
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

