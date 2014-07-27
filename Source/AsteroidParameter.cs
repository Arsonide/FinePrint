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

        public AsteroidParameter()
        {
            this.forStation = false;
        }

        public AsteroidParameter(bool forStation)
        {
            this.forStation = forStation;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            if (forStation)
                return "Build the facility into an asteroid";
            else
                return "Have an asteroid in tow";
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("forStation", forStation);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "AsteroidParameter", "forStation", ref forStation, false);
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        if (hasARoid(FlightGlobals.ActiveVessel))
                            base.SetComplete();
                        else
                            base.SetIncomplete();
                    }
                }
            }
        }

        private bool hasARoid(Vessel v)
        {
            bool roid = false;

            foreach (ModuleAsteroid asteroid in v.FindPartModulesImplementing<ModuleAsteroid>())
            {
                roid = true;
                break;
            }

            return roid;
        }
    }
}