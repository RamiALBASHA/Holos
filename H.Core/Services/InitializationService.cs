﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.Animals.Dairy;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Animals;
using H.Core.Providers.Climate;
using H.Core.Providers.Energy;
using H.Core.Providers.Plants;
using H.Core.Providers.Temperature;

namespace H.Core.Services
{
    public class InitializationService : IInitializationService
    {
        #region Fields

        private readonly IIndoorTemperatureProvider _indoorTemperatureProvider;
        private readonly Table_21_Average_Milk_Production_Dairy_Cows_Provider _averageMilkProductionDairyCowsProvider;
        private readonly Table_6_Manure_Types_Default_Composition_Provider _defaultManureCompositionProvider;
        private readonly Table_44_Fraction_OrganicN_Mineralized_As_Tan_Provider _fractionOrganicNMineralizedAsTanProvider;
        private readonly Table_36_Livestock_Emission_Conversion_Factors_Provider _livestockEmissionConversionFactorsProvider;
        private readonly Table_50_Fuel_Energy_Estimates_Provider _fuelEnergyEstimatesProvider;
        private readonly Table_16_Livestock_Coefficients_BeefAndDairy_Cattle_Provider _beefAndDairyCattleProvider;
        private readonly Table_22_Livestock_Coefficients_Sheep_Provider _sheepProvider;
        private readonly Table_30_Default_Bedding_Material_Composition_Provider _beddingMaterialCompositionProvider;
        private readonly Table_35_Methane_Producing_Capacity_Default_Values_Provider _defaultMethaneProducingCapacityProvider;

        #endregion

        #region Constructors

        public InitializationService()
        {
            _indoorTemperatureProvider = new Table_63_Indoor_Temperature_Provider();
            _averageMilkProductionDairyCowsProvider = new Table_21_Average_Milk_Production_Dairy_Cows_Provider();
            _defaultManureCompositionProvider = new Table_6_Manure_Types_Default_Composition_Provider();
            _fractionOrganicNMineralizedAsTanProvider = new Table_44_Fraction_OrganicN_Mineralized_As_Tan_Provider();
            _livestockEmissionConversionFactorsProvider = new Table_36_Livestock_Emission_Conversion_Factors_Provider();;
            _fuelEnergyEstimatesProvider = new Table_50_Fuel_Energy_Estimates_Provider();
            _beefAndDairyCattleProvider = new Table_16_Livestock_Coefficients_BeefAndDairy_Cattle_Provider();
            _sheepProvider = new Table_22_Livestock_Coefficients_Sheep_Provider();
            _beddingMaterialCompositionProvider = new Table_30_Default_Bedding_Material_Composition_Provider();
            _defaultMethaneProducingCapacityProvider = new Table_35_Methane_Producing_Capacity_Default_Values_Provider();
        }

        #endregion

        #region Public Methods

        public void CheckInitialization(Farm farm)
        {
            if (farm == null)
            {
                return;
            }

            if (farm.DefaultSoilData == null)
            {
                return;
            }

            if (farm.ClimateData == null)
            {
                return;
            }

            var climateData = farm.ClimateData;

            var barnTemperature = climateData.BarnTemperatureData;
            if (barnTemperature == null || barnTemperature.IsInitialized == false)
            {
                this.InitializeBarnTemperature(farm);
            }
        }

        public void ReInitializeFarms(IEnumerable<Farm> farms)
        {
            foreach (var farm in farms)
            {
                // Table 6
                this.InitializeManureCompositionData(farm);

                // Table 21
                this.InitializeMilkProduction(farm);

                // Table 36
                this.InitializeMethaneProducingCapacity(farm);

                // Table 36
                this.InitializeDefaultEmissionFactors(farm);

                // Table 44
                this.InitializeManureMineralizationFractions(farm);

                // Table 50
                this.InitializeFuelEnergy(farm);

                // Table 63
                this.InitializeBarnTemperature(farm);
            }
        }

        public void InitializeMethaneProducingCapacity(Farm farm)
        {
            if (farm != null)
            {
                foreach (var animalComponent in farm.AnimalComponents)
                {
                    foreach (var animalGroup in animalComponent.Groups)
                    {
                        foreach (var managementPeriod in animalGroup.ManagementPeriods)
                        {
                            var capacity = _defaultMethaneProducingCapacityProvider.GetMethaneProducingCapacityOfManure(managementPeriod.AnimalType);

                            managementPeriod.ManureDetails.MethaneProducingCapacityOfManure = capacity;
                        }   
                    }
                }
            }
        }

        public void InitializeManureCompositionData(Farm farm)
        {
            var manureCompositionData = _defaultManureCompositionProvider.ManureCompositionData;

            farm.DefaultManureCompositionData.Clear();
            farm.DefaultManureCompositionData.AddRange(manureCompositionData);

            var animalComponents = farm.AnimalComponents;
            foreach (var animalComponent in animalComponents)
            {
                foreach (var animalGroup in animalComponent.Groups)
                {
                    foreach (var managementPeriod in animalGroup.ManagementPeriods)
                    {
                        var defaults = _defaultManureCompositionProvider.GetManureCompositionDataByType(animalGroup.GroupType, managementPeriod.ManureDetails.StateType);
                        this.InitializeManureCompositionData(managementPeriod, defaults);
                    }
                }
            }
        }

