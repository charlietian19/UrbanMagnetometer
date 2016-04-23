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
        private const string fileMime = "application/octet-stream";
        private const string folderMime = "application/vnd.google-apps.folder";

        private Mock<File> rootMock;
        private File root;
        private Mock<IGDrive> google;


        [TestInitialize]
        public void TestSetup()
        {
            google = new Mock<IGDrive>();
            rootMock = TestHelper.SetupMockFile(google, "Root", 
                folderMime);
            root = rootMock.Object;
            google.Setup(o => o.GetRootFolderId()).Returns(root.Id);
            google.Setup(o => o.PathToGoogleFile("\\")).Returns(root);
            Program.foldersMimeType = folderMime;         
        }
        
        /**
            Before:
            root\
                |-"1"
                \-"1"

            After:
            root\
                 \-"1"
        */
        [TestMethod]
        public void MergeTwoFolders()
        {
            Mock<File> folder1, folder2;
            folder1 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder2 = TestHelper.SetupMockFile(google, "1", folderMime);
            TestHelper.SetChildren(google, rootMock, new Mock<File>[] { folder1, folder2 });

            Mock<File> file1, file2, file3, file4;
            file1 = TestHelper.SetupMockFile(google, "file1", fileMime);
            file2 = TestHelper.SetupMockFile(google, "file2", fileMime);
            TestHelper.SetChildren(google, folder1, new Mock<File>[] { file1, file2 });

            file3 = TestHelper.SetupMockFile(google, "file3", fileMime);
            file4 = TestHelper.SetupMockFile(google, "file4", fileMime);
            TestHelper.SetChildren(google, folder2, new Mock<File>[] { file3, file4 });

            Program.MergeTrees(google.Object, folder1.Object, folder2.Object);
            google.Verify(o => o.SetParent(folder1.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(folder2.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file1.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file2.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file3.Object.Id, folder1.Object.Id),
                Times.Once);
            google.Verify(o => o.SetParent(file4.Object.Id, folder1.Object.Id),
                Times.Once);
            google.Verify(o => o.DeleteFile(folder2.Object.Id), Times.Once);
        }

        /**
            Before:
            "1"\
                |-"file1"
                \-"file2"

            "1"\
                |-"file3"
                \-"file4"

            After:
            "1"\
                |-"file1"
                |-"file2"
                |-"file3"
                \-"file4"
        */
        [TestMethod]
        public void CleanUpTwoEmptyFolders()
        {
            Mock<File> folder1, folder2;
            folder1 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder2 = TestHelper.SetupMockFile(google, "1", folderMime);
            TestHelper.SetChildren(google, rootMock, new Mock<File>[] { folder1, folder2 });

            Program.CleanUpGDrive(google.Object, "\\");
            google.Verify(o => o.DeleteFile(root.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder1.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder2.Object.Id), Times.Once);
            google.Verify(o => o.SetParent(It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        /**
            Before:
            root\
                |-"1"\
                |     \-"file1"
                \-"1"\
                      \-"file2"

            After:
            root\
                 \-"1"\
                      |-"file1"
                      \-"file2"
        */
        [TestMethod]
        public void CleanUpFoldersWithFilesOnly()
        {
            Mock<File> folder1, folder2;
            folder1 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder2 = TestHelper.SetupMockFile(google, "1", folderMime);

            Mock<File> file1, file2;
            file1 = TestHelper.SetupMockFile(google, "file1", fileMime);
            TestHelper.SetChildren(google, folder1, new Mock<File>[] { file1 });
            file2 = TestHelper.SetupMockFile(google, "file2", fileMime);
            TestHelper.SetChildren(google, folder2, new Mock<File>[] { file2 });
            TestHelper.SetChildren(google, rootMock, new Mock<File>[] { folder1, folder2 });

            Program.CleanUpGDrive(google.Object, "\\");
            google.Verify(o => o.DeleteFile(folder2.Object.Id), Times.Once);
            google.Verify(o => o.SetParent(file2.Object.Id, folder1.Object.Id), 
                Times.Once);
            google.Verify(o => o.DeleteFile(folder1.Object.Id), Times.Never);
            google.Verify(o => o.SetParent(file1.Object.Id, It.IsAny<string>()),
                Times.Never);
        }

        /** 
            The program should to nothing with files nested in files.
        */
        [TestMethod]
        public void CleanUpFileNestedInFile()
        {
            Mock<File> folder1, folder2;
            folder1 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder2 = TestHelper.SetupMockFile(google, "1", fileMime);

            Mock<File> file1, file2;
            file1 = TestHelper.SetupMockFile(google, "file1", fileMime);
            TestHelper.SetChildren(google, folder1, new Mock<File>[] { file1 });
            file2 = TestHelper.SetupMockFile(google, "file2", fileMime);
            TestHelper.SetChildren(google, folder2, new Mock<File>[] { file2 });
            TestHelper.SetChildren(google, rootMock,
                new Mock<File>[] { folder1, folder2 });

            Program.CleanUpGDrive(google.Object, "\\");
            google.Verify(o => o.DeleteFile(folder2.Object.Id), Times.Never);
            google.Verify(o => o.SetParent(file2.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.DeleteFile(folder1.Object.Id), Times.Never);
            google.Verify(o => o.SetParent(file1.Object.Id, It.IsAny<string>()),
                Times.Never);
        }

        /**
            Before:
            root\
                |-"1"\
                |     |-"2"\
                |     |     |-"file1"
                |     |     \-"file2"
                |     \-"3"
                \-"1"\
                      \-"2"\
                            \-"file3"

            After:
            root\
                 \-"1"\
                      |-"2"\
                      |     |-"file1"
                      |     |-"file2"
                      |     \-"file3"
                      \-"3"
        */
        [TestMethod]
        public void CleanUpSubtrees()
        {
            Mock<File> folder11, folder12, folder21, folder22, folder3;
            folder11 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder12 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder21 = TestHelper.SetupMockFile(google, "2", folderMime);
            folder22 = TestHelper.SetupMockFile(google, "2", folderMime);
            folder3 = TestHelper.SetupMockFile(google, "3", folderMime);

            Mock<File> file1, file2, file3;
            file1 = TestHelper.SetupMockFile(google, "file1", fileMime);
            file2 = TestHelper.SetupMockFile(google, "file2", fileMime);
            file3 = TestHelper.SetupMockFile(google, "file3", fileMime);

            TestHelper.SetChildren(google, folder21, new Mock<File>[] { file1, file2 });
            TestHelper.SetChildren(google, folder22, new Mock<File>[] { file3 });
            TestHelper.SetChildren(google, folder11, new Mock<File>[] { folder21, folder3 });
            TestHelper.SetChildren(google, folder12, new Mock<File>[] { folder22 });
            TestHelper.SetChildren(google, rootMock, new Mock<File>[] { folder11, folder12 });

            Program.CleanUpGDrive(google.Object, "\\");
            google.Verify(o => o.DeleteFile(folder11.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder12.Object.Id), Times.Once);
            google.Verify(o => o.DeleteFile(folder21.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder22.Object.Id), Times.Once);
            google.Verify(o => o.DeleteFile(folder3.Object.Id), Times.Never);
            google.Verify(o => o.SetParent(file1.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file2.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file3.Object.Id, folder21.Object.Id),
                Times.Once);
        }
    }
}
