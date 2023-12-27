using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module;

namespace Festa.Client.MapBox
{
    class MBPropertyValue
    {
        //motorway        // High-speed, grade-separated highways
        //motorway_link   // Link roads/lanes/ramps connecting to motorways
        //trunk           // Important roads that are not motorways.
        //trunk_link      // Link roads/lanes/ramps connecting to trunk roads
        //primary         // A major highway linking large towns.
        //primary_link    //    Link roads/lanes connecting to primary roads
        //secondary       //   A highway linking large towns.
        //secondary_link  // Link roads/lanes connecting to secondary roads
        //tertiary        //    A road linking small settlements, or the local centres of a large town or city.
        //tertiary_link   // Link roads/lanes connecting to tertiary roads
        //street          //  Standard unclassified, residential, road, and living_street road types
        //street_limited  // Streets that may have limited or no access for motor vehicles.
        //pedestrian      // Includes pedestrian streets, plazas, and public transportation platforms.
        //construction    //    Includes motor roads under construction (but not service roads, paths, etc).
        //track           // Roads mostly for agricultural and forestry use etc.
        //service         // Access roads, alleys, agricultural tracks, and other services roads.Also includes parking lot aisles, public & private driveways.
        //ferry           // Those that serves automobiles and no or unspecified automobile service.
        //path            // Foot paths, cycle paths, ski trails.
        //major_rail      // Railways, including mainline, commuter rail, and rapid transit.
        //minor_rail      //  Includes light rail & tram lines.
        //service_rail    // Yard and service railways.
        //aerialway       // Ski lifts, gondolas, and other types of aerialway.
        //golf            // The approximate centerline of a golf course hole
        //roundabout      //  Circular continuous-flow intersection
        //mini_roundabout // Smaller variation of a roundabout with no center island or obstacle
        //turning_circle  //  (point) Widened section at the end of a cul-de-sac for turning around a vehicle
        //turning_loop    //    (point) Similar to a turning circle but with an island or other obstruction at the centerpoint
        //traffic_signals // (point) Lights or other signal controlling traffic flow at an intersection
        //junction        //    (point) A point indication for road junctions
        //intersection    //    (point) Indicating the class and type of roads meeting at an intersection.Intersections are only available in Japan

        public static int motorway = EncryptUtil.makeHashCode("motorway");
        public static int motorway_link = EncryptUtil.makeHashCode("motorway_link");
        public static int street = EncryptUtil.makeHashCode("street");
        public static int minor = EncryptUtil.makeHashCode("minor");
        public static int primary = EncryptUtil.makeHashCode("primary");
        public static int secondary = EncryptUtil.makeHashCode("secondary");
        public static int service = EncryptUtil.makeHashCode("service");
        public static int tertiary = EncryptUtil.makeHashCode("tertiary");
        public static int track = EncryptUtil.makeHashCode("track");
        public static int trunk = EncryptUtil.makeHashCode("trunk");
        public static int path = EncryptUtil.makeHashCode("path");
        public static int ferry = EncryptUtil.makeHashCode("ferry");
        public static int pedestrian = EncryptUtil.makeHashCode("pedestrian");
        public static int aerialway = EncryptUtil.makeHashCode("aerialway");
        public static int service_rail = EncryptUtil.makeHashCode("service_rail");
        public static int major_rail = EncryptUtil.makeHashCode("major_rail");
        public static int minor_rail = EncryptUtil.makeHashCode("minor_rail");
        public static int primary_link = EncryptUtil.makeHashCode("primary_link");
        public static int secondary_link = EncryptUtil.makeHashCode("secondary_link");
        public static int tertiary_link = EncryptUtil.makeHashCode("tertiary_link");
        public static int trunk_link = EncryptUtil.makeHashCode("trunk_link");
        public static int street_limited = EncryptUtil.makeHashCode("street_limited");
    }
}
