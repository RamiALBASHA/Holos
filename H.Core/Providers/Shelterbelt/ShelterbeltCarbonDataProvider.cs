﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using H.Content;
using H.Core.Calculators.Shelterbelt;
using H.Core.Enumerations;
using H.Infrastructure;

namespace H.Core.Providers.Shelterbelt
{
    public static class ShelterbeltCarbonDataProvider
    {
        public enum Columns
        {
            Dom_Mg_C_km,
            Dom_Mg_C_km_yr,
            Biom_Mg_C_km,
            Biom_Mg_C_km_yr,
            Tec_Mg_C_km,
            Tec_Mg_C_km_yr,
        }

        #region Fields

        /// <summary>
        /// Past data ranges from 1956 to 2015
        /// </summary>
        private static readonly List<ShelterbeltDomProviderData> _pastData;

        /// <summary>
        /// Future data ranges from 2016 to 2075
        /// </summary>
        private static readonly List<ShelterbeltDomProviderData> _futureData;

        #endregion

        #region Constructors

        static ShelterbeltCarbonDataProvider()
        {
            _pastData = ReadPastData();
            _futureData = ReadFutureData();
        }

        #endregion

        #region Public Methods

        public static List<ShelterbeltDomProviderData> GetData(int year)
        {
            if (year >= 2016)
            {
                return _futureData;
            }
            else
            {
                return _pastData;
            }
        }

        #endregion

        #region Private Methods

        private static List<ShelterbeltDomProviderData> ReadFutureData()
        {
            var result = new List<ShelterbeltDomProviderData>();

            result.AddRange(GetLines(TreeSpecies.Caragana, CsvResourceNames.CaraganaCarbonDataPast));
            result.AddRange(GetLines(TreeSpecies.GreenAsh, CsvResourceNames.GreenAshCarbonDataPast));
            result.AddRange(GetLines(TreeSpecies.HybridPoplar, CsvResourceNames.HybridPoplarCarbonDataPast));
            result.AddRange(GetLines(TreeSpecies.ManitobaMaple, CsvResourceNames.ManitobaMapleCarbonDataPast));
            result.AddRange(GetLines(TreeSpecies.ScotsPine, CsvResourceNames.ScotsPineCarbonDataPast));
            result.AddRange(GetLines(TreeSpecies.WhiteSpruce, CsvResourceNames.WhiteSpruceCarbonDataPast));

            return result;
        }

        private static List<ShelterbeltDomProviderData> ReadPastData()
        {
            var result = new List<ShelterbeltDomProviderData>();

            result.AddRange(GetLines(TreeSpecies.Caragana, CsvResourceNames.CaraganaCarbonDataFuture));
            result.AddRange(GetLines(TreeSpecies.GreenAsh, CsvResourceNames.GreenAshCarbonDataFuture));
            result.AddRange(GetLines(TreeSpecies.HybridPoplar, CsvResourceNames.HybridPoplarCarbonDataFuture));
            result.AddRange(GetLines(TreeSpecies.ManitobaMaple, CsvResourceNames.ManitobaMapleCarbonDataFuture));
            result.AddRange(GetLines(TreeSpecies.ScotsPine, CsvResourceNames.ScotsPineCarbonDataFuture));
            result.AddRange(GetLines(TreeSpecies.WhiteSpruce, CsvResourceNames.WhiteSpruceCarbonDataFuture));

            return result;
        }

