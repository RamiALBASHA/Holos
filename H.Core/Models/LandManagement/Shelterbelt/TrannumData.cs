﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Navigation;
using H.Core.Calculators.Shelterbelt;
using H.Core.Enumerations;
using H.Infrastructure;

namespace H.Core.Models.LandManagement.Shelterbelt
{
    /// <summary>
    /// This class exists for saving information relevant to the shelterbelt details screen.
    /// </summary>
    public class TrannumData : ModelBase
    {
        #region Event Handlers

        private void OnCircumferenceDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.CircumferenceData));
        }

        #endregion

        #region Fields

        private readonly ShelterbeltCalculator _shelterbeltCalculator = new ShelterbeltCalculator();

        private double _rowLength;
        private double _treeSpacing;
        private double _treeCount;
        private TreeSpecies _treeSpecies;
        private CircumferenceData _circumferenceData;
        private double _year;
        private double _aboveGroundCarbonKgPerTree;
        private double _belowGroundCarbonKgPerTree;
        private string _shelterbeltName;
        private string _rowName;
        private string _treeGroupName;
        private Guid _shelterbeltGuid;
        private Guid _rowGuid;
        private int _age;

        private double _percentMortality;
        private double _percentMortalityHigh;
        private double _percentMortalityLow;

        private double _totalBiomassPerTree;
        private double _totalLivingCarbonPerTreeType;

        private double _biomassPerTreeType;
        private double _totalCarbonForAllTreesOfSameSpecies;

        private double _totalLivingBiomassPerTreeType;
        private double _totalCarbonForAllTreesOfSameSpeciesPerStandardPlanting;

        private double _totalCarbonForShelterbeltAtYear;
        private double _totalCarbonForShelterbeltAtYearPerStandardPlanting;
        private double _totalCarbonForShelterbeltSequesteredAtYear;
        private double _totalCarbonDioxideForShelterbeltSequesteredAtYearPerStandardPlanting;

        private Guid _treeGroupGuid;

        #endregion

        #region Constructors

            /// <summary>
            /// This is NOT the way to construct TrannumDatas. Pass in a shelterbeltcomponent, rowData and treegroupdata, and year instead.
            /// </summary>
        public TrannumData()
        {
            this.RowLength = 100;
            this.TreeCount = 1;
            this.TreeSpecies = TreeSpecies.Caragana;
            this.CircumferenceData = new CircumferenceData();
            this.Year = DateTime.Now.Year;
            this.AboveGroundCarbonStocksPerTree = 0.0; //need to decide when this gets generated
            this.BelowGroundCarbonKgPerTree = 0.0; //need to decide when this gets generated
            this.ShelterbeltName = "Shelterbelt";
            this.RowName = "Row";
            this.TreeGroupName = "Treetype";
            //There are no Guids to connect, but could end up needing to be able to check that later?
            this.ShelterbeltGuid = Guid.NewGuid();
            this.RowGuid = Guid.NewGuid();
            this.TreeGroupGuid = Guid.NewGuid();
            this.SharedConstruction();
        }

        /// <summary>
        /// Generate a Trannum with all of its properties filled in from a corresponding set of components, along with the year
        /// of the trannum.
        /// </summary>
        /// <param name="shelterbeltComponent">The shelterbelt this trannum represents.</param>
        /// <param name="row">The row this trannum represents.</param>
        /// <param name="treeGroup">The treegroup this trannum represents.</param>
        /// <param name="year">The year this trannum represents.</param>
        public TrannumData(ShelterbeltComponent shelterbeltComponent, RowData row, TreeGroupData treeGroup, double year)
        {
            if (shelterbeltComponent == null || row == null || treeGroup == null)
            {
                throw new Exception(nameof(TrannumData) + "'s constructor cannot be passed null values.");
            }

            if (year < treeGroup.PlantYear || year > treeGroup.CutYear)
            {
                throw new Exception(nameof(TrannumData) +
                                    " the year passed to the constructor was outside the lifespan of the treegroup.");
            }

            this.TreeGroupData = treeGroup;

            //Simple copying
            this.YearOfObservation = shelterbeltComponent.YearOfObservation;            
            this.TreeSpecies = treeGroup.TreeSpecies;
            this.CircumferenceData = new CircumferenceData(treeGroup.CircumferenceData); //Cannot be a reference to the same copy.
            this.Year = year;
            if (shelterbeltComponent.NameIsFromUser)
            {
                this.ShelterbeltName = shelterbeltComponent.Name;
            }
            else
            {
                this.ShelterbeltName = shelterbeltComponent.GetCorrectName();
            }
            if (row.NameIsFromUser)
            {
                this.RowName = row.Name;
            }
            else
            {
                this.RowName = row.GetCorrectName();
            }

            if (treeGroup.NameIsFromUser)
            {
                this.TreeGroupName = treeGroup.Name;
            }
            else
            {
                this.TreeGroupName = treeGroup.GetCorrectName();
            }
            this.ShelterbeltGuid = shelterbeltComponent.Guid;
            this.RowGuid = row.Guid;
            this.TreeGroupGuid = treeGroup.Guid;

            // Assume the tree died in its first year
            if (treeGroup.PlantYear == year) 
            {
                this.TreeCount = treeGroup.PlantedTreeCount;
            }
            else
            {
                this.TreeCount = treeGroup.LiveTreeCount;
            }

            List<double> livetrees = new List<double>();
            List<double> plantedtrees = new List<double>();
            foreach (var treegroup in row.TreeGroupData)
            {
                livetrees.Add(treegroup.LiveTreeCount);
                plantedtrees.Add(treegroup.PlantedTreeCount);
            }

            this.PercentMortality = _shelterbeltCalculator.CalculatePercentMortalityOfALinearPlanting(plantedtrees, livetrees);

            int mortalityLow = _shelterbeltCalculator.CalculateMortalityLow(PercentMortality);
            int mortalityHigh = _shelterbeltCalculator.CalculateMortalityHigh(mortalityLow);

            this.RowLength = row.Length;
            this.TreeSpacing = _shelterbeltCalculator.CalculateTreeSpacing(this.RowLength, this.TreeCount);
            this.TreeCount = _shelterbeltCalculator.CalculateTreeCount(this.RowLength, this.TreeSpacing, this.PercentMortality);

            this.PercentMortalityLow = mortalityLow;
            this.PercentMortalityHigh = mortalityHigh;
            this.HardinessZone = shelterbeltComponent.HardinessZone;
            this.EcodistrictId = shelterbeltComponent.EcoDistrictId;

            // Define a default circumference for all years except the year of observation since the user will have defined a value for that year

            var ageOfTree = (int)(this.Year - treeGroup.PlantYear) + 1;
            this.Age = ageOfTree;

            this.SharedConstruction();
        }

        public int EcodistrictId { get; set; }

        private void SharedConstruction()
        {
            this.Name = "Tree Annum";
        }

        #endregion

        #region Properties

        public HardinessZone HardinessZone { get; set; }

        public TreeGroupData TreeGroupData { get; set; }

        public double RowLength
        {
            get { return _rowLength; }
            set { this.SetProperty(ref _rowLength, value); }
        }

        public double TreeSpacing
        {
            get { return _treeSpacing; }
            set { this.SetProperty(ref _treeSpacing, value); }
        }

        public double TreeCount
        {
            get { return _treeCount; }
            set { this.SetProperty(ref _treeCount, value); }
        }

        public TreeSpecies TreeSpecies
        {
            get { return _treeSpecies; }
            set { this.SetProperty(ref _treeSpecies, value); }
        }

        public CircumferenceData CircumferenceData
        {
            get { return _circumferenceData; }
            set
            {
                if (_circumferenceData != null)
                {
                    _circumferenceData.PropertyChanged -= this.OnCircumferenceDataPropertyChanged;
                }

                this.SetProperty(ref _circumferenceData, value);
                if (_circumferenceData != null)
                {
                    _circumferenceData.PropertyChanged += this.OnCircumferenceDataPropertyChanged;
                }
            }
        }

        public double Year
        {
            get { return _year; }
            set { this.SetProperty(ref _year, value); }
        }

        public double AboveGroundCarbonStocksPerTree
        {
            get { return _aboveGroundCarbonKgPerTree; }
            set { this.SetProperty(ref _aboveGroundCarbonKgPerTree, value); }
        }

        public double BelowGroundCarbonKgPerTree
        {
            get { return _belowGroundCarbonKgPerTree; }
            set { this.SetProperty(ref _belowGroundCarbonKgPerTree, value); }
        }

        public string ShelterbeltName
        {
            get { return _shelterbeltName; }
            set { this.SetProperty(ref _shelterbeltName, value); }
        }

        public string RowName
        {
            get { return _rowName; }
            set { this.SetProperty(ref _rowName, value); }
        }

        public string TreeGroupName
        {
            get { return _treeGroupName; }
            set { this.SetProperty(ref _treeGroupName, value); }
        }

        public Guid ShelterbeltGuid
        {
            get { return _shelterbeltGuid; }
            set { this.SetProperty(ref _shelterbeltGuid, value); }
        }

        public Guid RowGuid
        {
            get { return _rowGuid; }
            set { this.SetProperty(ref _rowGuid, value); }
        }

        public Guid TreeGroupGuid
        {
            get { return _treeGroupGuid; }
            set { this.SetProperty(ref _treeGroupGuid, value); }
        }

        /// <summary>
        /// Percent mortality of an entire row of trees
        ///
        /// (%)
        /// </summary>
        public double PercentMortality
        {
            get
            {
                return _percentMortality;
            }
            set
            {
                SetProperty(ref _percentMortality, value);
            }
        }

        public double PercentMortalityHigh
        {
            get { return _percentMortalityHigh; }
            set {SetProperty(ref _percentMortalityHigh, value); }
        }

        public double PercentMortalityLow
        {
            get { return _percentMortalityLow; }
            set {SetProperty(ref _percentMortalityLow, value); }
        }

        public int Age
        {
            get { return _age; }
            set {SetProperty(ref _age, value); }
        }

        public double AboveGroundCarbonStocksOfAParticularKind { get; set; }
        public double AboveGroundBiomass { get; set; }

        /// <summary>
        /// The total biomass of the tree (includes aboveground and belowground)
        ///
        /// (kg)
        /// </summary>
        public double TotalBiomassPerTree
        {
            get { return _totalBiomassPerTree; }
            set { SetProperty(ref _totalBiomassPerTree, value); }
        }

        public double RootsBiomassPerTree { get; set; }
        public double RootsCarbonPerTree { get; set; }

        /// <summary>
        /// Total carbon per tree (includes aboveground and belowground)
        ///
        /// (kg C)
        /// </summary>
        public double TotalLivingCarbonPerTreeType 
        {
            get { return _totalLivingCarbonPerTreeType; }
            set { SetProperty(ref _totalLivingCarbonPerTreeType, value); }
        }

        /// <summary>
        /// Total biomass of all trees in the same row of the same species
        ///
        /// (kg)
        /// </summary>
        public double BiomassPerTreeType
        {
            get => _biomassPerTreeType;
            set => SetProperty(ref _biomassPerTreeType, value);
        }

        /// <summary>
        /// Total biomass of all trees in the same row of the same species per standard planting (i.e. 1 km)
        ///
        /// (kg km^-1)
        /// </summary>
        public double TotalLivingBiomassPerTreeType
        {
            get => _totalLivingBiomassPerTreeType;
            set => SetProperty(ref _totalLivingBiomassPerTreeType, value);
        }

        /// <summary>
        /// Total carbon of all trees in the same row of the same species
        ///
        /// (kg C species^-1 row^-1)
        /// </summary>
        public double TotalCarbonForAllTreesOfSameSpecies
        {
            get => _totalCarbonForAllTreesOfSameSpecies;
            set => SetProperty(ref _totalCarbonForAllTreesOfSameSpecies, value);
        }

        /// <summary>
        /// Total carbon for the shelterbelt (all rows) during the year
        ///
        /// (kg C shelterbelt^-1)
        /// </summary>
        public double TotalCarbonForShelterbeltAtYear
        {
            get => _totalCarbonForShelterbeltAtYear;
            set => SetProperty(ref _totalCarbonForShelterbeltAtYear, value);
        }

        public double TotalCarbonForAllTreesOfSameSpeciesPerStandardPlanting
        {
            get => _totalCarbonForAllTreesOfSameSpeciesPerStandardPlanting;
            set => SetProperty(ref _totalCarbonForAllTreesOfSameSpeciesPerStandardPlanting, value);
        }

        public double TotalCarbonForShelterbeltSequesteredAtYear
        {
            get => _totalCarbonForShelterbeltSequesteredAtYear;
            set => SetProperty(ref _totalCarbonForShelterbeltSequesteredAtYear, value);
        }

        public double TotalCarbonForShelterbeltAtYearPerStandardPlanting
        {
            get => _totalCarbonForShelterbeltAtYearPerStandardPlanting;
            set => SetProperty(ref _totalCarbonForShelterbeltAtYearPerStandardPlanting, value);
        }

        public double TotalCarbonDioxideForShelterbeltSequesteredAtYearPerStandardPlanting
        {
            get => _totalCarbonDioxideForShelterbeltSequesteredAtYearPerStandardPlanting;
            set => SetProperty(ref _totalCarbonDioxideForShelterbeltSequesteredAtYearPerStandardPlanting, value);
        }

        /// <summary>
        /// (kg C km^-1 year^-1)
        /// </summary>
        public double EstimatedDeadOrganicMatterBasedOnRealGrowth { get; set; }

        public double RealGrowthRatio { get; set; }

        /// <summary>
        /// (kg C km^-1)
        /// </summary>
        public double EstimatedBiomassCarbonBasedOnRealGrowth { get; set; }

        /// <summary>
        /// (kg C km^-1)
        /// </summary>
        public double BiomasCarbonPerKilometerInKilograms { get; set; }

        #endregion

        #region Public Methods

        public string GenerateAutonym()
        {
            return this.TreeGroupName + " - " + this.Year;
        }

        public string GetCorrectName()
        {
            if (this.NameIsFromUser)
            {
                return this.Name;
            }

            return this.GenerateAutonym();
        }

        #endregion

        #region Private Methods

        #endregion
    }
}