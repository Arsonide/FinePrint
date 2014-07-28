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
	public class FacilitySystemsParameter : ContractParameter
	{
		private bool hasAntenna;
		private bool hasPowerGenerator;
		private bool hasOpenDockingPort;
		private string typeString;

		public FacilitySystemsParameter()
		{
			this.typeString = "potato";
			this.hasAntenna = false;
			this.hasPowerGenerator = false;
			this.hasOpenDockingPort = false;
		}

		public FacilitySystemsParameter(string typeString)
		{
			this.typeString = typeString;
			this.hasAntenna = false;
			this.hasPowerGenerator = false;
			this.hasOpenDockingPort = false;
		}

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			return "Build a new " + typeString + " that has power generation, an antenna, and a free docking port";
		}

		protected override void OnRegister()
		{
			this.DisableOnStateChange = false;
			hasAntenna = false;
			hasPowerGenerator = false;
			hasOpenDockingPort = false;
		}

		protected override void OnSave(ConfigNode node)
		{
			node.AddValue("hasAntenna", hasAntenna);
			node.AddValue("hasPowerGenerator", hasPowerGenerator);
			node.AddValue("hasOpenDockingPort", hasOpenDockingPort);
			node.AddValue("typeString", typeString);
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "FacilitySystemsParameter", "hasAntenna", ref hasAntenna, false);
			Util.LoadNode(node, "FacilitySystemsParameter", "hasPowerGenerator", ref hasPowerGenerator, false);
			Util.LoadNode(node, "FacilitySystemsParameter", "hasOpenDockingPort", ref hasOpenDockingPort, false);
			Util.LoadNode(node, "FacilitySystemsParameter", "typeString", ref typeString, "potato");
		}

		protected override void OnUpdate()
		{
			if (this.Root.ContractState == Contract.State.Active)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (FlightGlobals.ready)
					{
						bool facility = (isStationValid(FlightGlobals.ActiveVessel));

						if (this.State == ParameterState.Incomplete)
						{
							if (facility)
								base.SetComplete();
						}

						if (this.State == ParameterState.Complete)
						{
							if (!facility)
								base.SetIncomplete();
						}
					}
				}
			}
		}

		private bool isStationValid(Vessel v)
		{
			if (!v.IsControllable)
				return false;

			if (v.launchTime < this.Root.DateAccepted)
				return false;

			hasAntenna = false;
			hasPowerGenerator = false;
			hasOpenDockingPort = false;

			foreach (ModuleDataTransmitter antenna in v.FindPartModulesImplementing<ModuleDataTransmitter>())
			{
				hasAntenna = true;
				break;
			}

			foreach (ModuleGenerator generator in v.FindPartModulesImplementing<ModuleGenerator>())
			{
				hasPowerGenerator = true;
				break;
			}

			foreach (ModuleDeployableSolarPanel solarPanel in v.FindPartModulesImplementing<ModuleDeployableSolarPanel>())
			{
				hasPowerGenerator = true;
				break;
			}

			foreach (ModuleDockingNode dockNode in v.FindPartModulesImplementing<ModuleDockingNode>())
			{
				if (dockNode.state == "Ready")
					hasOpenDockingPort = true;
			}

			if (hasPowerGenerator && hasAntenna && hasOpenDockingPort)
				return true;
			else
				return false;
		}
	}
}