using System.Collections.Generic;
using UnityEngine;

namespace World
{
    /// <summary>
    ///     Represents an object in the world with geospatial coordinates, dimensions,
    ///     metadata, and a reference to its GameObject.
    /// </summary>
    public class WorldObject : MonoBehaviour
    {
        /// <summary>
        ///     Defines categories of world objects. Can be expanded to include more types.
        /// </summary>
        public enum WorldObjectType
        {
            Player,
            POI
        }

        private double altitude;
        private double height; // for y.
        private double latitude;
        private double longitude;

        private Dictionary<string, string>
            metadata; // additional String:String metadata incase more is needed - future proofing. 

        private string name; // a display name for the object i guess?
        private GameObject obj;

        private WorldObjectType
            type; // can be used for the player or points of interest on the map, good to specify what.

        // do these need to be doubles? for now it is fine.
        private double width; // for x and z, assumed circular/square.

        /// <summary>
        ///     Creates a new WorldObject with default values and an empty metadata dictionary.
        /// </summary>
        public WorldObject()
        {
            metadata = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Creates a new WorldObject with the specified properties.
        /// </summary>
        /// <param name="name">A display name for the object.</param>
        /// <param name="type">The world object category (e.g., Player or POI).</param>
        /// <param name="obj">The Unity GameObject associated with this world object.</param>
        /// <param name="lon">Longitude in decimal degrees.</param>
        /// <param name="lat">Latitude in decimal degrees.</param>
        /// <param name="altitude">Altitude in meters.</param>
        /// <param name="width">Width (X/Z) in meters.</param>
        /// <param name="height">Height (Y) in meters.</param>
        public WorldObject(string name, WorldObjectType type, GameObject obj, double lon, double lat, double altitude,
            double width, double height) : this()
        {
            this.name = name;
            this.type = type;
            this.obj = obj;
            longitude = lon;
            latitude = lat;
            this.altitude = altitude;
            this.width = width;
            this.height = height;
        }

        // getters and setters.

        /// <summary>
        ///     Gets the longitude value.
        /// </summary>
        /// <returns>The longitude in decimal degrees.</returns>
        public double GetLongitude()
        {
            return longitude;
        }

        /// <summary>
        ///     Sets the longitude value.
        /// </summary>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        public void SetLongitude(double longitude)
        {
            this.longitude = longitude;
        }

        /// <summary>
        ///     Gets the latitude value.
        /// </summary>
        /// <returns>The latitude in decimal degrees.</returns>
        public double GetLatitude()
        {
            return latitude;
        }

        /// <summary>
        ///     Sets the latitude value.
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        public void SetLatitude(double latitude)
        {
            this.latitude = latitude;
        }

        /// <summary>
        ///     Gets the altitude value.
        /// </summary>
        /// <returns>Altitude in meters.</returns>
        public double GetAltitude()
        {
            return altitude;
        }

        /// <summary>
        ///     Sets the altitude value.
        /// </summary>
        /// <param name="altitude">Altitude in meters.</param>
        public void SetAltitude(double altitude)
        {
            this.altitude = altitude;
        }

        /// <summary>
        ///     Gets the width (X/Z) value.
        /// </summary>
        /// <returns>Width in meters.</returns>
        public double GetWidth()
        {
            return width;
        }

        /// <summary>
        ///     Sets the width (X/Z) value.
        /// </summary>
        /// <param name="width">Width in meters.</param>
        public void SetWidth(double width)
        {
            this.width = width;
        }

        /// <summary>
        ///     Gets the height (Y) value.
        /// </summary>
        /// <returns>Height in meters.</returns>
        public double GetHeight()
        {
            return height;
        }

        /// <summary>
        ///     Sets the height (Y) value.
        /// </summary>
        /// <param name="height">Height in meters.</param>
        public void SetHeight(double height)
        {
            this.height = height;
        }

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        /// <returns>The object's display name.</returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        ///     Sets the display name.
        /// </summary>
        /// <param name="name">A display name for the object.</param>
        public void SetName(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///     Gets the world object type.
        /// </summary>
        /// <returns>The type of this world object.</returns>
        public WorldObjectType GetWorldObjType()
        {
            return type;
        }

        /// <summary>
        ///     e
        ///     Sets the world object type.
        /// </summary>
        /// <param name="type">A value from <see cref="WorldObjectType" />.</param>
        public void SetWorldObjType(WorldObjectType type)
        {
            this.type = type;
        }

        /// <summary>
        ///     Gets the associated Unity GameObject.
        /// </summary>
        /// <returns>The GameObject linked to this world object.</returns>
        public GameObject GetObj()
        {
            return obj;
        }

        /// <summary>
        ///     Sets the associated Unity GameObject.
        /// </summary>
        /// <param name="obj">The GameObject to associate with this world object.</param>
        public void SetObj(GameObject obj)
        {
            this.obj = obj;
        }

        /// <summary>
        ///     Gets the metadata dictionary for this object.
        /// </summary>
        /// <returns>A dictionary of string key/value metadata.</returns>
        public Dictionary<string, string> GetMetadata()
        {
            return metadata;
        }

        /// <summary>
        ///     Sets the metadata dictionary for this object.
        /// </summary>
        /// <param name="metadata">A dictionary of string key/value metadata.</param>
        public void SetMetadata(Dictionary<string, string> metadata)
        {
            this.metadata = metadata;
        }
    }
}