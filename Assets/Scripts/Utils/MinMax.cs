namespace SpaceGame.Utils
{
    public class MinMax
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public MinMax()
        {
            ResetValues();
        }

        public void AddValue(float v)
        {
            if (v > Max)
            {
                Max = v;
            }
            if (v < Min)
            {
                Min = v;
            }
        }

        public void ResetValues()
        {
            Min = float.MaxValue;
            Max = float.MinValue;
        }
    }
}