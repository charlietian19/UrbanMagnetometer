using Google.Apis.Drive.v2.Data;
using Utils.GDrive;
using Moq;

namespace UnitTests.GDrive
{
    public class TestHelper
    {
        /* Returns a file mock given title. */
        public static Mock<File> SetupMockFile(Mock<IGDrive> google, string title)
        {
            var mock = new Mock<File>();
            mock.Setup(o => o.Title).Returns(title);
            mock.Setup(o => o.Id).Returns(title + mock.GetHashCode());
            google.Setup(o => o.GetFileInfo(mock.Object.Id))
                .Returns(mock.Object);
            return mock;
        }

        /* Returns a mock given a title and mime type */
        public static Mock<File> SetupMockFile(Mock<IGDrive> google,
            string title, string mime)
        {
            var mock = SetupMockFile(google, title);
            mock.Setup(o => o.MimeType).Returns(mime);
            return mock;
        }

        /* Returs the child reference for a given file. */
        public static Mock<ChildReference> GetChild(Mock<IGDrive> google,
            Mock<File> file)
        {
            var child = new Mock<ChildReference>();
            child.Setup(o => o.Id).Returns(file.Object.Id);
            return child;
        }

        /* Nest children into parent. (Overrides the children list) */
        public static void SetChildren(Mock<IGDrive> google,
            Mock<File> parent, Mock<ChildReference>[] children)
        {
            var childrenTotal = children.Length;
            var childList = new ChildReference[childrenTotal];
            for (int i = 0; i < childrenTotal; i++)
            {
                childList[i] = children[i].Object;
            }
            google.Setup(o => o.ChildList(parent.Object)).Returns(childList);
        }
    }
}
