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
            google.Setup(o => o.GetFileInfo(mock.Object.Id))
                .Returns(mock.Object);
            return mock;
        }

        /* When google.NewFolder is called the mock is updated to seem like
        there is an empty child folder inside the parent folder. */
        private void SetupFolderCreationCallback(string name, File parent,
            File child, ChildReference childReference)
        {
            google.Setup(o => o.NewFolder(name, parent))
                .Callback(() => {
                    google.Setup(o => o.ChildList(parent))
                        .Returns(new ChildReference[] { childReference });
                    google.Setup(o => o.ChildList(child))
                        .Returns(new ChildReference[] { });
                });
        }

        [TestInitialize]
        /* Setup root, foo, and bar folders. */
        public void TestSetup()
        {
            google = new Mock<IGDrive>();

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
            google.Setup(o => o.GetRootFolderId()).Returns(root.Id);
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
        public void AbsolutePathCaching()
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
        /* Checks that parts of the path resolution are cached. */
        public void PartialPathCaching()
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

        [TestMethod]
        [ExpectedException(typeof(System.IO.FileNotFoundException))]
        /* Looking up a file that doesn't exist should throw an exception. */
        public void LookupNonExistent()
        {
            google.Setup(o => o.ChildList(root))
                .Returns(new ChildReference[] { fooChild.Object });
            google.Setup(o => o.ChildList(foo))
                .Returns(new ChildReference[] { barChild.Object });
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            var file = path.PathToGoogleFile(@"\bar");
        }

        [TestMethod]
        /* Creating a file that doesn't exist in root folder. */
        public void CreateInRoot()
        {
            google.Setup(o => o.ChildList(root))
                .Returns(new ChildReference[] {});
            google.Setup(o => o.NewFolder("foo", root));
            SetupFolderCreationCallback("foo", root, foo, fooChild.Object);
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            var file = path.CreateDirectoryTree(@"\foo");
            google.Verify(o => o.NewFolder("foo", root), Times.Once());
        }

        [TestMethod]
        /* Creating a file in a subfolder*/
        public void CreateInSubfolder()
        {
            google.Setup(o => o.ChildList(root))
                .Returns(new ChildReference[] { fooChild.Object });
            google.Setup(o => o.ChildList(foo))
                .Returns(new ChildReference[] {});
            google.Setup(o => o.NewFolder("baz", foo));
            SetupFolderCreationCallback("baz", foo, baz, bazChild.Object);
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            var file = path.CreateDirectoryTree(@"\foo\baz");
            google.Verify(o => o.NewFolder("baz", foo), Times.Once());
        }

        [TestMethod]
        /* Creating a directory tree. */
        public void CreateDirectoryTree()
        {
            var mewMock = SetupMockFile("mew");
            var mew = mewMock.Object;
            var mewChild = new Mock<ChildReference>();
            mewChild.Setup(o => o.Id).Returns(mew.Id);
            google.Setup(o => o.ChildList(root))
                .Returns(new ChildReference[] {});

            SetupFolderCreationCallback("foo", root, foo, fooChild.Object);
            SetupFolderCreationCallback("bar", foo, bar, barChild.Object);
            SetupFolderCreationCallback("baz", bar, baz, bazChild.Object);
            SetupFolderCreationCallback("mew", baz, mew, mewChild.Object);
            IGDrivePathHelper path = new GDrivePathHelper(google.Object);

            var file = path.CreateDirectoryTree(@"\foo\bar\baz\mew");
            google.Verify(o => o.NewFolder("foo", root), Times.Once());
            google.Verify(o => o.NewFolder("bar", foo), Times.Once());
            google.Verify(o => o.NewFolder("baz", bar), Times.Once());
            google.Verify(o => o.NewFolder("mew", baz), Times.Once());
            Assert.AreEqual(mewMock.Object.Title,
                path.PathToGoogleFile(@"\foo\bar\baz\mew").Title);
        }
    }
}
