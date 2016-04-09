using System.Reflection;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using System.Linq;
using UnityEngine;
using System;

namespace kOS.AddOns.TrajectoriesAddon
{
    [kOS.Safe.Utilities.KOSNomenclature("TRAddon")]
    public class Addon : Suffixed.Addon
    {
        //Code is arranged to never use Trajectories.Trajectory in a function unless it is sure a compatible Trajectories version is installed.
        private static bool? available = null;
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
            if (Available() == true)
            {
                return TrajectoryImpactPos();
            } else
            {
                return new kOS.Suffixed.Vector(0, 0, 0);
            }
        }
        public override BooleanValue Available()
        {
            if (available == true)
            {
                return true;
            } else if (available == false)
            {
                return false;
            } else// if (available == null)
            {               
                Type TrType = AssemblyLoader.loadedAssemblies
                    .Select(a => a.assembly.GetExportedTypes())
                    .SelectMany(t => t)
                    .FirstOrDefault(t => t.FullName == "Trajectories.Trajectory"); // Equivalent to Type.GetType("Trajectories.Trajectory") except it works;
                //Debug.Log("Trajectories.Trajectory Type: " + TrType + ", null?: " + (TrType == null));
                if (TrType == null)
                {
                    available = false;
                    return false;
                }

                MethodInfo trMethod = TrType.GetMethod("TrajectoriesInstalled");
                //Debug.Log("TrajectoriesInstalled method info: " + myMethod + ", null?: " + (trMethod == null));
                if (trMethod == null)
                {
                    available = false;
                    return false;
                }

                object myTrajectory = Activator.CreateInstance(TrType);
                object value = trMethod.Invoke(myTrajectory, new object[] { });
                //Debug.Log("Instance value: " + value + ", null?: " + (value == null));

                if ((bool)value == true)
                {
                    available = true;
                    return true;
                } else
                {
                    available = false;
                    return false;
                }
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
    }
}