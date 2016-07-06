namespace Common
{
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
        public LocationAngle xRotation { get; set; }
        public LocationAngle yRotation { get; set; }
        public LocationAngle zRotation { get; set; }
    }
}