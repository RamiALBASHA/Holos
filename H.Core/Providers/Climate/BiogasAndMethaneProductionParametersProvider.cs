﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using H.Core.Converters;
using H.Core.Enumerations;
using H.Infrastructure;
using H.Content;

namespace H.Core.Providers.Climate
{
    public class BiogasAndMethaneProductionParametersProvider
    {
        #region Fields

        private readonly FarmResidueTypeStringConverter _farmResidueTypeStringConverter;
        private readonly BeddingMaterialTypeStringConverter _beddingMaterialTypeStringConverter;
        private readonly AnimalTypeStringConverter _animalTypeStringConverter;

        #endregion

        #region Constructors

        public BiogasAndMethaneProductionParametersProvider()
        {
            _farmResidueTypeStringConverter = new FarmResidueTypeStringConverter();
            _beddingMaterialTypeStringConverter = new BeddingMaterialTypeStringConverter();
            _animalTypeStringConverter = new AnimalTypeStringConverter();
            
            this.ReadFile();
        }

        #endregion

        #region Properties
        private List<BiogasAndMethaneProductionManureData> ManureData { get; set; }
        private List<BiogasAndMethaneProductionFarmResiduesData> FarmResiduesData { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds a manure substrate type by taking in an <see cref="AnimalType" /> and a <see cref="BeddingMaterialType"/>.
        /// </summary>
        /// <param name="animalType">The AnimalType whose manure we need the data for.</param>
        /// <param name="beddingMaterial">The bedding material for the animal. Specify 'None' for no bedding material.</param>
        /// <returns>Returns a single instance of <see cref="BiogasAndMethaneProductionParametersData"/> based on the parameters specified.</returns>
        public BiogasAndMethaneProductionParametersData GetBiogasMethaneProductionInstance(AnimalType animalType, BeddingMaterialType beddingMaterial)
        {
            BiogasAndMethaneProductionParametersData data = this.ManureData.Find(x => (x.AnimalType == animalType) && (x.BeddingType == beddingMaterial));

            if (data != null)
            {
                return data;
            }

            data = this.ManureData.Find(x => x.AnimalType == animalType);

            if (data != null)
            {
                Trace.TraceError($"{nameof(BiogasAndMethaneProductionParametersProvider)}.{nameof(BiogasAndMethaneProductionParametersProvider.GetBiogasMethaneProductionInstance)}" +
                    $" does not contain BeddingMaterialType of {beddingMaterial}. Returning an empty instance of {nameof(BiogasAndMethaneProductionParametersProvider)}");
            }
            else
            {
                Trace.TraceError($"{nameof(BiogasAndMethaneProductionParametersProvider)}.{nameof(BiogasAndMethaneProductionParametersProvider.GetBiogasMethaneProductionInstance)}" +
                    $" does not contain AnimalType of {animalType}. Returning an empty instance of {nameof(BiogasAndMethaneProductionParametersProvider)}");
            }

            return new BiogasAndMethaneProductionParametersData();

        }

        /// <summary>
        /// Finds a farm residues substrate type by taking in a <see cref="FarmResidueType"/> as the parameter.
        /// Unit of measurement: Biomethane potential = Nm3 ton-1 VS
        /// </summary>
        /// <param name="residueType">The farm residue type for which we need the required data values.</param>
        /// <returns>Returns a single instance of <see cref="BiogasAndMethaneProductionParametersData"/> based on the parameters specified. Returns an empty instance otherwise.</returns>
        public BiogasAndMethaneProductionParametersData GetBiogasMethaneProductionInstance(FarmResidueType residueType)
        {
            BiogasAndMethaneProductionParametersData data = this.FarmResiduesData.Find(x => x.ResidueType == residueType);

            if (data != null)
            {
                return data;
            }

            Trace.TraceError($"{nameof(BiogasAndMethaneProductionParametersProvider)}.{nameof(BiogasAndMethaneProductionParametersProvider.GetBiogasMethaneProductionInstance)}" +
             $" does not contain FarmResidueTyoe of {residueType}. Returning an empty instance of {nameof(BiogasAndMethaneProductionParametersProvider)}");

            return new BiogasAndMethaneProductionParametersData();

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads the csv file and stores data into two lists. Each list corresponds to Manure Type and Farm Residue Type substrates enteries in the csv respectively.
        /// </summary>
        private void ReadFile()
        {
            // If more manure type subtrates are added to csv file, increase the value of int const LastManureRow accordingly.
            // If LastManureRow is changed, also change the lines the foreach loop skips to get to farm residue substrate types.

            List<BiogasAndMethaneProductionManureData> manureData = new List<BiogasAndMethaneProductionManureData>();
            List<BiogasAndMethaneProductionFarmResiduesData> residueData = new List<BiogasAndMethaneProductionFarmResiduesData>();

            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            IEnumerable<string[]> fileLines = CsvResourceReader.GetFileLines(CsvResourceNames.ParametersBiogasMethaneProduction);

            double biomethanePotential, methaneFraction, volatileSolids, totalSolids, totalNitrogen;

            // LastManureRow indicates the last row with a manure type substrate in the csv file.
            const int LastManureRow = 10;
            // Store the first half of the file into a manure type data list. Row's value is based on the location of the first row containing manure data in the csv.
            for (int row = 2; row < LastManureRow; row++)
            {
                string[] line = fileLines.ElementAt(row);

                AnimalType animalType = _animalTypeStringConverter.Convert(line[0]);
                BeddingMaterialType beddingType = _beddingMaterialTypeStringConverter.Convert(line[1]);
                biomethanePotential = double.Parse(line[2], cultureInfo);
                methaneFraction = double.Parse(line[3], cultureInfo);
                volatileSolids = double.Parse(line[4], cultureInfo);
                totalSolids = double.Parse(line[5], cultureInfo);
                totalNitrogen = double.Parse(line[6], cultureInfo);

                manureData.Add(new BiogasAndMethaneProductionManureData
                {
                    AnimalType = animalType,
                    BeddingType = beddingType,
                    BioMethanePotential = biomethanePotential,
                    MethaneFraction = methaneFraction,
                    VolatileSolids = volatileSolids,
                    TotalSolids = totalSolids,
                    TotalNitrogen = totalNitrogen,
                });
            }

            // We're skipping 11 lines as the farm residue entries in the csv start at row 11.
            // Store the second half of the file into a farm residue type data list.
            foreach (string[] line in fileLines.Skip(11))
            {
                FarmResidueType residueType = _farmResidueTypeStringConverter.Convert(line[0]);
                biomethanePotential = double.Parse(line[2], cultureInfo);
                methaneFraction = double.Parse(line[3], cultureInfo);
                volatileSolids = double.Parse(line[4], cultureInfo);
                totalSolids = double.Parse(line[5], cultureInfo);
                totalNitrogen = double.Parse(line[6], cultureInfo);

                residueData.Add(new BiogasAndMethaneProductionFarmResiduesData
                {
                    ResidueType = residueType,
                    BioMethanePotential = biomethanePotential,
                    MethaneFraction = methaneFraction,
                    VolatileSolids = volatileSolids,
                    TotalSolids = totalSolids,
                    TotalNitrogen = totalNitrogen,
                });
            }
            
            ManureData = manureData;
            FarmResiduesData = residueData;
        }

        #endregion
    }
}
