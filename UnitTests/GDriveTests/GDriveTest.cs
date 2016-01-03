using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GDriveNURI;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;

namespace UnitTests.GDriveTests
{
    [TestClass]
    public class GDriveTest
    {
        //private Mock<IService> serviceMock;
        //private Mock<About> AboutExecute;
        //private IService service;

        [TestInitialize]
        public void TestSetup()
        {
            //serviceMock = new Mock<IService>();
            //service = serviceMock.Object;
        }

        /* Apparently I can't mock .Execute() method :/ */
        //[TestMethod]
        //public void NewFolderTest()
        //{
        //    File file = new File();
        //    var service = new Mock<IService>();
        //    var clientService = new Mock<IClientService>();
        //    var filesResource = new Mock<FilesResource>
        //        (new object[] { clientService.Object });
        //    var aboutResource = new Mock<AboutResource>
        //        (new object[] { clientService.Object });
        //    var aboutGetRequest = new Mock<AboutResource.GetRequest>
        //        (new object[] { clientService.Object });
        //    filesResource.Setup(o => o.Insert(It.IsAny<File>()))
        //        .Callback<File>(f => file = f)
        //        .Returns<File>(f => new FilesResource
        //            .InsertRequest(clientService.Object, f));
        //    service.Setup(o => o.Files).Returns(filesResource.Object);
        //    service.Setup(o => o.About).Returns(aboutResource.Object);
        //    //var insertRequest = new Mock<FilesResource.InsertRequest>();

        //    //insertRequest.Setup(o => o.Execute()).Callback<File>((f) => file = f);
        //    aboutResource.Setup(o => o.Get()).Returns(aboutGetRequest.Object);

        //    var parent = new Mock<File>();
        //    parent.Setup(o => o.Id).Returns("ParentID");

        //    var google = new GDrive(service.Object);
        //    google.NewFolder("MyFolder", parent.Object);
        //    //insertRequest.Verify(o => o.Execute(), Times.Once());
        //    Assert.AreEqual("MyFolder", file.Title);
        //    Assert.AreEqual("ParentID", file.Parents[0].Id);
        //}
    }
}
