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
        private int successCounter;

		public LocationAndSituationParameter()
		{
			targetSituation = Vessel.Situations.ESCAPING;
			targetBody = null;
			noun = "potato";
            this.successCounter = 0;
        }

		public LocationAndSituationParameter(CelestialBody targetBody, Vessel.Situations targetSituation, string noun)
		{
			this.targetBody = targetBody;
			this.targetSituation = targetSituation;
			this.noun = noun;
            this.successCounter = 0;
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

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			if (targetBody == null)
				return "Do something on the surface of the Sun";

			switch (targetSituation)
			{
				case Vessel.Situations.DOCKED:
					return "Dock your " + noun + " near " + targetBody.theName;
				case Vessel.Situations.ESCAPING:
                    return "Get your " + noun + " into an escape trajectory out of " + targetBody.theName;
				case Vessel.Situations.FLYING:
                    return "Fly your " + noun + " on " + targetBody.theName;
				case Vessel.Situations.LANDED:
                    return "Land your " + noun + " on " + targetBody.theName;
				case Vessel.Situations.ORBITING:
                    return "Put your " + noun + " in orbit of " + targetBody.theName;
				case Vessel.Situations.PRELAUNCH:
                    return "Be ready to launch your " + noun + " from " + targetBody.theName;
				case Vessel.Situations.SPLASHED:
                    return "Splash your " + noun + " down on " + targetBody.theName;
				case Vessel.Situations.SUB_ORBITAL:
                    return "Set your " + noun + " on a crash course for " + targetBody.theName;
				default:
                    return "Have your " + noun + " near " + targetBody.theName;
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
			Util.LoadNode(node, "LocationAndSituationParameter", "targetSituation", ref targetSituation, Vessel.Situations.ORBITING);
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

                        if (this.State == ParameterState.Incomplete)
						{
                            if (isInLocationAndSituation)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
						}

                        if (this.State == ParameterState.Complete)
						{
							if (!isInLocationAndSituation)
								base.SetIncomplete();
						}
					}
				}
			}
		}
	}
}