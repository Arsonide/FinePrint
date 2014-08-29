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

namespace FinePrint.Contracts
{
	public class RoverContract : Contract
	{
		CelestialBody targetBody = null;
		double centerLatitude = 0.0;
		double centerLongitude = 0.0;

		protected override bool Generate()
		{
            if (AreWheelsUnlocked() == false)
                return false;

            //Allow four contracts in pocket but only two on the board at a time.
            int offeredContracts = 0;
            int activeContracts = 0;
            foreach (RoverContract contract in ContractSystem.Instance.GetCurrentContracts<RoverContract>())
            {
                if (contract.ContractState == Contract.State.Offered)
                    offeredContracts++;
                else if (contract.ContractState == Contract.State.Active)
                    activeContracts++;
            }

            if (offeredContracts >= FPConfig.Rover.MaximumAvailable || activeContracts >= FPConfig.Rover.MaximumActive)
                return false;

			System.Random generator = new System.Random(this.MissionSeed);
			double range = 10000.0;
			List<CelestialBody> bodies = GetBodies_Reached(true, false);

			if (bodies.Count == 0)
				return false;

			targetBody = bodies[generator.Next(0, bodies.Count)];

			if (targetBody.GetName() == "Jool")
			{
				targetBody = Util.RandomJoolianMoon();

				if (targetBody.GetName() == "Jool" || targetBody == null)
					return false;
			}

			int waypointCount = 0;
            float fundsMultiplier = 1;
            float scienceMultiplier = 1;
            float reputationMultiplier = 1;
            float wpFundsMultiplier = 1;
            float wpScienceMultiplier = 1;
            float wpReputationMultiplier = 1;

			switch(this.prestige)
            {
                case ContractPrestige.Trivial:
                    waypointCount = FPConfig.Rover.TrivialWaypoints;
                    range = FPConfig.Rover.TrivialRange;

                    range /= 2;
                    range = range + range * targetBody.GeeASL;

                    if (generator.Next(0, 100) < FPConfig.Rover.TrivialHomeNearbyChance && targetBody == Planetarium.fetch.Home)
                        WaypointManager.ChooseRandomPositionNear(out centerLatitude, out centerLongitude, SpaceCenter.Instance.Latitude, SpaceCenter.Instance.Longitude, targetBody.GetName(), FPConfig.Rover.TrivialHomeNearbyRange, false);
                    else
                        WaypointManager.ChooseRandomPosition(out centerLatitude, out centerLongitude, targetBody.GetName(), false, false);

                    break;
                case ContractPrestige.Significant:
                    waypointCount = FPConfig.Rover.SignificantWaypoints;
                    range = FPConfig.Rover.SignificantRange;
                    fundsMultiplier = FPConfig.Rover.Funds.SignificantMultiplier;
                    scienceMultiplier = FPConfig.Rover.Science.SignificantMultiplier;
                    reputationMultiplier = FPConfig.Rover.Reputation.SignificantMultiplier;
                    wpFundsMultiplier = FPConfig.Rover.Funds.WaypointSignificantMultiplier;
                    wpScienceMultiplier = FPConfig.Rover.Science.WaypointSignificantMultiplier;
                    wpReputationMultiplier = FPConfig.Rover.Reputation.WaypointSignificantMultiplier;

                    range /= 2;
                    range = range + range * targetBody.GeeASL;

                    if (generator.Next(0, 100) < FPConfig.Rover.SignificantHomeNearbyChance && targetBody == Planetarium.fetch.Home)
                        WaypointManager.ChooseRandomPositionNear(out centerLatitude, out centerLongitude, SpaceCenter.Instance.Latitude, SpaceCenter.Instance.Longitude, targetBody.GetName(), FPConfig.Rover.SignificantHomeNearbyRange, false);
                    else
                        WaypointManager.ChooseRandomPosition(out centerLatitude, out centerLongitude, targetBody.GetName(), false, false);

                    break;
                case ContractPrestige.Exceptional:
                    waypointCount = FPConfig.Rover.ExceptionalWaypoints;
                    range = FPConfig.Rover.ExceptionalRange;
                    fundsMultiplier = FPConfig.Rover.Funds.ExceptionalMultiplier;
                    scienceMultiplier = FPConfig.Rover.Science.ExceptionalMultiplier;
                    reputationMultiplier = FPConfig.Rover.Reputation.ExceptionalMultiplier;
                    wpFundsMultiplier = FPConfig.Rover.Funds.WaypointExceptionalMultiplier;
                    wpScienceMultiplier = FPConfig.Rover.Science.WaypointExceptionalMultiplier;
                    wpReputationMultiplier = FPConfig.Rover.Reputation.WaypointExceptionalMultiplier;

                    range /= 2;
                    range = range + range * targetBody.GeeASL;

                    if (generator.Next(0, 100) < FPConfig.Rover.ExceptionalHomeNearbyChance && targetBody == Planetarium.fetch.Home)
                        WaypointManager.ChooseRandomPositionNear(out centerLatitude, out centerLongitude, SpaceCenter.Instance.Latitude, SpaceCenter.Instance.Longitude, targetBody.GetName(), FPConfig.Rover.ExceptionalHomeNearbyRange, false);
                    else
                        WaypointManager.ChooseRandomPosition(out centerLatitude, out centerLongitude, targetBody.GetName(), false, false);

                    break;
            }

            int secret = generator.Next(0, waypointCount);

			for (int x = 0; x < waypointCount; x++)
			{
				ContractParameter newParameter;

				if (x == secret)
					newParameter = this.AddParameter(new RoverWaypointParameter(x, targetBody, centerLatitude, centerLongitude, range, true), null);
				else
					newParameter = this.AddParameter(new RoverWaypointParameter(x, targetBody, centerLatitude, centerLongitude, range, false), null);

				newParameter.SetFunds(FPConfig.Rover.Funds.WaypointBaseReward * wpFundsMultiplier, targetBody);
                newParameter.SetScience(FPConfig.Rover.Science.WaypointBaseReward * wpScienceMultiplier, targetBody);
                newParameter.SetReputation(FPConfig.Rover.Reputation.WaypointBaseReward * wpReputationMultiplier, targetBody);
			}

			base.AddKeywords(new string[] { "roversearch" });
            base.SetExpiry(FPConfig.Rover.Expire.MinimumExpireDays, FPConfig.Rover.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.Rover.Expire.DeadlineDays, targetBody);
            base.SetFunds(FPConfig.Rover.Funds.BaseAdvance * fundsMultiplier, FPConfig.Rover.Funds.BaseReward * fundsMultiplier, FPConfig.Rover.Funds.BaseFailure * fundsMultiplier, targetBody);
            base.SetScience(FPConfig.Rover.Science.BaseReward * scienceMultiplier, targetBody);
			base.SetReputation(FPConfig.Rover.Reputation.BaseReward * reputationMultiplier, FPConfig.Rover.Reputation.BaseFailure * reputationMultiplier, targetBody);
            return true;
		}