        public void InitializeManureMineralizationFractions(Farm farm)
        {
            var animalComponents = farm.AnimalComponents;
            foreach (var animalComponent in animalComponents)
            {
                foreach (var animalGroup in animalComponent.Groups)
                {
                    foreach (var managementPeriod in animalGroup.ManagementPeriods)
                    {
                        var fractions = _fractionOrganicNMineralizedAsTanProvider.GetByStorageType(managementPeriod.ManureDetails.StateType, managementPeriod.AnimalType);
                        this.InitializeManureMineralizationFractions(managementPeriod, fractions);
                    }
                }
            }
        }

        /// <summary>
        /// Reinitialize the manure <see cref="DefaultManureCompositionData"/> for the selected <see cref="ManagementPeriod"/>.
        /// </summary>
        /// <param name="managementPeriod">The <see cref="ManagementPeriod"/> that will have it's fractions reset to default values</param>
        /// <param name="manureCompositionData">The <see cref="DefaultManureCompositionData"/> containing the new default values to use</param>
        public void InitializeManureCompositionData(ManagementPeriod managementPeriod, DefaultManureCompositionData manureCompositionData)
        {
            if (managementPeriod != null && 
                managementPeriod.ManureDetails != null && 
                manureCompositionData != null)
            {
                managementPeriod.ManureDetails.FractionOfPhosphorusInManure = manureCompositionData.PhosphorusFraction;
                managementPeriod.ManureDetails.FractionOfCarbonInManure = manureCompositionData.CarbonFraction;
                managementPeriod.ManureDetails.FractionOfNitrogenInManure = manureCompositionData.NitrogenFraction;
            }
        }
        
        /// <summary>
        /// Reinitialize each <see cref="CropViewItem"/> within <see cref="Farm"/> with new default values
        /// </summary>
        /// <param name="farm">The <see cref="Farm"/> that will be reinitialized to new default values</param>
        public void InitializeFuelEnergy(Farm farm)
        {
            var viewItems = farm.GetCropDetailViewItems();
            foreach (var viewItem in viewItems)
            {
                InitializeFuelEnergy(farm, viewItem);
            }
        }

        /// <summary>
        /// Reinitialize the <see cref="CropViewItem"/> from the selected <see cref="Farm"/> with new default values
        /// </summary>
        /// <param name="farm">The <see cref="Farm"/> containing the relevant data to pass into <see cref="Table_50_Fuel_Energy_Estimates_Provider"/></param>
        /// <param name="viewItem">The <see cref="CropViewItem"/> that will have its values reset with new default values</param>
        public void InitializeFuelEnergy(Farm farm, CropViewItem viewItem)
        {
            var soilData = farm.GetPreferredSoilData(viewItem);
            var fuelEnergyEstimates = _fuelEnergyEstimatesProvider.GetFuelEnergyEstimatesDataInstance(
                province: soilData.Province,
                soilCategory: soilData.SoilFunctionalCategory,
                tillageType: viewItem.TillageType,
                cropType: viewItem.CropType);

            if (fuelEnergyEstimates != null)
            {
                viewItem.FuelEnergy = fuelEnergyEstimates.FuelEstimate;
            }
        }

        public void InitializeManureMineralizationFractions(ManagementPeriod managementPeriod, FractionOfOrganicNitrogenMineralizedData  mineralizationFractions)
        {
            if (managementPeriod != null &&
                managementPeriod.ManureDetails != null &&
                mineralizationFractions != null)
            {
                managementPeriod.ManureDetails.FractionOfOrganicNitrogenImmobilized = mineralizationFractions.FractionImmobilized;
                managementPeriod.ManureDetails.FractionOfOrganicNitrogenNitrified = mineralizationFractions.FractionNitrified;
                managementPeriod.ManureDetails.FractionOfOrganicNitrogenMineralized = mineralizationFractions.FractionMineralized;
            }
        }

        public void InitializeDefaultEmissionFactors(Farm farm)
        {
            if (farm != null)
            {
                foreach (var animalComponent in farm.AnimalComponents)
                {
                    foreach (var animalGroup in animalComponent.Groups)
                    {
                        foreach (var animalGroupManagementPeriod in animalGroup.ManagementPeriods)
                        {
                            this.InitializeDefaultEmissionFactors(farm, animalComponent, animalGroupManagementPeriod);
                        }
                    }
                }
            }
        }

