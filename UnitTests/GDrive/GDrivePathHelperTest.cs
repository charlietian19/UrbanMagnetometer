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
        private Mock<File> rootMock, fooMock, barMock, bazMock;
        private File root, bar, foo, baz;
        private Mock<ChildReference> fooChild, barChild, bazChild;
        private Mock<IGDrive> google;

        /* Returns a file mock given title. */
        private Mock<File> SetupMockFile(string title)
        {
            var mock = new Mock<File>();
            mock.Setup(o => o.Title).Returns(title);
            mock.Setup(o => o.Id).Returns(title + "ID");
            return mock;
        }

        [TestInitialize]
        /* Setup root, foo, and bar folders. */
        public void TestSetup()
        {
            rootMock = SetupMockFile("Root");
            root = rootMock.Object;
            fooMock = SetupMockFile("foo");
            foo = fooMock.Object;
            barMock = SetupMockFile("bar");
            bar = barMock.Object;
            bazMock = SetupMockFile("baz");
            baz = bazMock.Object;
            fooChild = new Mock<ChildReference>();
            barChild = new Mock<ChildReference>();
            bazChild = new Mock<ChildReference>();
            barChild.Setup(o => o.Id).Returns(bar.Id);
            fooChild.Setup(o => o.Id).Returns(foo.Id);
            bazChild.Setup(o => o.Id).Returns(baz.Id);

            google = new Mock<IGDrive>();
            google.Setup(o => o.GetRootFolderId()).Returns(root.Id);
            google.Setup(o => o.GetFileInfo(root.Id)).Returns(root);
            google.Setup(o => o.GetFileInfo(foo.Id)).Returns(foo);
            google.Setup(o => o.GetFileInfo(bar.Id)).Returns(bar);
            google.Setup(o => o.GetFileInfo(baz.Id)).Returns(baz);
        }

        [TestMethod]
        /* Checks that Lookup("\") returns the expected object. */
        public void LookupRoot()
        {
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);
            Assert.AreEqual(root.Title, path.PathToGoogleFile(@"\").Title);
        }

        [TestMethod]
        /* Checks that Lookup("\foo") and Lookup("\bar") return 
        the expected objects. */
        public void LookupFileInRootDirectory()
        {
            google.Setup(o => o.ChildList(root)).Returns(
                new ChildReference[] {barChild.Object, fooChild.Object});
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            Assert.AreEqual(foo.Title, path.PathToGoogleFile(@"\foo").Title);
            Assert.AreEqual(bar.Title, path.PathToGoogleFile(@"\bar").Title);
        }

        [TestMethod]
        /* Checks that Lookup("\foo\bar") returns the expected object. */
        public void LookupNested()
        {
            google.Setup(o => o.ChildList(root)).Returns(
                new ChildReference[] { fooChild.Object });
            google.Setup(o => o.ChildList(foo)).Returns(
                new ChildReference[] { barChild.Object });
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            Assert.AreEqual(bar.Title, 
                path.PathToGoogleFile(@"\foo\bar").Title);
        }

        [TestMethod]
        /* Checks that Lookup("\foo\bar\baz") returns the expected object. */
        public void LookupNestedTwice()
        {
            google.Setup(o => o.ChildList(root)).Returns(
                new ChildReference[] { fooChild.Object });
            google.Setup(o => o.ChildList(foo)).Returns(
                new ChildReference[] { barChild.Object });
            google.Setup(o => o.ChildList(bar)).Returns(
                new ChildReference[] { bazChild.Object });
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            Assert.AreEqual(baz.Title,
                path.PathToGoogleFile(@"\foo\bar\baz").Title);
        }
        
        [TestMethod]
        /* Checks that absolute path conversions are cached. */
        public void LookupAbsolutePathCaching()
        {
            google.Setup(o => o.ChildList(root))
                .Returns(new ChildReference[] { fooChild.Object });

            IGDrivePathHelper path = new GDrivePathHelper(google.Object);
            google.Verify(o => o.GetFileInfo(It.IsAny<string>()),
                Times.Exactly(1));
            google.Verify(o => o.ChildList(It.IsAny<File>()),
                Times.Exactly(0));

            var file = path.PathToGoogleFile(@"\foo");
            google.Verify(o => o.GetFileInfo(It.IsAny<string>()),
                Times.Exactly(2));
            google.Verify(o => o.ChildList(It.IsAny<File>()),
                Times.Exactly(2));

            file = path.PathToGoogleFile(@"\foo");
            google.Verify(o => o.GetFileInfo(It.IsAny<string>()),
                Times.Exactly(2));
            google.Verify(o => o.ChildList(It.IsAny<File>()),
                Times.Exactly(2));
        }

        [TestMethod]
        /* Checks that parts of the path are cached. */
        public void LookupPartialPathCaching()
        {
            google.Setup(o => o.ChildList(root))
                .Returns(new ChildReference[] { fooChild.Object });
            google.Setup(o => o.ChildList(foo))
                .Returns(new ChildReference[] { barChild.Object });

            IGDrivePathHelper path = new GDrivePathHelper(google.Object);
            google.Verify(o => o.GetFileInfo(It.IsAny<string>()),
                Times.Exactly(1));
            google.Verify(o => o.ChildList(It.IsAny<File>()),
                Times.Exactly(0));

            var file = path.PathToGoogleFile(@"\foo");
            google.Verify(o => o.GetFileInfo(It.IsAny<string>()),
                Times.Exactly(2));
            google.Verify(o => o.ChildList(It.IsAny<File>()),
                Times.Exactly(2));

            file = path.PathToGoogleFile(@"\foo\bar");
            google.Verify(o => o.GetFileInfo(It.IsAny<string>()),
                Times.Exactly(3));
            google.Verify(o => o.ChildList(It.IsAny<File>()),
                Times.Exactly(5));
        }
    }
}
