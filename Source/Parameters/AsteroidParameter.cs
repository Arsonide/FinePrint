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
	public class AsteroidParameter : ContractParameter
	{
		private bool forStation;
        private List<int> asteroidSeeds;
        private string asteroidClass;
        private int successCounter;

		public AsteroidParameter()
		{
			this.forStation = false;
            this.successCounter = 0;
            this.asteroidClass = "A";
		}

		public AsteroidParameter(string size, bool forStation)
		{
			this.forStation = forStation;
            this.successCounter = 0;
            this.asteroidClass = size.ToUpper();
		}

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			if (forStation)
				return "Build the facility into a newly discovered Class " + asteroidClass + " asteroid";
			else
				return "Have a newly discovered Class " + asteroidClass + " asteroid in tow";
		}

		protected override void OnRegister()
		{
            this.DisableOnStateChange = false;

            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onPartCouple.Add(OnDock);
                GameEvents.onFlightReady.Add(FlightReady);
                GameEvents.onVesselChange.Add(VesselChange);
            }
        }

        protected override void OnUnregister()
        {
            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onPartCouple.Remove(OnDock);
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

        private void OnDock(GameEvents.FromToAction<Part, Part> action)
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        foreach (ModuleAsteroid asteroid in action.from.vessel.FindPartModulesImplementing<ModuleAsteroid>())
                        {
                            if (asteroid.vessel.DiscoveryInfo.Level.ToString() != "Owned")
                            {
                                //This is a new asteroid. Keep track of any new asteroids in a list.
                                if (asteroid.vessel.DiscoveryInfo.size.Value == asteroidClass)
                                {
                                    addAsteroid(asteroid.seed);
                                    ScreenMessages.PostScreenMessage("This asteroid fits " + this.Root.Agent.Name + "'s requirements.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                }
                                else
                                    ScreenMessages.PostScreenMessage("This asteroid is not the size that " + this.Root.Agent.Name + " requested.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                            }
                            else
                            {
                                bool isNew = false;
                                foreach (int savedSeed in asteroidSeeds)
                                {
                                    if (asteroid.seed == savedSeed)
                                        isNew = true;
                                }

                                if (!isNew)
                                    ScreenMessages.PostScreenMessage(this.Root.Agent.Name + " notes that this is not a newly discovered asteroid.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                else
                                    ScreenMessages.PostScreenMessage("This asteroid fits " + this.Root.Agent.Name + "'s requirements.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                            }
                        }
                    }
                }
            }
        }

		protected override void OnSave(ConfigNode node)
		{
			node.AddValue("forStation", forStation);
            node.AddValue("asteroidClass", asteroidClass);

            if (asteroidSeeds == null)
            {
                node.AddValue("seedList", "");
                return;
            }

            node.AddValue("seedList", packSeeds());
		}

		protected override void OnLoad(ConfigNode node)
		{
            string seedList = "";
            Util.LoadNode(node, "AsteroidParameter", "forStation", ref forStation, false);
            Util.LoadNode(node, "AsteroidParameter", "asteroidClass", ref asteroidClass, "A");
            Util.LoadNode(node, "AsteroidParameter", "seedList", ref seedList, "");
            unpackStrings(seedList);
        }

		protected override void OnUpdate()
		{
			if (this.Root.ContractState == Contract.State.Active)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (FlightGlobals.ready)
					{
						if (this.State == ParameterState.Incomplete)
						{
                            if (isTowingAsteroid(FlightGlobals.ActiveVessel))
                                successCounter++;
                            else
                                successCounter = 0;

                            if ( successCounter >= Util.frameSuccessDelay )
                                base.SetComplete();
						}

						if (this.State == ParameterState.Complete)
						{
                            if (!isTowingAsteroid(FlightGlobals.ActiveVessel))
								base.SetIncomplete();
						}
					}
				}
			}
		}

        private bool isTowingAsteroid(Vessel v)
        {
            if (asteroidSeeds == null)
                return false;

            foreach (ModuleAsteroid asteroid in v.FindPartModulesImplementing<ModuleAsteroid>())
            {
                foreach (int savedSeed in asteroidSeeds)
                {
                    if (asteroid.seed == savedSeed)
                        return true;
                }
            }

            return false;
        }

        private string packSeeds()
        {
            string result = "";

            if (asteroidSeeds != null)
            {
                List<string> tempSeeds = asteroidSeeds.ConvertAll<string>(x => x.ToString());
                result = string.Join("|", tempSeeds.ToArray());
            }

            return result;
        }

        private void unpackStrings(string packed)
        {
            if (!string.IsNullOrEmpty(packed))
            {
                List<string> unpacked = packed.Split('|').ToList();

                if (unpacked.Count > 0)
                {
                    foreach (string temp in unpacked)
                    {
                        int seed;
                        if (int.TryParse(temp, out seed))
                        {
                            addAsteroid(seed);
                        }
                    }
                }
            }
        }

        private void addAsteroid(int seed)
        {
            if (asteroidSeeds == null)
                asteroidSeeds = new List<int>();

            asteroidSeeds.Add(seed);
        }
	}
}