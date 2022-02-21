﻿using H.Core.Enumerations;
using H.Core.Providers.Climate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace H.Core.Test.Providers.Climate
{
    [TestClass]
    public class GrowingDegreeCoefficientsProviderTest
    {
        private GrowingDegreeCoefficientsProvider_Table_1 _provider;

        [TestInitialize]
        public void TestInitialize()
        {
            _provider = new GrowingDegreeCoefficientsProvider_Table_1();
        }
        [TestMethod]
        public void GetGrowingDegreeCoefficientsCropNameTest()
        {
            var data = _provider.GetGrowingDegreeCoefficients();

            #region Asserts
            Assert.AreEqual(Enumerations.CropType.AlfalfaSeed, data[0].Crop);
            Assert.AreEqual(Enumerations.CropType.HayLegume, data[1].Crop);
            Assert.AreEqual(Enumerations.CropType.Dill, data[15].Crop);
            Assert.AreEqual(Enumerations.CropType.GreenFeed, data[33].Crop);
            Assert.AreEqual(Enumerations.CropType.FallRye, data[74].Crop);
            #endregion
        }

        [TestMethod]
        public void GetGrowingDegreeCoefficientsABCDETest()
        {

            var data = _provider.GetGrowingDegreeCoefficients();

            Assert.AreEqual(1.05E-01, data[0].A);
            Assert.AreEqual(3.82E-03, data[0].B);
            Assert.AreEqual(-6.77E-06, data[0].C);
            Assert.AreEqual(4.83E-09, data[0].D);
            Assert.AreEqual(-1.25E-12, data[0].E);
            Assert.AreEqual(1.01E+00, data[29].A);
            Assert.AreEqual(4.11E-03, data[29].B);
            Assert.AreEqual(0, data[29].C);
            Assert.AreEqual(0, data[29].D);
            Assert.AreEqual(0, data[29].E);
            Assert.AreEqual(2.37E-01, data[77].A);
            Assert.AreEqual(1.61E-03, data[77].B);
            Assert.AreEqual(-9.88E-07, data[77].C);
            Assert.AreEqual(3.77E-10, data[77].D);
            Assert.AreEqual(-9.49E-14, data[77].E);
        }
        [TestMethod]
        public void TestGetByCropType()
        {
            foreach (CropType i in Enum.GetValues(typeof(CropType)))
            {
                var data = _provider.GetByCropType(i);
                Assert.IsNotNull(data);
            }
        }
    }
}
