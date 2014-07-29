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
        private bool hasDockingPort;
		private string typeString;

		public FacilitySystemsParameter()
		{
			this.typeString = "potato";
			this.hasAntenna = false;
			this.hasPowerGenerator = false;
            this.hasDockingPort = false;
		}

		public FacilitySystemsParameter(string typeString)
		{
			this.typeString = typeString;
			this.hasAntenna = false;
			this.hasPowerGenerator = false;
            this.hasDockingPort = false;
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
            hasDockingPort = false;
		}

		protected override void OnSave(ConfigNode node)
		{
			node.AddValue("hasAntenna", hasAntenna);
			node.AddValue("hasPowerGenerator", hasPowerGenerator);
            node.AddValue("hasDockingPort", hasDockingPort);
			node.AddValue("typeString", typeString);
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "FacilitySystemsParameter", "hasAntenna", ref hasAntenna, false);
			Util.LoadNode(node, "FacilitySystemsParameter", "hasPowerGenerator", ref hasPowerGenerator, false);
            Util.LoadNode(node, "FacilitySystemsParameter", "hasDockingPort", ref hasDockingPort, false);
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

                        if (base.State == ParameterState.Incomplete)
						{
							if (facility)
								base.SetComplete();
						}

                        if (base.State == ParameterState.Complete)
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
            if (v == null)
                return false;

            if (!v.IsControllable)
				return false;

            if (v.launchTime < this.Root.DateAccepted)
				return false;

            hasAntenna = Util.shipHasModuleList(new List<string> { "ModuleDataTransmitter", "ModuleLimitedDataTransmitter", "ModuleRTDataTransmitter", "ModuleRTAntenna" });
            hasPowerGenerator = Util.shipHasModuleList(new List<string> { "ModuleGenerator", "ModuleDeployableSolarPanel", "FNGenerator", "FNAntimatterReactor", "FNNuclearReactor", "FNFusionReactor", "KolonyConverter" });
            hasDockingPort = Util.shipHasModuleList(new List<string> { "ModuleDockingNode" });

            if (hasPowerGenerator && hasAntenna && hasDockingPort)
				return true;
			else
				return false;
		}
	}
}