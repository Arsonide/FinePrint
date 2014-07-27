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
    public class MobileBaseParameter : ContractParameter
    {
        public MobileBaseParameter()
        {

        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "The ground base must be mobile";
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        if (hasWheelsOnGround(FlightGlobals.ActiveVessel))
                            base.SetComplete();
                        else
                            base.SetIncomplete();
                    }
                }
            }
        }

        private bool hasWheelsOnGround(Vessel v)
        {
            bool ground = false;

            foreach (ModuleWheel wheel in v.FindPartModulesImplementing<ModuleWheel>())
            {
                if (wheel.hasMotor && wheel.part.GroundContact)
                {
                    ground = true;
                    break;
                }
            }

            return ground;
        }
    }
}