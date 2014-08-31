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
    public class ResourcePossessionParameter : ContractParameter
    {
        private int successCounter;
        private double currentResource;
        public double goalResource;
        public string resourceName;
        bool eventsAdded;

        public ResourcePossessionParameter()
        {
            this.successCounter = 0;
            this.currentResource = 0;
            this.goalResource = 1000;
            this.resourceName = "Karbonite";
            this.eventsAdded = false;
        }

        public ResourcePossessionParameter(string resourceName, double goalResource)
        {
            this.successCounter = 0;
            this.currentResource = 0;
            this.goalResource = goalResource;
            this.resourceName = resourceName;
            this.eventsAdded = false;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Have " + Mathf.Round((float)goalResource) + " " + resourceName + " in your vessel";
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
            node.AddValue("goalResource", goalResource);
            node.AddValue("resourceName", resourceName);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "ResourcePossessionParameter", "goalResource", ref goalResource, 1000);
            Util.LoadNode(node, "ResourcePossessionParameter", "resourceName", ref resourceName, "Karbonite");
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        currentResource = Util.ResourcesOnVessel(FlightGlobals.ActiveVessel, resourceName);
                        bool haveEnough = (currentResource >= goalResource);

                        if (this.State == ParameterState.Incomplete)
                        {
                            if (haveEnough)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
                        }

                        if (this.State == ParameterState.Complete)
                        {
                            if (!haveEnough)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }
    }
}