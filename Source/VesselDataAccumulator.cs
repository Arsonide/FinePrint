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

namespace FinePrint
{
    //Instantiate around a stub vessel to get more information
    class VesselDataAccumulator {

        public Vessel vessel;
        public Dictionary<string, ResourceData> resources;

        public double dryMass;
        public double mass;

        public enum CommandStatuses { none, seat, pod}
        public CommandStatuses commandStatus = CommandStatuses.none;

        public int crewCount, crewCapacity;
        public List<string> crewMembers;

        bool debug = false;

        public VesselDataAccumulator(Vessel vessel) {
            this.vessel = vessel;
            ProtoVessel proto = vessel.protoVessel;
            if (debug)
                Debug.Log("Looking at " + vessel.GetName());

            crewMembers = proto.GetVesselCrew().Select(k => k.name).ToList();
            crewCount = crewMembers.Count();
            resources = new Dictionary<string, ResourceData>();

            foreach (var p in proto.protoPartSnapshots)
                VisitPart(p);
            mass = dryMass + resources.Values.Select(r => r.GetMass()).Sum();
            if (debug)
                Dump();
        }

        private void VisitPart(ProtoPartSnapshot p) {

            dryMass += p.mass;

            crewCapacity += p.partInfo.partPrefab.CrewCapacity;


            foreach (var r in p.resources) {
                ResourceData d;
                if (resources.ContainsKey(r.resourceName))
                    d = resources[r.resourceName];
                else {
                    d = new ResourceData(r.resourceName);
                    resources[r.resourceName] = d;
                }
                var v = r.resourceValues;
                Util.LoadNode(v, "VesselDataAccumulator", "amount", ref d.current, 0);
                Util.LoadNode(v, "VesselDataAccumulator", "maxAmount", ref d.max, 0);
            }


            foreach (var m in p.modules) { // TODO: look for engines, decouplers
                if (commandStatus != CommandStatuses.pod)
                {
                    if (m.moduleName == "ModuleCommand")
                    {
                        commandStatus = CommandStatuses.pod;
                    }
                    else if (m.moduleName == "KerbalSeat")
                    {
                        commandStatus = CommandStatuses.seat;
                    }
                }
            }

        }

        public void Dump() {
            string dump = "mass " + mass + " dry " + dryMass + "\n" + "crew " + crewCount + "/" + crewCapacity;

            if (crewCount > 0)
                dump = dump + crewMembers.Aggregate((acc, e) => acc + ", " + e) + "\n";

            if (resources.Count() > 0)
                dump = dump + resources.Values.Select(e => e.name + ": " + e.current + "/" + e.max + "\n").Aggregate((a,b) => a + b);

            Debug.Log(dump);
        }
    }
    class ResourceData {
        public string name = "";
        public PartResourceDefinition def;
        public double current = 0;
        public double max = 0;
        public ResourceData(string name) {
            this.name = name;
            this.def = PartResourceLibrary.Instance.GetDefinition(name);
        }
        public double GetMass() {
            return def == null ? 0 : def.density * current;
        }
    }
}

