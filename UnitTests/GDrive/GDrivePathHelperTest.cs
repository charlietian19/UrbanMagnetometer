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
        private Mock<File> rootMock;
        private File root;

        /* Returns a file mock given title. */
        private Mock<File> SetupMockFile(string title)
        {
            var mock = new Mock<File>();
            mock.Setup(o => o.Title).Returns(title);
            mock.Setup(o => o.Id).Returns(title + "ID");
            return mock;
        }

        [TestInitialize]
        public void TestSetup()
        {
            rootMock = SetupMockFile("Root");
            root = rootMock.Object;
        }

        [TestMethod]
        public void LookupPathRoot()
        {
            var google = new Mock<IGDrive>();
            google.Setup(o => o.GetRootFolderId()).Returns(root.Id);
            google.Setup(o => o.GetFileInfo(root.Id)).Returns(root);
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);
            Assert.AreEqual(root.Title, path.PathToGoogleFile(@"\").Title);
        }

        [TestMethod]
        public void LookupFileInRootDirectory()
        {
            var fooMock = SetupMockFile("foo");
            var foo = fooMock.Object;
            var barMock = SetupMockFile("bar");
            var bar = barMock.Object;
            var google = new Mock<IGDrive>();
            var fooChild = new Mock<ChildReference>();
            var barChild = new Mock<ChildReference>();
            barChild.Setup(o => o.Id).Returns(bar.Id);
            fooChild.Setup(o => o.Id).Returns(foo.Id);
            google.Setup(o => o.GetRootFolderId()).Returns(root.Id);
            google.Setup(o => o.GetFileInfo(root.Id)).Returns(root);
            google.Setup(o => o.GetFileInfo(foo.Id)).Returns(foo);
            google.Setup(o => o.GetFileInfo(bar.Id)).Returns(bar);
            google.Setup(o => o.ChildList(root)).Returns(
                new ChildReference[] {barChild.Object, fooChild.Object});
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            Assert.AreEqual(foo.Title, path.PathToGoogleFile(@"\foo").Title);
            Assert.AreEqual(bar.Title, path.PathToGoogleFile(@"\bar").Title);
        }
    }
}
