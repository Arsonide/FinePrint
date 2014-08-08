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
	public class CrewCapacityParameter : ContractParameter
	{
		private int targetCapacity;
        private int successCounter;

		public CrewCapacityParameter()
		{
			targetCapacity = 8;
            this.successCounter = 0;
		}

		public CrewCapacityParameter(int targetCapacity)
		{
			this.targetCapacity = targetCapacity;
            this.successCounter = 0;
		}

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			return "Have a facility supporting at least " + Util.integerToWord(targetCapacity) + " kerbals";
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
			node.AddValue("targetCapacity", targetCapacity);
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "CrewCapacityParameter", "targetCapacity", ref targetCapacity, 8);
		}

		protected override void OnUpdate()
		{
			if (this.Root.ContractState == Contract.State.Active)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (FlightGlobals.ready)
					{
						bool capacityReached = (FlightGlobals.ActiveVessel.GetCrewCapacity() >= targetCapacity);

						if (this.State == ParameterState.Incomplete)
						{
                            if (capacityReached)
                                successCounter++;
                            else
                                successCounter = 0;

                            if ( successCounter >= Util.frameSuccessDelay )
                                base.SetComplete();
						}

						if (this.State == ParameterState.Complete)
						{
							if (!capacityReached)
								base.SetIncomplete();
						}
					}
				}
			}
		}
	}
}