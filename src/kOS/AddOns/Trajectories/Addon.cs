using kOS.Safe.Encapsulation.Suffixes;

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
            AddSuffix("IMPACTPOS", new Suffix<kOS.Suffixed.Vector>(GetImpactPos, "Get impact position. vector(lat,1,lng) z=0 if no pos"));
        }

        private kOS.Suffixed.Vector GetImpactPos()
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
            return true;
        }

    }
}