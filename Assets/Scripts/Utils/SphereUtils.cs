using SpaceGame.Celestial;
using UnityEngine;

namespace SpaceGame.Utils
{
    public static class SphereUtils
    {
        /*!
         * Calculate the gravityUp with respect to the given transform.
         */

        public static Vector3 GravityUp(Transform transform, Transform planet) => (transform.position - planet.position).normalized;

        /*!
         * Look at a given target with respect to the planets surface.
         */

        public static void LookAtTarget(Transform transform, Transform planet, Vector3 target)
        {
            var gravityUp = GravityUp(transform, planet);

            var forward = Vector3.ProjectOnPlane(target - transform.position, gravityUp);
            if (forward != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(forward, gravityUp);
        }

        /*!
         * Align a transform to planets surface and rotate towards a specific target on that planet surface.
         */

        public static void AlignToSphereSurface(Transform transform, Transform planet)
        {
            var gravityUp = GravityUp(transform, planet);

            // Snap back to planets surface
            var planetRadius = planet.GetComponent<Planet>().shapeSettings.radius;
            transform.position = gravityUp * (planetRadius + 1);
        }

        public static Vector3 SphericalToCartesian(Vector3 sphericalCoord)
        {
            return SphericalToCartesian(sphericalCoord.x, sphericalCoord.y, sphericalCoord.z);
        }

        /*!
         * Returns new Vector3(x, y, z)
         */

        public static Vector3 SphericalToCartesian(float radius, float polar, float azimuthal)
        {
            float a = radius * Mathf.Cos(azimuthal);

            Vector3 result = new Vector3();
            result.x = a * Mathf.Cos(polar);
            result.y = radius * Mathf.Sin(azimuthal);
            result.z = a * Mathf.Sin(polar);

            return result;
        }

        /*!
         * Returns new Vector3(radius, polar, azimuthal)
         */

        public static Vector3 CartesianToSpherical(Vector3 cartCoords)
        {
            float radius, polar, azimuthal;

            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;

            radius = Mathf.Sqrt((cartCoords.x * cartCoords.x) + (cartCoords.y * cartCoords.y) + (cartCoords.z * cartCoords.z));
            polar = Mathf.Atan(cartCoords.z / cartCoords.x);

            if (cartCoords.x < 0)
                polar += Mathf.PI;
            azimuthal = Mathf.Asin(cartCoords.y / radius);

            Vector3 result = new Vector3(radius, polar, azimuthal);
            return result;
        }

        /*!
         * Gets the center point given 3 vertices.
         */

        public static Vector3 GetCenterPoint(Vector3 a, Vector3 b, Vector3 c) => (a + b + c) / 3;

        /*!
         * Returns the midpoint given two given vertices.
         */

        public static Vector3 GetMidPointVertex(Vector3 a, Vector3 b) => (a + b) / 2;
    }
}