        private static List<ShelterbeltDomProviderData> GetLines(
            TreeSpecies treeSpecies,
            CsvResourceNames resourceName)
        {
            var result = new List<ShelterbeltDomProviderData>();
            var filelines = CsvResourceReader.GetFileLines(resourceName);
            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;

            foreach (var line in filelines.Skip(1))
            {
                var entry = new ShelterbeltDomProviderData();
                entry.Species = treeSpecies;
                entry.ClusterId = line[3];
                entry.PercentageMortality = double.Parse(line[4], cultureInfo);
                entry.Age = int.Parse(line[5], cultureInfo);

                if (double.TryParse(line[6], out var tecPerKilometerPerYear))
                {
                    entry.TecPerKilometerPerYear = tecPerKilometerPerYear;
                }

                if (double.TryParse(line[7], out var tecPerKilometer))
                {
                    entry.TecPerKilometer = tecPerKilometer;
                }

                if (double.TryParse(line[8], out var biomassCarbonPerKilometerPerYear))
                {
                    entry.BiomassCarbonPerKilometerPerYear = biomassCarbonPerKilometerPerYear;
                }

                if (double.TryParse(line[9], out var biomassCarbonPerKilometer))
                {
                    entry.BiomassCarbonPerKilometer = biomassCarbonPerKilometer;
                }

                if (double.TryParse(line[10], out var deadOrganicMatterCarbonPerKilometerPerYear))
                {
                    entry.DeadOrganicMatterCarbonPerKilometerPerYear = deadOrganicMatterCarbonPerKilometerPerYear;
                }

                if (double.TryParse(line[11], out var deadOrganicMatterCarbonPerKilometer))
                {
                    entry.DeadOrganicMatterCarbonPerKilometer = deadOrganicMatterCarbonPerKilometer;
                }

                result.Add(entry);
            }

            return result;
        }

        public static double GetInterpolatedValue(
            TreeSpecies treeSpecies, 
            HardinessZone hardinessZone, 
            int ecodistrictId, 
            double percentMortality, 
            int mortalityLow, 
            int mortalityHigh, 
            int age, 
            Columns column, 
            int year)
        {
            if (age > 60)
            {
                age = 60;
            }

            // Data in table is indexed by cluster. Get the cluster id from the ecodistrict now
            var clusterData = ShelterbeltEcodistrictToClusterLookupProvider.GetClusterData(ecodistrictId);

            var data = GetData(year);

            var tableLookupLow = data.SingleOrDefault(
                x => x.Species == treeSpecies &&
                     x.ClusterId == clusterData.ClusterId &&
                     Math.Abs(x.PercentageMortality - mortalityLow) < double.Epsilon &&
                     x.Age == age);

            var tableLookupHigh = data.SingleOrDefault(
                x => x.Species == treeSpecies &&
                     x.ClusterId == clusterData.ClusterId &&
                     Math.Abs(x.PercentageMortality - mortalityHigh) < double.Epsilon &&
                     x.Age == age);

            if (tableLookupHigh != null && tableLookupLow != null)
            {
                var low = mortalityLow;
                var high = mortalityHigh;

                var targetLow = 0d;
                var targetHigh = 0d;

                if (column == Columns.Dom_Mg_C_km)
                {
                    targetLow = tableLookupLow.DeadOrganicMatterCarbonPerKilometer;
                    targetHigh = tableLookupHigh.DeadOrganicMatterCarbonPerKilometer;
                }
                else if (column == Columns.Dom_Mg_C_km_yr)
                {
                    targetLow = tableLookupLow.DeadOrganicMatterCarbonPerKilometerPerYear;
                    targetHigh = tableLookupHigh.DeadOrganicMatterCarbonPerKilometerPerYear;
                }
                else if (column == Columns.Biom_Mg_C_km)
                {
                    targetLow = tableLookupLow.BiomassCarbonPerKilometer;
                    targetHigh = tableLookupHigh.BiomassCarbonPerKilometer;
                }
                else if (column == Columns.Biom_Mg_C_km_yr)
                {
                    targetLow = tableLookupLow.BiomassCarbonPerKilometerPerYear;
                    targetHigh = tableLookupHigh.BiomassCarbonPerKilometerPerYear;
                }
                else if (column == Columns.Tec_Mg_C_km)
                {
                    targetLow = tableLookupLow.TecPerKilometer;
                    targetHigh = tableLookupHigh.TecPerKilometer;
                }
                else if (column == Columns.Tec_Mg_C_km_yr)
                {
                    targetLow = tableLookupLow.TecPerKilometerPerYear;
                    targetHigh = tableLookupHigh.TecPerKilometerPerYear;
                }

                var ratio = (targetLow - targetHigh) / (high - low);
                var product = (percentMortality - mortalityLow) * ratio;
                var result = targetLow - product;

                return result;
            }
            else
            {
                Trace.TraceError((nameof(ShelterbeltCarbonDataProvider) + " cannot find value in lookup table."));

                return 0;
            }
        }

        #endregion
    }
}