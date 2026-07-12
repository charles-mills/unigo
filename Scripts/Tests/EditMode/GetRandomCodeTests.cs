using APIs_Helpers;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class GetRandomCodeTests
    {
        [Test]
        public void LengthPropertyOperational()
        {
            var codeOne = GetRandomCode.GetRandom(8);
            var codeTwo = GetRandomCode.GetRandom(6);

            Assert.AreEqual(codeOne.Length, 8);
            Assert.AreEqual(codeTwo.Length, 6);
        }
    }
}