        public void InitializeDefaultEmissionFactors(Farm farm, AnimalComponentBase animalComponent, ManagementPeriod managementPeriod)
        {
            if (farm != null &&
                animalComponent != null &&
                managementPeriod != null)
            {
                var emissionData = _livestockEmissionConversionFactorsProvider.GetFactors(manureStateType: managementPeriod.ManureDetails.StateType,
                    componentCategory: animalComponent.ComponentCategory,
                    meanAnnualPrecipitation: farm.ClimateData.PrecipitationData.GetTotalAnnualPrecipitation(),
                    meanAnnualTemperature: farm.ClimateData.TemperatureData.GetMeanAnnualTemperature(),
                    meanAnnualEvapotranspiration: farm.ClimateData.EvapotranspirationData.GetTotalAnnualEvapotranspiration(),
                    beddingRate: managementPeriod.HousingDetails.UserDefinedBeddingRate,
                    animalType: managementPeriod.AnimalType,
                    farm: farm,
                    year: managementPeriod.Start.Date.Year);

                managementPeriod.ManureDetails.MethaneConversionFactor = emissionData.MethaneConversionFactor;
                managementPeriod.ManureDetails.N2ODirectEmissionFactor = emissionData.N20DirectEmissionFactor;
                managementPeriod.ManureDetails.VolatilizationFraction = emissionData.VolatilizationFraction;
                managementPeriod.ManureDetails.EmissionFactorVolatilization = emissionData.EmissionFactorVolatilization;
                managementPeriod.ManureDetails.EmissionFactorLeaching = emissionData.EmissionFactorLeach;
            }
        }

        public void ReinitializeBeddingMaterial(Farm farm)
        {
            if (farm != null)
            {
                var data = _beddingMaterialCompositionProvider.Data;
                farm.DefaultsCompositionOfBeddingMaterials.Clear();
                farm.DefaultsCompositionOfBeddingMaterials.AddRange(data);

                foreach (var animalComponent in farm.AnimalComponents)
                {
                    foreach (var animalGroup in animalComponent.Groups)
                    {
                        foreach (var managementPeriod in animalGroup.ManagementPeriods)
                        {
                            var beddingMaterialComposition = farm.GetBeddingMaterialComposition(
                                beddingMaterialType: managementPeriod.HousingDetails.BeddingMaterialType,
                                animalType: managementPeriod.AnimalType);

                            this.InitializeBeddingMaterial(managementPeriod, beddingMaterialComposition);
                        }
                    }
                }
            }
        }

        public void InitializeBeddingMaterial(ManagementPeriod managementPeriod, Table_30_Default_Bedding_Material_Composition_Data data)
        {
            if (managementPeriod != null && managementPeriod.HousingDetails != null && data != null)
            {
                managementPeriod.HousingDetails.TotalCarbonKilogramsDryMatterForBedding = data.TotalCarbonKilogramsDryMatter;
                managementPeriod.HousingDetails.TotalNitrogenKilogramsDryMatterForBedding = data.TotalNitrogenKilogramsDryMatter;
                managementPeriod.HousingDetails.TotalPhosphorusKilogramsDryMatterForBedding = data.TotalPhosphorusKilogramsDryMatter;
                managementPeriod.HousingDetails.MoistureContentOfBeddingMaterial = data.MoistureContent;
            }
        }

        /// <summary>
        /// Reinitialize the MilkProduction value <see cref="MilkProduction"/> for each ManagementPeriod for each animalGroup in the DairyComponent of a <see cref="Farm"/> with new default values from table 21.
        /// </summary>
        /// <param name="farm">The <see cref="Farm"/> that will be reinitialized to new default value for the MilkProduction</param>
        public void InitializeMilkProduction(Farm farm)
        {
            List<Table_21_Average_Milk_Production_Dairy_Cows_Data> milkProductionDataList = _averageMilkProductionDairyCowsProvider.ReadFile();
            if (farm != null && farm.DairyComponents != null)
            {
                foreach (var dairyComponent in farm.DairyComponents.Cast<DairyComponent>())
                {
                    if (dairyComponent.Groups != null)
                    {
                        foreach (var animalGroup in dairyComponent.Groups)
                        {
                            if (animalGroup != null && animalGroup.GroupType == AnimalType.DairyLactatingCow)
                            {
                                foreach (var animalGroupManagementPeriod in animalGroup.ManagementPeriods)
                                {
                                    //Calling ReadFile() to get the milkProductionList, extract the province and year to pull value from table and reset to default.
                                    int year = animalGroupManagementPeriod.Start.Year;
                                    IEnumerable<double> milkProduction
                                        = from mp in milkProductionDataList
                                        where (mp.Province == farm.DefaultSoilData.Province && (int)mp.Year == year)
                                          select mp.AverageMilkProduction;
                                    if (milkProduction?.Any() != true)
                                    {
                                        throw new NullReferenceException();
                                    }
                                    else
                                    {
                                        animalGroupManagementPeriod.MilkProduction = milkProduction.First();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        #endregion

        #region Private Methods

        private void InitializeBarnTemperature(Farm farm)
        {
            if (farm != null && farm.ClimateData != null)
            {
                var climateData = farm.ClimateData;
                var province = farm.DefaultSoilData.Province;

                climateData.BarnTemperatureData = _indoorTemperatureProvider.GetIndoorTemperature(province);
                climateData.BarnTemperatureData.IsInitialized = true;
            }
        }

        #endregion
    }
}