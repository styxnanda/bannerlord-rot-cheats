using System;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Patches
{
    /// <summary>
    /// Hotfix patches for custom siege scenes (such as Winterfell in Realm of Thrones)
    /// where deployment boundary polygons or navigation mesh zones are null, incomplete,
    /// or cause null-pointer exceptions during the initial troop spawn / deployment phase.
    /// </summary>
    [HarmonyPatch(typeof(MBMath), "IntersectRayWithPolygon")]
    public static class IntersectRayWithPolygonPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Vec2 rayOrigin, Vec2 rayDir, MBList<Vec2> polygon, out Vec2 intersectionPoint, ref bool __result)
        {
            if (polygon == null || polygon.Count == 0)
            {
                intersectionPoint = rayOrigin;
                __result = false;
                return false; // Safely skip original method, preventing NullReferenceException
            }

            intersectionPoint = rayOrigin;
            return true;
        }
    }

    [HarmonyPatch(typeof(MBSceneUtilities), "IsPointInsideBoundaries")]
    public static class MBSceneUtilitiesIsPointInsideBoundariesPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Vec2 point, MBList<Vec2> boundaries, float acceptanceThreshold, ref bool __result)
        {
            if (boundaries == null || boundaries.Count <= 2)
            {
                __result = false;
                return false; // Safely skip original method, preventing NullReferenceException
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(DefaultTeamDeploymentPlan), "GetPathDeploymentBoundaryIntersection")]
    public static class TeamDeploymentPlanBoundaryIntersectionPatch
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, ref bool __result)
        {
            if (__exception != null)
            {
                __result = false;
                return null; // Suppress exception and let pathfinding fallback safely
            }

            return null;
        }
    }

    [HarmonyPatch(typeof(DefaultMissionDeploymentPlan), "ProjectPositionToDeploymentBoundaries")]
    public static class MissionDeploymentPlanProjectPatch
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                return null; // Suppress exception so the unit remains at spawned position
            }

            return null;
        }
    }
}
