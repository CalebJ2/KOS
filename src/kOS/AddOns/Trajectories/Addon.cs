using System.Reflection;
using kOS.Safe.Encapsulation.Suffixes;
using System.Linq;
using UnityEngine;
using System;

namespace kOS.AddOns.TrajectoriesAddon
{
    public class Addon : Suffixed.Addon
    {
        public Addon(SharedObjects shared) : base("TR", shared)
        {
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("IMPACTPOS", new Suffix<kOS.Suffixed.Vector>(GetImpactPos, "Get impact position. Returns vector(lat,1,lng) z=0 if position isn't available"));
        }

        private kOS.Suffixed.Vector GetImpactPos()
        {
            if (Available() == false)
            {
                return new kOS.Suffixed.Vector(0, 0, 0);
            } else
            {
                return TrajectoryImpactPos();
            }
        }
        private kOS.Suffixed.Vector TrajectoryImpactPos()
        {
            var ship = FlightGlobals.ActiveVessel;
            var body = ship.orbit.referenceBody;

            Trajectories.Trajectory myTrajectory = Trajectories.Trajectory.fetch;
            myTrajectory.Update();
            foreach (var patch in myTrajectory.patches)
            {
                if (patch.impactPosition.HasValue)
                {
                    var worldImpactPos = patch.impactPosition.Value + body.position;
                    var lat = body.GetLatitude(worldImpactPos);
                    var lng = body.GetLongitude(worldImpactPos);
                    while (lng < -180)
                        lng += 360;
                    while (lng > 180)
                        lng -= 360;
                    return new kOS.Suffixed.Vector(lat, 1, lng);
                }
            }
            return new kOS.Suffixed.Vector(0, 0, 0);
        }
        public override bool Available()
        {
            Type TrType = null;//Type.GetType("Trajectories.Trajectory");
            TrType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Trajectories.Trajectory");
            //Debug.Log("Trajectories.Trajectory Type: " + TrType + ", null?: " + (TrType == null));
            if (TrType == null) return false;

            MethodInfo myMethod = TrType.GetMethod("TrajectoriesInstalled");
            //Debug.Log("TrajectoriesInstalled method info: " + myMethod + ", null?: " + (myMethod == null));
            if (myMethod == null) return false;

            object myTrajectory = Activator.CreateInstance(TrType);
            object value = myMethod.Invoke(myTrajectory, new object[]{});
            //Debug.Log("Instance value: " + value + ", null?: " + (value == null));

            if ((bool)value == true) {
                return true;
            } else
            {
                return false;
            }
        }

    }
}