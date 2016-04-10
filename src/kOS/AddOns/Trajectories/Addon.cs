﻿using System.Reflection;
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
        private bool? available = null;
        private MethodInfo trImpactMethod = null;
        public Addon(SharedObjects shared) : base("TR", shared)
        {
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("IMPACTPOS", new Suffix<kOS.Suffixed.GeoCoordinates>(GetImpactPos, "Get impact position coordinates. Returns LATLNG(0,0) if no position is available."));
        }

        private kOS.Suffixed.GeoCoordinates GetImpactPos()
        {
            if (Available() == true)
            {
                var ship = FlightGlobals.ActiveVessel;
                var body = ship.orbit.referenceBody;

                Vector3? impactPos = (Vector3?)trImpactMethod.Invoke(null, new object[] { });
                if (impactPos != null)
                {
                    var worldImpactPos = (Vector3d)impactPos + body.position;
                    var lat = body.GetLatitude(worldImpactPos);
                    var lng = body.GetLongitude(worldImpactPos);
                    while (lng < -180)
                        lng += 360;
                    while (lng > 180)
                        lng -= 360;
                    return new kOS.Suffixed.GeoCoordinates(shared, lat, lng);
                }
                else {
                    return new kOS.Suffixed.GeoCoordinates(shared, 0, 0);
                }
            } else
            {
                return new kOS.Suffixed.GeoCoordinates(shared, 0, 0);
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
            } else// if (available == null) check for Trajectories mod via reflection and init trImpactMethod
            {
                Type trajectoriesType = AssemblyLoader.loadedAssemblies
                    .Select(a => a.assembly.GetExportedTypes())
                    .SelectMany(t => t)
                    .FirstOrDefault(t => t.FullName == "Trajectories.API"); // Equivalent to Type.GetType("Trajectories.API") except it works
                if (trajectoriesType == null)
                {
                    Debug.Log("Trajectories.API Type Null. Trajectories not installed or wrong version.");
                    available = false;
                    return false;
                }

                MethodInfo trAvailableMethod = trajectoriesType.GetMethod("APIAvailable");
                trImpactMethod = trajectoriesType.GetMethod("impactPosition");
                if (trAvailableMethod == null || trImpactMethod == null)
                {
                    Debug.Log("Trajectories.API APIAvailable or impactPosition method is null. Incompatible Trajectories version");
                    available = false;
                    return false;
                }

                object trajectoriesAPIInstance = Activator.CreateInstance(trajectoriesType);
                object value = trAvailableMethod.Invoke(trajectoriesAPIInstance, new object[] { });

                if ((bool)value == true)
                {
                    available = true;
                    return true;
                } else
                {
                    Debug.Log("Trajectories probably installed but not working maybe, idk.");
                    available = false;
                    return false;
                }
            }
        }
    }
}