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
    public class LocationAndSituationParameter : ContractParameter
    {
        private CelestialBody targetBody;
        private Vessel.Situations targetSituation;
        private string noun;

        public LocationAndSituationParameter()
        {
            targetSituation = Vessel.Situations.ESCAPING;
            targetBody = null;
            noun = "potato";
        }

        public LocationAndSituationParameter(CelestialBody targetBody, Vessel.Situations targetSituation, string noun)
        {
            this.targetBody = targetBody;
            this.targetSituation = targetSituation;
            this.noun = noun;
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            string formattedBody = "potato";

            if (targetBody == null)
                return "Do something on the surface of the Sun";

            //I'm OCD wannafightaboutit?
            if (targetBody.GetName() == "Sun" || targetBody.GetName() == "Mun")
                formattedBody = "the " + targetBody.GetName();
            else
                formattedBody = targetBody.GetName();

            switch (targetSituation)
            {
                case Vessel.Situations.DOCKED:
                    return "Dock your " + noun + " near " + formattedBody;
                case Vessel.Situations.ESCAPING:
                    return "Get your " + noun + " into an escape trajectory out of " + formattedBody;
                case Vessel.Situations.FLYING:
                    return "Fly your " + noun + " on " + formattedBody;
                case Vessel.Situations.LANDED:
                    return "Land your " + noun + " on " + formattedBody;
                case Vessel.Situations.ORBITING:
                    return "Put your " + noun + " in orbit of " + formattedBody;
                case Vessel.Situations.PRELAUNCH:
                    return "Be ready to launch your " + noun + " from " + formattedBody;
                case Vessel.Situations.SPLASHED:
                    return "Splash your " + noun + " down on " + formattedBody;
                case Vessel.Situations.SUB_ORBITAL:
                    return "Set your " + noun + " on a crash course for " + formattedBody;
                default:
                    return "Have your " + noun + " near " + formattedBody;
            }
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("targetSituation", (int)targetSituation);
            node.AddValue("noun", noun);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "LocationAndSituationParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "LocationAndSituationParameter", "targetSituation", ref targetSituation, Vessel.Situations.PRELAUNCH);
            Util.LoadNode(node, "LocationAndSituationParameter", "noun", ref noun, "potato");
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        bool isInLocationAndSituation = (FlightGlobals.ActiveVessel.situation == targetSituation && FlightGlobals.ActiveVessel.mainBody == targetBody);

                        if (isInLocationAndSituation)
                        {
                            if (base.State == ParameterState.Incomplete)
                                base.SetComplete();
                        }

                        if (!isInLocationAndSituation)
                        {
                            if (base.State == ParameterState.Complete)
                                base.SetIncomplete();
                        }
                    }
                }
            }
        }
    }
}