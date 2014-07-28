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
	public class ARMContract : Contract
	{
		CelestialBody targetBody = null;

		protected override bool Generate()
		{
			if (this.prestige == Contract.ContractPrestige.Trivial)
			{
				targetBody = Planetarium.fetch.Home;
			}
			else if (this.prestige == Contract.ContractPrestige.Significant)
			{
				List<CelestialBody> bodies = GetBodies_Reached(true, false);

				if (bodies.Count == 0)
					return false;

				targetBody = bodies[UnityEngine.Random.Range(0, bodies.Count)];

				if (targetBody.GetName() == "Jool")
				{
					targetBody = Util.RandomJoolianMoon();

					if (targetBody.GetName() == "Jool" || targetBody == null)
						return false;
				}
			}
			else if (this.prestige == Contract.ContractPrestige.Exceptional)
			{
				targetBody = Planetarium.fetch.Sun;
			}

			this.AddParameter(new AsteroidParameter(false), null);

			if (targetBody == Planetarium.fetch.Sun)
			{
				this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ESCAPING, "vessel"), null);
			}
			else
			{
				this.AddParameter(new LocationAndSituationParameter(targetBody, Vessel.Situations.ORBITING, "vessel"), null);
			}

			base.AddKeywords(new string[] { "asteroidretrieval" });
			base.SetExpiry();
			base.SetDeadlineYears(7.0f, targetBody);

			if (this.prestige == Contract.ContractPrestige.Trivial)
			{
				base.SetFunds(10000f, 60000f, this.targetBody);
				base.SetReputation(300f, 150f, this.targetBody);
				base.SetScience(150f, this.targetBody);
			}
			else if (this.prestige == Contract.ContractPrestige.Significant)
			{
				base.SetFunds(10000f, 60000f, this.targetBody);
				base.SetReputation(300f, 150f, this.targetBody);
				base.SetScience(150f, this.targetBody);
			}
			else
			{
				//targetBody is the Sun, and that modifies the rewards, so the values here need to actually be smaller.
				base.SetFunds(2500f, 15000f, this.targetBody);
				base.SetReputation(75f, 40f, this.targetBody);
				base.SetScience(50f, this.targetBody);
			}

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
			if (this.prestige == Contract.ContractPrestige.Trivial)
				return "Bring an asteroid into an orbit around Kerbin.";
			else if (this.prestige == Contract.ContractPrestige.Significant)
				return "Bring an asteroid into an orbit around " + targetBody.GetName() + ".";
			else
				return "Bring an asteroid into an escape trajectory out of the solar system.";
		}

		protected override string GetDescription()
		{
			//those 3 strings appear to do nothing
			return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "retrieving an asteroid", "snooker", "large rocks", new System.Random().Next());
		}

		protected override string GetSynopsys()
		{
			if (this.prestige == Contract.ContractPrestige.Trivial)
				return "Capture an asteroid, then bring it into a stable orbit around Kerbin for further study.";
			else if (this.prestige == Contract.ContractPrestige.Significant)
			{
				switch (UnityEngine.Random.Range(0, 3))
				{
					case 0:
						return "Capture an asteroid, then bring it into a stable orbit around " + targetBody.GetName() + " to test our capabilities.";
					case 1:
						return "Capture an asteroid, then bring it into a stable orbit around " + targetBody.GetName() + ". Why? FOR SCIENCE!";
					default:
						return "Mission control says low Kerbin orbit is getting a bit crowded. Capture an asteroid and take it into orbit around " + targetBody.GetName() + " instead.";
				}
			}
			else
			{
				switch (UnityEngine.Random.Range(0, 3))
				{
					case 0:
						return "Capture an asteroid, then put it on an extrasolar trajectory. The less of these things orbiting the sun, the better.";
					case 1:
						return "The last asteroid that passed Kerbin nearly wiped out our species, capture one and get rid of it.";
					default:
						return "How do you feel about throwing a large rock out of the solar system?";
				}
			}
		}

		protected override string MessageCompleted()
		{
			if (this.prestige == Contract.ContractPrestige.Trivial)
				return "You successfully captured an asteroid and brought it home.";
			else if (this.prestige == Contract.ContractPrestige.Significant)
				return "You successfully captured an asteroid and brought it to " + targetBody.GetName() + ".";
			else
			{
				switch (UnityEngine.Random.Range(0, 3))
				{
					case 0:
						return "You successfuly flung a rock out of the solar system.";
					case 1:
						return "That asteroid is OUTTA HERE!";
					default:
						return "Looks like the asteroid is on a solar escape trajectory. Mission complete.";
				}
			}
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "ARMContract", "targetBody", ref targetBody, Planetarium.fetch.Home);
		}

		protected override void OnSave(ConfigNode node)
		{
			int bodyID = targetBody.flightGlobalsIndex;
			node.AddValue("targetBody", bodyID);
		}

		public override bool MeetRequirements()
		{
			if (Util.haveTechnology("GrapplingDevice") == false)
				return false;

            return true;
		}

		protected static CelestialBody GetNextUnreachedTarget(int depth, bool removeSun, bool removeKerbin)
		{
			var bodies = Contract.GetBodies_NextUnreached(depth, null);
			if (bodies != null)
			{
				if (removeSun)
					bodies.Remove(Planetarium.fetch.Sun);

				if (removeKerbin)
					bodies.Remove(Planetarium.fetch.Home);

				if (bodies.Count > 0)
					return bodies[UnityEngine.Random.Range(0, bodies.Count - 1)];
			}
			return null;
		}
	}
}