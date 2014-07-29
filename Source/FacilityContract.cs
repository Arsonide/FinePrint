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
	public class FacilityContract : Contract
	{
		CelestialBody targetBody = null;
		bool onLand = false;
		string typeString = "potato";
		int capacity = 0;

		protected override bool Generate()
		{
            if (AreFacilitiesUnlocked() == false)
                return false;

            if (ContractSystem.Instance.GetCurrentContracts<FacilityContract>().Count() >= 2)
                return false;

			System.Random generator = new System.Random(this.MissionSeed);
			//I'll use this to determine how "difficult" the mission is, and adjust the pricing at the end.
			float difficultyFactor = 0.0f;
			float scienceFactor = 1.0f;
			onLand = (generator.Next(0, 2) == 1);

			if (onLand)
				typeString = "land base";
			else
				typeString = "orbital station";

			if (this.prestige == Contract.ContractPrestige.Trivial)
			{
				List<CelestialBody> bodies;

				if (onLand)
					bodies = GetBodies_Reached(false, false);
				else
					bodies = GetBodies_Reached(true, true);

				if (bodies.Count == 0)
					return false;

				targetBody = bodies[generator.Next(0, bodies.Count)];

				if (targetBody.GetName() == "Jool" && onLand)
				{
					targetBody = Util.RandomJoolianMoon();

					if (targetBody.GetName() == "Jool" || targetBody == null)
						return false;
				}

				if (onLand)
					this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.LANDED, "base"), null);
				else
					this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "station"), null);

				this.AddParameter(new FacilitySystemsParameter(typeString), null);
				this.AddParameter(new CrewCapacityParameter(5), null);
				difficultyFactor = 5.0f / 4.0f;
				capacity = 5;

				if (Util.haveTechnology("Large.Crewed.Lab"))
				{
					if (generator.Next(0, 100) > 80)
					{
						this.AddParameter(new FacilityLabParameter(), null);
						difficultyFactor += 1.0f;
						scienceFactor = 2.0f;
					}
				}
			}
			else if (this.prestige == Contract.ContractPrestige.Significant)
			{
				List<CelestialBody> bodies;

				if (onLand)
					bodies = GetBodies_Reached(false, false);
				else
					bodies = GetBodies_Reached(false, true);

				if (bodies.Count == 0)
					return false;

				targetBody = bodies[generator.Next(0, bodies.Count)];

				if (targetBody.GetName() == "Jool" && onLand)
				{
					targetBody = Util.RandomJoolianMoon();

					if (targetBody.GetName() == "Jool" || targetBody == null)
						return false;
				}

				if (onLand)
					this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.LANDED, "base"), null);
				else
					this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "station"), null);

				this.AddParameter(new FacilitySystemsParameter(typeString), null);
				int contractCapacity = 5 + generator.Next(1, 8);
				this.AddParameter(new CrewCapacityParameter(contractCapacity), null);
				difficultyFactor = contractCapacity / 4.0f;
				capacity = contractCapacity;

				if (Util.haveTechnology("Large.Crewed.Lab"))
				{
					if (generator.Next(0, 100) > 60)
					{
						this.AddParameter(new FacilityLabParameter(), null);
						difficultyFactor += 1.0f;
						scienceFactor = 3.0f;
					}
				}
			}
			else if (this.prestige == Contract.ContractPrestige.Exceptional)
			{
				int contractCapacity = 5 + generator.Next(1, 8);

				//Prefer unreached targets for this level of difficulty.
				if (onLand)
					targetBody = GetNextUnreachedTarget(1, true, true);
				else
					targetBody = GetNextUnreachedTarget(1, false, true);

				// Player has reached all targets, use one we've reached, but bump up capacity to increase difficulty.
				if (targetBody == null)
				{
					List<CelestialBody> bodies;

					if (onLand)
						bodies = GetBodies_Reached(false, false);
					else
						bodies = GetBodies_Reached(false, true);

					if (bodies.Count == 0)
						return false;

					contractCapacity = 7 + generator.Next(1, 14);
					targetBody = bodies[generator.Next(0, bodies.Count)];
				}

				if (targetBody.GetName() == "Jool" && onLand)
				{
					targetBody = Util.RandomJoolianMoon();

					if (targetBody.GetName() == "Jool" || targetBody == null)
						return false;
				}

				if (onLand)
					this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.LANDED, "base"), null);
				else
					this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "station"), null);

				this.AddParameter(new FacilitySystemsParameter(typeString), null);
				this.AddParameter(new CrewCapacityParameter(contractCapacity), null);
				difficultyFactor = contractCapacity / 4.0f;
				capacity = contractCapacity;

				if (Util.haveTechnology("Large.Crewed.Lab"))
				{
					if (generator.Next(0, 100) > 60)
					{
						this.AddParameter(new FacilityLabParameter(), null);
						difficultyFactor += 1.0f;
						scienceFactor = 4.0f;
					}
				}

				if (Util.haveTechnology("GrapplingDevice") && !onLand)
				{
					if (generator.Next(0, 100) > 80)
					{
						this.AddParameter(new AsteroidParameter(true), null);
						difficultyFactor += 2.0f;
					}
				}

				if (AreWheelsUnlocked() && onLand)
				{
					if (generator.Next(0, 100) > 80)
					{
						this.AddParameter(new MobileBaseParameter(), null);
						difficultyFactor += 2.0f;
					}
				}
			}

			if (onLand)
				base.AddKeywords(new string[] { "groundbase" });
			else
				base.AddKeywords(new string[] { "spacestation" });

			base.SetExpiry();
			base.SetDeadlineYears(5.0f * difficultyFactor, targetBody);
			base.SetFunds(8000f * difficultyFactor, 35000f * difficultyFactor, this.targetBody);
			base.SetReputation(100f * difficultyFactor, 50f * difficultyFactor, this.targetBody);
			base.SetScience(scienceFactor + (1f * difficultyFactor), this.targetBody);
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
			if (onLand)
				return "Build a new planetary base on " + targetBody.theName + ".";
			else
			{
				if (targetBody == Planetarium.fetch.Sun)
					return "Build a new orbital station on a solar orbit.";
				else
					return "Build a new orbital station around " + targetBody.theName + ".";
			}
		}

		protected override string GetDescription()
		{
			//those 3 strings appear to do nothing

			if (onLand)
				return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "building a base", "engineering", "planetary structures", new System.Random().Next());
			else
				return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "building a station", "orbital structures", "engineering", new System.Random().Next());
		}

		protected override string GetSynopsys()
		{
			if (onLand)
				return "Build a new planetary base for this agency that can support " + Util.integerToWord(capacity) + " kerbals on the surface of " + targetBody.theName + ".";
			else
			{
				if (targetBody == Planetarium.fetch.Sun)
					return "Build a new orbital station for this agency that can support " + Util.integerToWord(capacity) + " kerbals in a solar orbit.";
				else
					return "Build a new orbital station for this agency that can support " + Util.integerToWord(capacity) + " kerbals in orbit of " + targetBody.theName + ".";
			}
		}

		protected override string MessageCompleted()
		{
			if (onLand)
				return "You have finished construction of a new planetary base on the surface of " + targetBody.theName + ".";
			else
			{
				if (targetBody == Planetarium.fetch.Sun)
					return "You have finished construction of a new orbital station on it's own orbit around the sun.";
				else
					return "You have finished construction of a new orbital station around " + targetBody.theName + ".";
			}
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "FacilityContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
			Util.LoadNode(node, "FacilityContract", "onLand", ref onLand, false);
			Util.LoadNode(node, "FacilityContract", "typeString", ref typeString, "potato");
			Util.LoadNode(node, "FacilityContract", "capacity", ref capacity, 8);
		}

		protected override void OnSave(ConfigNode node)
		{
			int bodyID = targetBody.flightGlobalsIndex;
			node.AddValue("targetBody", bodyID);
			node.AddValue("onLand", onLand);
			node.AddValue("typeString", typeString);
			node.AddValue("capacity", capacity);
		}

		public override bool MeetRequirements()
		{
            return true;
		}

		protected CelestialBody GetNextUnreachedTarget(int depth, bool removeSun, bool removeKerbin)
		{
			System.Random generator = new System.Random(MissionSeed);
			var bodies = Contract.GetBodies_NextUnreached(depth, null);

			if (bodies != null)
			{
				if (removeSun)
					bodies.Remove(Planetarium.fetch.Sun);

				if (removeKerbin)
					bodies.Remove(Planetarium.fetch.Home);

				if (bodies.Count > 0)
					return bodies[generator.Next(0, bodies.Count - 1)];
			}
			return null;
		}

		protected static bool AreFacilitiesUnlocked()
		{
			if (AreAntennaeUnlocked() && ArePortsUnlocked() && ArePowerPartsUnlocked())
				return true;

			return false;
		}

		protected static bool ArePortsUnlocked()
		{
			if (Util.haveTechnology("dockingPort1"))
				return true;

			if (Util.haveTechnology("dockingPort2"))
				return true;

			if (Util.haveTechnology("dockingPort3"))
				return true;

			if (Util.haveTechnology("dockingPortLarge"))
				return true;

			if (Util.haveTechnology("dockingPortLateral"))
				return true;

			return false;
		}

		protected static bool ArePowerPartsUnlocked()
		{
			if (Util.haveTechnology("rtg"))
				return true;

			if (Util.haveTechnology("largeSolarPanel"))
				return true;

			if (Util.haveTechnology("solarPanels1"))
				return true;

			if (Util.haveTechnology("solarPanels2"))
				return true;

			if (Util.haveTechnology("solarPanels3"))
				return true;

			if (Util.haveTechnology("solarPanels4"))
				return true;

			if (Util.haveTechnology("solarPanels5"))
				return true;

			return false;
		}

		protected static bool AreAntennaeUnlocked()
		{
			if (Util.haveTechnology("commDish"))
				return true;

			if (Util.haveTechnology("longAntenna"))
				return true;

			if (Util.haveTechnology("mediumDishAntenna"))
				return true;

			return false;
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
	}
}