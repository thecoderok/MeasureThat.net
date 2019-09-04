using MeasureThat.Net.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeasureThat.Net.Tests
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void TestHomePage()
        {
            // Arrange
            var controller = new HomeController(null, null);
            var result = controller.Index();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
    }
}
