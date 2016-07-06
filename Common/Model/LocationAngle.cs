using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class LocationAngle
    {
        private readonly int MAX_ANGLE = 359;
        private readonly int MIN_ANGLE = 0;

        private double _angle;
        public double Value
        {
            get { return _angle; }

            set
            {
                var floorAngle = Math.Floor(value);
                if (floorAngle < MAX_ANGLE ||
                    floorAngle > MIN_ANGLE)
                {
                    _angle = value;
                }
                else
                {
                    _angle = value % MAX_ANGLE;
                }
            }
        }
    }
}
