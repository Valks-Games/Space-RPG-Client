using UnityEngine;

namespace SpaceGame.Celestial
{
    public interface INoiseFilter
    {
        float Evaluate(Vector3 point);
    }
}