using Microsoft.VisualStudio.TestTools.UnitTesting;
using GDriveFolderMerge;
using Google.Apis.Drive.v2.Data;
using Utils.GDrive;
using UnitTests.GDrive;
using Moq;

namespace UnitTests.GDriveFolderMerge
{
    [TestClass]
    public class GDriveFolderMerge
    {
        private const string file = "application/octet-stream";
        private const string folder = "application/vnd.google-apps.folder";

        private Mock<File> rootMock;
        private File root;
        private Mock<IGDrive> google;


        [TestInitialize]
        public void TestSetup()
        {
            google = new Mock<IGDrive>();
            rootMock = TestHelper.SetupMockFile(google, "Root", 
                folder);
            root = rootMock.Object;
            google.Setup(o => o.GetRootFolderId()).Returns(root.Id);
            google.Setup(o => o.PathToGoogleFile("\\")).Returns(root);
            Program.foldersMimeType = folder;         
        }

        

        [TestMethod]
        public void CleanUpTwoEmptyFolders()
        {
            Mock<File> folder1 = TestHelper.SetupMockFile(google, "1", folder);
            Mock<File> folder2 = TestHelper.SetupMockFile(google, "1", folder);
            TestHelper.SetChildren(google, rootMock, 
                new Mock<File>[] { folder1, folder2 });

            Program.CleanUpGDrive(google.Object, "\\");
            google.Verify(o => o.DeleteFile(root.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder1.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder2.Object.Id), Times.Once);
            google.Verify(o => o.SetParent(It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void MergeTwoFolders()
        {

        }
    }
}
