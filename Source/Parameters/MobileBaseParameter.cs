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
	public class MobileBaseParameter : ContractParameter
	{
        private int successCounter;
        bool eventsAdded;

		public MobileBaseParameter()
		{
            this.successCounter = 0;
        }

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			return "The ground base must be mobile";
		}

		protected override void OnRegister()
		{
			this.DisableOnStateChange = false;

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

		protected override void OnUpdate()
		{
			if (this.Root.ContractState == Contract.State.Active)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (FlightGlobals.ready)
					{
						bool grounded = (Util.hasWheelsOnGround());

						if (this.State == ParameterState.Incomplete)
						{
                            if (grounded)
                                successCounter++;
                            else
                                successCounter = 0;

                            if (successCounter >= Util.frameSuccessDelay)
                                base.SetComplete();
						}

						if (this.State == ParameterState.Complete)
						{
							if (!grounded)
								base.SetIncomplete();
						}
					}
				}
			}
		}
	}
}