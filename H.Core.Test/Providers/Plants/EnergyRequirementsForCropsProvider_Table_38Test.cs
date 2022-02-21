﻿using H.Core.Enumerations;
using H.Core.Providers.Plants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Core.Test.Providers.Plants
{
    [TestClass]
    public class EnergyRequirementsForCropsProvider_Table_38Test
    {
        [TestMethod]
        public void GetEnergyDataForCropInWesternCanadaTest()
        {
            var provider = new EnergyRequirementsForCropsProvider_Table_38();

            var result = provider.GetEnergyData(Province.Alberta, SoilFunctionalCategory.Black, TillageType.Intensive, CropType.Barley);

            Assert.AreEqual(2.63, result.EnergyForFuel);
            Assert.AreEqual(0.16, result.EnergyForHerbicide);
        }

        [TestMethod]
        public void GetEnergyDataForFallowInWesternCanadaTest()
        {
            var provider = new EnergyRequirementsForCropsProvider_Table_38();

            var result = provider.GetEnergyData(Province.Alberta, SoilFunctionalCategory.BlackGrayChernozem, TillageType.Reduced, CropType.Fallow);

            Assert.AreEqual(1.71, result.EnergyForFuel);
            Assert.AreEqual(0.11, result.EnergyForHerbicide);
        }

        [TestMethod]
        public void GetEnergyDataForCropInEasternCanadaTest()
        {
            var provider = new EnergyRequirementsForCropsProvider_Table_38();

            var result = provider.GetEnergyData(Province.NewBrunswick, SoilFunctionalCategory.EasternCanada, TillageType.Intensive, CropType.HayGrass);

            Assert.AreEqual(0.81, result.EnergyForFuel);
            Assert.AreEqual(0, result.EnergyForHerbicide);
        }

        [TestMethod]
        public void GetEnergyDataForHayGrassInWesternCanadaTest()
        {
            var provider = new EnergyRequirementsForCropsProvider_Table_38();

            var result = provider.GetEnergyData(Province.Alberta, SoilFunctionalCategory.Black, TillageType.Intensive, CropType.HayGrass);

            Assert.AreEqual(2.63, result.EnergyForFuel);
            Assert.AreEqual(0.16, result.EnergyForHerbicide);
        }

        [TestMethod]
        public void GetEnergyDataTest()
        {
            var provider = new EnergyRequirementsForCropsProvider_Table_38();

            var smallGrainCerealsAlbertaBlackIntensive = provider.GetEnergyData(Province.Alberta, SoilFunctionalCategory.Black, TillageType.Intensive, CropType.SmallGrainCereals);
            Assert.AreEqual(2.63, smallGrainCerealsAlbertaBlackIntensive.EnergyForFuel);
            Assert.AreEqual(0.16, smallGrainCerealsAlbertaBlackIntensive.EnergyForHerbicide);

            var smallGrainCerealsAlbertaBlackReduced = provider.GetEnergyData(Province.Alberta, SoilFunctionalCategory.Black, TillageType.Reduced, CropType.SmallGrainCereals);
            Assert.AreEqual(2.39, smallGrainCerealsAlbertaBlackReduced.EnergyForFuel);
            Assert.AreEqual(0.23, smallGrainCerealsAlbertaBlackReduced.EnergyForHerbicide);

            var smallGrainCerealsAlbertaBlackNoTill = provider.GetEnergyData(Province.Alberta, SoilFunctionalCategory.Black, TillageType.NoTill, CropType.SmallGrainCereals);
            Assert.AreEqual(1.43, smallGrainCerealsAlbertaBlackNoTill.EnergyForFuel);
            Assert.AreEqual(0.46, smallGrainCerealsAlbertaBlackNoTill.EnergyForHerbicide);
        }
    }
}
