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

		public CrewCapacityParameter()
		{
			targetCapacity = 8;
		}

		public CrewCapacityParameter(int targetCapacity)
		{
			this.targetCapacity = targetCapacity;
		}

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			return "Have a facility supporting at least " + Util.integerToWord(targetCapacity) + " kerbals";
		}

		protected override void OnRegister()
		{
			this.DisableOnStateChange = false;
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