		public override bool CanBeCancelled()
		{
			return true;
		}

		public override bool CanBeDeclined()
		{
			return true;
		}

		protected override string GetHashString()
		{
			return (this.MissionSeed.ToString() + this.DateAccepted.ToString());
		}

		protected override string GetTitle()
		{
			return "Chart source of data anomaly near " + Util.generateSiteName(MissionSeed, (targetBody == Planetarium.fetch.Home)) + " on " + targetBody.theName + " with a rover.";
		}

		protected override string GetDescription()
		{
			//those 3 strings appear to do nothing
			return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "driving a rover", "joyriding", "surface travel", new System.Random().Next());
		}

		protected override string GetSynopsys()
		{
            return "We've noticed an interesting anomaly in the general area of " + Util.generateSiteName(MissionSeed, (targetBody == Planetarium.fetch.Home)) + " on " + targetBody.theName + ". It's a wide area, so you will need a rover to find it.";
		}

		protected override string MessageCompleted()
		{
            return "You have successfully found the source of the data anomaly in " + Util.generateSiteName(MissionSeed, (targetBody == Planetarium.fetch.Home)) + " on " + targetBody.theName + ".";
		}

		protected override void OnLoad(ConfigNode node)
		{
            Util.CheckForPatchReset();
			Util.LoadNode(node, "RoverContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
			Util.LoadNode(node, "RoverContract", "centerLatitude", ref centerLatitude, 0.0);
			Util.LoadNode(node, "RoverContract", "centerLongitude", ref centerLongitude, 0.0);
		}

		protected override void OnSave(ConfigNode node)
		{
			int bodyID = targetBody.flightGlobalsIndex;
			node.AddValue("targetBody", bodyID);
			node.AddValue("centerLatitude", centerLatitude);
			node.AddValue("centerLongitude", centerLongitude);
		}

		//for testing purposes
		public override bool MeetRequirements()
		{
            return true;
		}

		protected static bool AreWheelsUnlocked()
		{
			if (Util.haveTechnology("roverWheel1"))
				return true;
			if (Util.haveTechnology("roverWheel2"))
				return true;
			if (Util.haveTechnology("roverWheel3"))
				return true;
			if (Util.haveTechnology("wheelMed"))
				return true;

			return false;
		}

		protected static bool AreProbesUnlocked()
		{
			if (Util.haveTechnology("probeCoreCube"))
				return true;
			if (Util.haveTechnology("probeCoreHex"))
				return true;
			if (Util.haveTechnology("probeCoreOcto"))
				return true;
			if (Util.haveTechnology("probeCoreOcto2"))
				return true;
			if (Util.haveTechnology("probeCoreSphere"))
				return true;
			if (Util.haveTechnology("probeStackLarge"))
				return true;
			if (Util.haveTechnology("probeStackSmall"))
				return true;

			return false;
		}
	}
}