using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.Apis.Drive.v2.Data;
using GDriveNURI;
using Moq;

namespace UnitTests
{
    [TestClass]
    public class GDrivePathHelperTest
    {
        [TestMethod]
        public void LookupPathRoot()
        {
            var google = new Mock<IGDrive>();
            google.Setup(o => o.GetRootFolderId()).Returns("rootID");
            google.Setup(o => o.GetFileInfo("rootID"))
                .Returns(new File() { Title = "Root", Id = "rootID" });
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);
            Assert.AreEqual("Root", path.PathToGoogleFile(@"\").Title);
            Assert.AreEqual("rootID", path.PathToGoogleFile(@"\").Id);
        }
    }
}
