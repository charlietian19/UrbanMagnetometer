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

        /**
            Before:
            root\
                |-"1"\
                |     |-"2"\
                |     |     \-"3"\
                |     |           \-"7"\
                |     |                 \-"file1"
                |     \-"2"\
                |           \-"4"\
                |                 \-"file2"
                \-"1"\
                      |-"2"\
                      |     \-"5"\
                      |           \-"file3"
                      \-"2"\
                            \-"6"\
                                  \-"file4"

            After:
            root\
                 \-"1"\
                       \-"2"\
                             |-"3"\
                             |     \-"7"\
                             |           \-"file1"
                             |-"4"\
                             |     \-"file2"          
                             |-"5"\
                             |     \-"file3"
                             \-"6"\
                                   \-"file1"
        */
        [TestMethod]
        public void CleanUpSubtrees2()
        {
            Mock<File> folder11, folder12, folder21, folder22, folder23, 
                folder24, folder3, folder4, folder5, folder6, folder7;
            folder11 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder12 = TestHelper.SetupMockFile(google, "1", folderMime);
            folder21 = TestHelper.SetupMockFile(google, "2", folderMime);
            folder22 = TestHelper.SetupMockFile(google, "2", folderMime);
            folder23 = TestHelper.SetupMockFile(google, "2", folderMime);
            folder24 = TestHelper.SetupMockFile(google, "2", folderMime);
            folder3 = TestHelper.SetupMockFile(google, "3", folderMime);
            folder4 = TestHelper.SetupMockFile(google, "4", folderMime);
            folder5 = TestHelper.SetupMockFile(google, "5", folderMime);
            folder6 = TestHelper.SetupMockFile(google, "6", folderMime);
            folder7 = TestHelper.SetupMockFile(google, "7", folderMime);

            Mock<File> file1, file2, file3, file4;
            file1 = TestHelper.SetupMockFile(google, "file1", fileMime);
            file2 = TestHelper.SetupMockFile(google, "file2", fileMime);
            file3 = TestHelper.SetupMockFile(google, "file3", fileMime);
            file4 = TestHelper.SetupMockFile(google, "file4", fileMime);

            TestHelper.SetChildren(google, rootMock, new Mock<File>[] { folder11, folder12 });
            TestHelper.SetChildren(google, folder11, new Mock<File>[] { folder21, folder22 });
            TestHelper.SetChildren(google, folder12, new Mock<File>[] { folder23, folder24 });
            TestHelper.SetChildren(google, folder21, new Mock<File>[] { folder3 });
            TestHelper.SetChildren(google, folder22, new Mock<File>[] { folder4 });
            TestHelper.SetChildren(google, folder23, new Mock<File>[] { folder5 });
            TestHelper.SetChildren(google, folder24, new Mock<File>[] { folder6 });
            TestHelper.SetChildren(google, folder3, new Mock<File>[] { folder7 });
            TestHelper.SetChildren(google, folder4, new Mock<File>[] { file2 });
            TestHelper.SetChildren(google, folder5, new Mock<File>[] { file3 });
            TestHelper.SetChildren(google, folder6, new Mock<File>[] { file4 });
            TestHelper.SetChildren(google, folder7, new Mock<File>[] { file1 });

            Program.CleanUpGDrive(google.Object, "\\");
            google.Verify(o => o.DeleteFile(folder11.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder12.Object.Id), Times.Once);
            google.Verify(o => o.DeleteFile(folder21.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder22.Object.Id), Times.Once);
            google.Verify(o => o.DeleteFile(folder23.Object.Id), Times.Once);
            google.Verify(o => o.DeleteFile(folder24.Object.Id), Times.Once);
            google.Verify(o => o.DeleteFile(folder3.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder4.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder5.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder6.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(folder7.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(file1.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(file2.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(file3.Object.Id), Times.Never);
            google.Verify(o => o.DeleteFile(file4.Object.Id), Times.Never);

            google.Verify(o => o.SetParent(folder11.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(folder12.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(folder21.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(folder22.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(folder23.Object.Id, folder11.Object.Id),
                Times.Once);
            google.Verify(o => o.SetParent(folder24.Object.Id, folder11.Object.Id),
                Times.Once);
            google.Verify(o => o.SetParent(folder3.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(folder4.Object.Id, folder21.Object.Id),
                Times.Once);
            google.Verify(o => o.SetParent(folder5.Object.Id, folder21.Object.Id),
                Times.Once);
            google.Verify(o => o.SetParent(folder6.Object.Id, folder21.Object.Id),
                Times.Once);
            google.Verify(o => o.SetParent(folder7.Object.Id, It.IsAny<string>()),
                Times.Never);

            google.Verify(o => o.SetParent(file1.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file2.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file3.Object.Id, It.IsAny<string>()),
                Times.Never);
            google.Verify(o => o.SetParent(file4.Object.Id, It.IsAny<string>()),
                Times.Never);
        }
    }
}
