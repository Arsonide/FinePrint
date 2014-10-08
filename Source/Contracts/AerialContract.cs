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
	public class AerialContract : Contract
	{
		CelestialBody targetBody = null;
		double minAltitude = 0.0;
		double maxAltitude = 2000.0;
        double centerLatitude = 0.0;
        double centerLongitude = 0.0;
        private bool isLowAltitude = false;

		protected override bool Generate()
		{
            if (AreWingsUnlocked() == false)
                return false;

            int offeredContracts = 0;
            int activeContracts = 0;
            foreach (AerialContract contract in ContractSystem.Instance.GetCurrentContracts<AerialContract>())
            {
                if (contract.ContractState == Contract.State.Offered)
                    offeredContracts++;
                else if (contract.ContractState == Contract.State.Active)
                    activeContracts++;
            }

            if (offeredContracts >= FPConfig.Aerial.MaximumAvailable || activeContracts >= FPConfig.Aerial.MaximumActive)
                return false;

            double range = 10000.0;
			System.Random generator = new System.Random(this.MissionSeed);
			int additionalWaypoints = 1;
			List<CelestialBody> allBodies = GetBodies_Reached(true, false);
			List<CelestialBody> atmosphereBodies = new List<CelestialBody>();

			foreach (CelestialBody body in allBodies)
			{
				if (body.atmosphere)
					atmosphereBodies.Add(body);
			}

			if (atmosphereBodies.Count == 0)
				return false;

			targetBody = atmosphereBodies[generator.Next(0, atmosphereBodies.Count)];

            maxAltitude = targetBody.maxAtmosphereAltitude * 0.33;
            minAltitude = maxAltitude * 0.5;

            if (Util.IsGasGiant(targetBody))
            {
                additionalWaypoints -= 1;
            }
            if (targetBody == Planetarium.fetch.Home)
            {
                additionalWaypoints += 1;
            }


            int waypointCount = 0;
            float fundsMultiplier = 1;
            float scienceMultiplier = 1;
            float reputationMultiplier = 1;
            float wpFundsMultiplier = 1;
            float wpScienceMultiplier = 1;
            float wpReputationMultiplier = 1;

			// Center flights on interesting landmarks instead of flat featureless land or water
			float homeNearbyChance = 0;
			double homeNearbyRange = 0;
			double landmarkLatitude = 0;
			double landmarkLongitude = 0;
            
            double altitudeHalfQuarterRange = Math.Abs(maxAltitude - minAltitude) * 0.125;
			double upperMidAltitude = ((maxAltitude + minAltitude) / 2.0) + altitudeHalfQuarterRange;
			double lowerMidAltitude = ((maxAltitude + minAltitude) / 2.0) - altitudeHalfQuarterRange;
			minAltitude = Math.Round((minAltitude + (generator.NextDouble() * (lowerMidAltitude - minAltitude))) / 100.0) * 100.0;
			maxAltitude = Math.Round((upperMidAltitude + (generator.NextDouble() * (maxAltitude - upperMidAltitude))) / 100.0) * 100.0;

			switch(this.prestige)
            {
                case ContractPrestige.Trivial:
				    waypointCount = FPConfig.Aerial.TrivialWaypoints;
				    waypointCount += additionalWaypoints;
                    range = FPConfig.Aerial.TrivialRange;
					homeNearbyChance = FPConfig.Aerial.TrivialHomeNearbyChance;
					homeNearbyRange = FPConfig.Aerial.TrivialHomeNearbyRange;


                    if (Util.IsGasGiant(targetBody))
                    {
                        if (generator.Next(0, 100) < FPConfig.Aerial.ExceptionalLowAltitudeChance/2)
                        {
                            minAltitude *= FPConfig.Aerial.ExceptionalLowAltitudeMultiplier;
                            maxAltitude *= FPConfig.Aerial.ExceptionalLowAltitudeMultiplier;
                            isLowAltitude = true;
                        }
                    }
                    else
                    {
                        if (generator.Next(0, 100) < FPConfig.Aerial.TrivialLowAltitudeChance)
                        {
                            minAltitude *= FPConfig.Aerial.TrivialLowAltitudeMultiplier;
                            maxAltitude *= FPConfig.Aerial.TrivialLowAltitudeMultiplier;
                            isLowAltitude = true;
                        }
                    }

					if (targetBody.GetName() == "Kerbin")
					{
						// Old Airfield island flights
						landmarkLatitude = -2;
						landmarkLongitude = -72.5;
					}
					else if (targetBody.GetName() == "Earth")
					{
						// TODO: Center on a Real Solar System landmark
						landmarkLatitude = SpaceCenter.Instance.Latitude;
						landmarkLongitude = SpaceCenter.Instance.Longitude;
					}
					else
					{
						// Unknown landmarks
						landmarkLatitude = SpaceCenter.Instance.Latitude;
						landmarkLongitude = SpaceCenter.Instance.Longitude;
					}

                    break;
                case ContractPrestige.Significant:
                    waypointCount = FPConfig.Aerial.SignificantWaypoints;
				    waypointCount += additionalWaypoints;
                    range = FPConfig.Aerial.SignificantRange;
                    fundsMultiplier = FPConfig.Aerial.Funds.SignificantMultiplier;
                    scienceMultiplier = FPConfig.Aerial.Science.SignificantMultiplier;
                    reputationMultiplier = FPConfig.Aerial.Reputation.SignificantMultiplier;
                    wpFundsMultiplier = FPConfig.Aerial.Funds.WaypointSignificantMultiplier;
                    wpScienceMultiplier = FPConfig.Aerial.Science.WaypointSignificantMultiplier;
                    wpReputationMultiplier = FPConfig.Aerial.Reputation.WaypointSignificantMultiplier;

                    if (Util.IsGasGiant(targetBody))
                    {
                        if (generator.Next(0, 100) < FPConfig.Aerial.SignificantLowAltitudeChance/2)
                        {
                            minAltitude *= FPConfig.Aerial.SignificantLowAltitudeMultiplier;
                            maxAltitude *= FPConfig.Aerial.SignificantLowAltitudeMultiplier;
                            isLowAltitude = true;
                        }
                    }
                    else
                    {
                        if (generator.Next(0, 100) < FPConfig.Aerial.SignificantLowAltitudeChance)
                        {
                            minAltitude *= FPConfig.Aerial.SignificantLowAltitudeMultiplier;
                            maxAltitude *= FPConfig.Aerial.SignificantLowAltitudeMultiplier;
                            isLowAltitude = true;
                        }
                    }

					if (targetBody.GetName() == "Kerbin")
					{
						// Mountain flights
						landmarkLatitude = 1;
						landmarkLongitude = -79;
					}
					else if (targetBody.GetName() == "Earth")
					{
						// TODO: Center on a Real Solar System landmark
						landmarkLatitude = SpaceCenter.Instance.Latitude;
						landmarkLongitude = SpaceCenter.Instance.Longitude;
					}
					else
					{
						// Unknown landmarks
						landmarkLatitude = SpaceCenter.Instance.Latitude;
						landmarkLongitude = SpaceCenter.Instance.Longitude;
					}

                    break;
                case ContractPrestige.Exceptional:
                    waypointCount = FPConfig.Aerial.ExceptionalWaypoints;
				    waypointCount += additionalWaypoints;
                    range = FPConfig.Aerial.ExceptionalRange;
                    fundsMultiplier = FPConfig.Aerial.Funds.ExceptionalMultiplier;
                    scienceMultiplier = FPConfig.Aerial.Science.ExceptionalMultiplier;
                    reputationMultiplier = FPConfig.Aerial.Reputation.ExceptionalMultiplier;
                    wpFundsMultiplier = FPConfig.Aerial.Funds.WaypointExceptionalMultiplier;
                    wpScienceMultiplier = FPConfig.Aerial.Science.WaypointExceptionalMultiplier;
                    wpReputationMultiplier = FPConfig.Aerial.Reputation.WaypointExceptionalMultiplier;

                    if (Util.IsGasGiant(targetBody))
                    {
                        if (generator.Next(0, 100) < FPConfig.Aerial.TrivialLowAltitudeChance/2)
                        {
                            minAltitude *= FPConfig.Aerial.TrivialLowAltitudeMultiplier;
                            maxAltitude *= FPConfig.Aerial.TrivialLowAltitudeMultiplier;
                            isLowAltitude = true;
                        }
                    }
                    else
                    {
                        if (generator.Next(0, 100) < FPConfig.Aerial.ExceptionalLowAltitudeChance)
                        {
                            minAltitude *= FPConfig.Aerial.ExceptionalLowAltitudeMultiplier;
                            maxAltitude *= FPConfig.Aerial.ExceptionalLowAltitudeMultiplier;
                            isLowAltitude = true;
                        }
                    }

					if (targetBody.GetName() == "Kerbin")
					{
						// Southern peninsula flights
						landmarkLatitude = -3.5;
						landmarkLongitude = -85.5;
					}
					else if (targetBody.GetName() == "Earth")
					{
						// TODO: Center on a Real Solar System landmark
						landmarkLatitude = SpaceCenter.Instance.Latitude;
						landmarkLongitude = SpaceCenter.Instance.Longitude;
					}
					else
					{
						// Unknown landmarks
						landmarkLatitude = SpaceCenter.Instance.Latitude;
						landmarkLongitude = SpaceCenter.Instance.Longitude;
					}

                    break;
			}

			if (generator.Next(0, 100) < homeNearbyChance && targetBody == Planetarium.fetch.Home)
				WaypointManager.ChooseRandomPositionNear(out centerLatitude, out centerLongitude, landmarkLatitude, landmarkLongitude, targetBody.GetName(), homeNearbyRange, true);
			else
				WaypointManager.ChooseRandomPosition(out centerLatitude, out centerLongitude, targetBody.GetName(), true, false);

			for (int x = 0; x < waypointCount; x++)
			{
				ContractParameter newParameter;
				newParameter = this.AddParameter(new FlightWaypointParameter(x, targetBody, minAltitude, maxAltitude, centerLatitude, centerLongitude, range), null);
				newParameter.SetFunds(Mathf.Round(FPConfig.Aerial.Funds.WaypointBaseReward * wpFundsMultiplier), targetBody);
                newParameter.SetReputation(Mathf.Round(FPConfig.Aerial.Reputation.WaypointBaseReward * wpReputationMultiplier), targetBody);
                newParameter.SetScience(Mathf.Round(FPConfig.Aerial.Science.WaypointBaseReward * wpScienceMultiplier), targetBody);
			}

			base.AddKeywords(new string[] { "surveyflight" });
            base.SetExpiry(FPConfig.Aerial.Expire.MinimumExpireDays, FPConfig.Aerial.Expire.MaximumExpireDays);
            base.SetDeadlineDays(FPConfig.Aerial.Expire.DeadlineDays, targetBody);
            base.SetFunds(Mathf.Round(FPConfig.Aerial.Funds.BaseAdvance * fundsMultiplier), Mathf.Round(FPConfig.Aerial.Funds.BaseReward * fundsMultiplier), Mathf.Round(FPConfig.Aerial.Funds.BaseFailure * fundsMultiplier), targetBody);
            base.SetScience(Mathf.Round(FPConfig.Aerial.Science.BaseReward * scienceMultiplier), targetBody);
            base.SetReputation(Mathf.Round(FPConfig.Aerial.Reputation.BaseReward * reputationMultiplier), Mathf.Round(FPConfig.Aerial.Reputation.BaseFailure * reputationMultiplier), targetBody);
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
			// It's weird for a space program to explore our home planet. Do test flights there instead.
            return "Perform atmospheric test flights over " + targetBody.theName + " at an altitude of " + (int)minAltitude + " to " + (int)maxAltitude + ".";
		}

		protected override string GetDescription()
		{
			//those 3 strings appear to do nothing
			return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "flying", "not crashing", "aerial", new System.Random().Next());
		}

		protected override string GetSynopsys()
		{
			if (targetBody == Planetarium.fetch.Home) {
				return "Pilot a jet to these locations to collect flight data for commercial aircraft engineering.";
			}
            return "Fly through the atmosphere of " + targetBody.theName + " to test extraplanetary aircraft designs.";
		}

		protected override string MessageCompleted()
		{
            return "You gathered valuable test flight data from " + targetBody.theName + ".";
		}

        protected override string GetNotes()
        {
            string notes = "";

            if (Util.IsGasGiant(targetBody) && !isLowAltitude)
                notes += "Warning: this contract specifies flight in the atmosphere of a gas giant.";
            else if (Util.IsGasGiant(targetBody) && isLowAltitude)
                notes += "Warning: this is a low altitude flight contract for a gas giant. We recommend sending cheap unmanned probes on a one way trip.";
            else
                return null;

            //In Gene's dialogue, the notes smush up against the parameters. Add one new line.
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                notes += "\n";

            return notes;
        }

		protected override void OnLoad(ConfigNode node)
		{
            Util.CheckForPatchReset();
			Util.LoadNode(node, "AerialContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
			Util.LoadNode(node, "AerialContract", "minAltitude", ref minAltitude, 0.0);
            Util.LoadNode(node, "AerialContract", "maxAltitude", ref maxAltitude, 10000);
            Util.LoadNode(node, "AerialContract", "centerLatitude", ref centerLatitude, 0.0);
            Util.LoadNode(node, "AerialContract", "centerLongitude", ref centerLongitude, 0.0);
            Util.LoadNode(node, "AerialContract", "isLowAltitude", ref isLowAltitude, false);
		}

		protected override void OnSave(ConfigNode node)
		{
			int bodyID = targetBody.flightGlobalsIndex;
			node.AddValue("targetBody", bodyID);
			node.AddValue("minAltitude", minAltitude);
			node.AddValue("maxAltitude", maxAltitude);
            node.AddValue("centerLatitude", centerLatitude);
            node.AddValue("centerLongitude", centerLongitude);
            node.AddValue("isLowAltitude", isLowAltitude);
		}

		public override bool MeetRequirements()
		{
            return true;
		}

		protected static bool AreWingsUnlocked()
		{
			if (Util.haveTechnology("AdvancedCanard"))
				return true;
			if (Util.haveTechnology("StandardCtrlSrf"))
				return true;
			if (Util.haveTechnology("airplaneTail"))
				return true;
			if (Util.haveTechnology("CanardController"))
				return true;
			if (Util.haveTechnology("deltaWing"))
				return true;
			if (Util.haveTechnology("noseConeAdapter"))
				return true;
			if (Util.haveTechnology("rocketNoseCone"))
				return true;
			if (Util.haveTechnology("smallCtrlSrf"))
				return true;
			if (Util.haveTechnology("standardNoseCone"))
				return true;
			if (Util.haveTechnology("sweptWing"))
				return true;
			if (Util.haveTechnology("tailfin"))
				return true;
			if (Util.haveTechnology("wingConnector"))
				return true;
			if (Util.haveTechnology("winglet3"))
				return true;

			return false;
		}
	}
}