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
    public class ResourceExtractionParameter : ContractParameter
    {
        private CelestialBody targetBody;
        private int successCounter;
        private double currentResource;
        private double lastResource;
        public double totalHarvested;
        public double goalHarvested;
        public string resourceName;
        private bool eventsAdded;
        private bool frameSkip;
        private float notifyLevel;

        public ResourceExtractionParameter()
        {
            this.targetBody = Planetarium.fetch.Home;
            this.successCounter = 0;
            this.currentResource = 0;
            this.lastResource = 0;
            this.totalHarvested = 0;
            this.goalHarvested = 1000;
            this.resourceName = "Karbonite";
            this.eventsAdded = false;
            this.frameSkip = true;
            this.notifyLevel = 0.25f;
        }

        public ResourceExtractionParameter(string resourceName, double goalHarvested, CelestialBody targetBody)
        {
            this.targetBody = targetBody;
            this.successCounter = 0;
            this.currentResource = 0;
            this.lastResource = 0;
            this.totalHarvested = 0;
            this.goalHarvested = goalHarvested;
            this.resourceName = resourceName;
            this.eventsAdded = false;
            this.frameSkip = true;
            this.notifyLevel = 0.25f;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Acquire " + Mathf.Round((float)goalHarvested) + " units of fresh " + resourceName + " from " + targetBody.theName;
        }

        protected override void OnRegister()
        {
            eventsAdded = false;
            frameSkip = true;

            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onFlightReady.Add(FlightReady);
                GameEvents.onVesselChange.Add(VesselChange);
                GameEvents.onVesselWasModified.Add(VesselChange);
                eventsAdded = true;
            }
        }

        protected override void OnUnregister()
        {
            if (eventsAdded)
            {
                GameEvents.onFlightReady.Remove(FlightReady);
                GameEvents.onVesselChange.Remove(VesselChange);
                GameEvents.onVesselWasModified.Remove(VesselChange);
            }
        }

        private void FlightReady()
        {
            frameSkip = true;
            base.SetIncomplete();
        }

        private void VesselChange(Vessel v)
        {
            frameSkip = true;
            base.SetIncomplete();
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("totalHarvested", totalHarvested);
            node.AddValue("goalHarvested", goalHarvested);
            node.AddValue("resourceName", resourceName);
            node.AddValue("notifyLevel", notifyLevel);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "ResourceExtractionParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "ResourceExtractionParameter", "totalHarvested", ref totalHarvested, 0);
            Util.LoadNode(node, "ResourceExtractionParameter", "goalHarvested", ref goalHarvested, 1000);
            Util.LoadNode(node, "ResourceExtractionParameter", "resourceName", ref resourceName, "Karbonite");
            Util.LoadNode(node, "ResourceExtractionParameter", "notifyLevel", ref notifyLevel, 0.25f);
            frameSkip = true;
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        if (FlightGlobals.ActiveVessel.mainBody == targetBody)
                        {
                            currentResource = Util.ResourcesOnVessel(FlightGlobals.ActiveVessel, resourceName);
                            //On major vessel modifications, skip one frame to prevent those resources from counting.
                            if (!frameSkip)
                            {
                                double deltaResource = Math.Max(0, currentResource - lastResource);
                                totalHarvested += deltaResource;

                                if (deltaResource > 0 && (totalHarvested/goalHarvested) >= notifyLevel)
                                {
                                    ScreenMessages.PostScreenMessage("You have extracted " + Math.Round(notifyLevel*100) + "% of " + Util.PossessiveString(Root.Agent.Name) + " " + resourceName + ".", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                    notifyLevel += 0.25f;
                                }

                                bool gatheredEnough = (totalHarvested >= goalHarvested);

                                if (this.State == ParameterState.Incomplete)
                                {
                                    if (gatheredEnough)
                                        successCounter++;
                                    else
                                        successCounter = 0;

                                    if (successCounter >= Util.frameSuccessDelay)
                                        base.SetComplete();
                                }

                                if (this.State == ParameterState.Complete)
                                {
                                    if (!gatheredEnough)
                                        base.SetIncomplete();
                                }
                            }

                            lastResource = currentResource;
                            frameSkip = false;
                        }
                    }
                }
            }
        }
    }
}