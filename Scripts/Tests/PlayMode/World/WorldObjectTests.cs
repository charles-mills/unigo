using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using World;

namespace Tests.PlayMode.World
{
    public class WorldObjectTests
    {
        private readonly double alt = 14.03;
        private readonly string expectedName = "Test1";
        private readonly double height = 18.05;
        private readonly double lat = 10.01;
        private readonly double lon = 12.02;
        private readonly double width = 16.04;
        private GameObject dummyObject;

        private WorldObject worldObject;


        [SetUp]
        public void Setup()
        {
            dummyObject = new GameObject("dummy");

            worldObject = new WorldObject();
        }

        [TearDown]
        public void Teardown()
        {
            if (dummyObject != null) Object.DestroyImmediate(dummyObject);
        }

        [Test]
        public void WorldObjectConstructorTest()
        {
            var type = WorldObject.WorldObjectType.POI;

            var worldObj = new WorldObject(expectedName, type, dummyObject, lon, lat, alt, width, height);

            Assert.AreEqual(expectedName, worldObj.GetName());
            Assert.AreEqual(type, worldObj.GetWorldObjType());
            Assert.AreSame(dummyObject, worldObj.GetObj());
            Assert.AreEqual(lon, worldObj.GetLongitude());
            Assert.AreEqual(lat, worldObj.GetLatitude());
            Assert.AreEqual(alt, worldObj.GetAltitude());
            Assert.AreEqual(width, worldObj.GetWidth());
            Assert.AreEqual(height, worldObj.GetHeight());
            Assert.IsNotNull(worldObj.GetMetadata(), "Metadata should be created through the constructor");

            Object.DestroyImmediate(dummyObject);
        }


        [Test]
        public void LongitudeSetterAndGetter()
        {
            worldObject.SetLongitude(lat);

            Assert.AreEqual(lat, worldObject.GetLongitude());
        }


        [Test]
        public void LatitudeSetterAndGetter()
        {
            worldObject.SetLatitude(lon);

            Assert.AreEqual(lon, worldObject.GetLatitude());
        }


        [Test]
        public void AltitudeSetterAndGetter()
        {
            worldObject.SetAltitude(alt);

            Assert.AreEqual(alt, worldObject.GetAltitude());
        }


        [Test]
        public void WidthSetterAndGetter()
        {
            worldObject.SetWidth(width);

            Assert.AreEqual(width, worldObject.GetWidth());
        }


        [Test]
        public void HeightSetterAndGetter()
        {
            worldObject.SetHeight(height);

            Assert.AreEqual(height, worldObject.GetHeight());
        }


        [Test]
        public void NameSetterAndGetter()
        {
            worldObject.SetName("Test2");

            Assert.AreEqual("Test2", worldObject.GetName());
        }


        [Test]
        public void MetaDataSetter()
        {
            var newMetadata = new Dictionary<string, string>();
            newMetadata.Add("Score", "100");
            newMetadata.Add("Status", "Active");

            worldObject.SetMetadata(newMetadata);
            var result = worldObject.GetMetadata();

            Assert.IsNotNull(result);
            Assert.AreEqual(newMetadata, result);
            Assert.AreEqual("100", result["Score"]);
        }
    }
}