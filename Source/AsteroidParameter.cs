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
        private int roidSeed;
        private bool asteroidTowed;

		public AsteroidParameter()
		{
			this.forStation = false;
            this.asteroidTowed = false;
            this.roidSeed = 0;
		}

		public AsteroidParameter(bool forStation)
		{
			this.forStation = forStation;
            this.asteroidTowed = false;
            this.roidSeed = 0;
		}

		protected override string GetHashString()
		{
			return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
		}

		protected override string GetTitle()
		{
			if (forStation)
				return "Build the facility into a newly discovered asteroid";
			else
				return "Have a newly discovered asteroid in tow";
		}

		protected override void OnRegister()
		{
            this.DisableOnStateChange = false;

            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onPartCouple.Add(OnDock);
                GameEvents.onFlightReady.Add(OnFlight);
            }
        }

        protected override void OnUnregister()
        {
            if (Root.ContractState == Contract.State.Active)
            {
                GameEvents.onPartCouple.Remove(OnDock);
                GameEvents.onFlightReady.Remove(OnFlight);

            }
        }

        private void OnFlight()
        {
            bool findSameRoid = false;

            List<ModuleAsteroid> asteroids = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>();

            if (asteroids.Count() > 0)
            {
                foreach (ModuleAsteroid asteroid in asteroids)
                {
                    if (asteroid.seed == roidSeed)
                        findSameRoid = true;
                }
            }

            asteroidTowed = findSameRoid;
        }

        private void OnDock(GameEvents.FromToAction<Part, Part> action)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                List<ModuleAsteroid> asteroids = action.from.vessel.FindPartModulesImplementing<ModuleAsteroid>();

                if (asteroids.Count() > 0)
                {
                    foreach (ModuleAsteroid asteroid in asteroids)
                    {
                        if (asteroid.seed == roidSeed)
                            asteroidTowed = true;
                    }

                    if (FlightGlobals.ActiveVessel.mainBody == Planetarium.fetch.Sun)
                    {
                        foreach (ModuleAsteroid asteroid in asteroids)
                        {
                            roidSeed = asteroid.seed;
                            asteroidTowed = true;
                        }
                    }
                }
            }
        }

		protected override void OnSave(ConfigNode node)
		{
			node.AddValue("forStation", forStation);
            node.AddValue("roidSeed", roidSeed);
            node.AddValue("asteroidTowed", asteroidTowed);
		}

		protected override void OnLoad(ConfigNode node)
		{
			Util.LoadNode(node, "AsteroidParameter", "forStation", ref forStation, false);
            Util.LoadNode(node, "AsteroidParameter", "roidSeed", ref roidSeed, 0);
            Util.LoadNode(node, "AsteroidParameter", "asteroidTowed", ref asteroidTowed, false);
		}

		protected override void OnUpdate()
		{
			if (this.Root.ContractState == Contract.State.Active)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (FlightGlobals.ready)
					{
                        checkSameRoid(FlightGlobals.ActiveVessel);

						if (this.State == ParameterState.Incomplete)
						{
                            if (asteroidTowed)
								base.SetComplete();
						}

						if (this.State == ParameterState.Complete)
						{
                            if (!asteroidTowed)
								base.SetIncomplete();
						}
					}
				}
			}
		}

        private void checkSameRoid(Vessel v)
        {
            if (asteroidTowed == false)
                return;

            asteroidTowed = false;

            foreach (ModuleAsteroid asteroid in v.FindPartModulesImplementing<ModuleAsteroid>())
            {
                if (asteroid.seed == roidSeed)
                {
                    asteroidTowed = true;
                    break;
                }
            }
        }
	}
}