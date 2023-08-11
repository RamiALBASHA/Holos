﻿using H.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.CLI.Interfaces;
using H.Core.Models.Animals;
using System.Globalization;

namespace H.CLI.ComponentKeys
{
    public class OtherLivestockKeys : AnimalKeyBase, IComponentKeys
    {
        #region Constructors

        public OtherLivestockKeys() : base()
        {
            base.Keys.Add(Properties.Resources.Key_Name, null);
            base.Keys.Add(H.Core.Properties.Resources.ComponentType, null);
            base.Keys.Add(Properties.Resources.GroupName, null);
            base.Keys.Add(Properties.Resources.GroupType, null);

            base.Keys.Add(Properties.Resources.ManagementPeriodName, null);
            base.Keys.Add(Properties.Resources.ManagementPeriodStartDate, null);
            base.Keys.Add(Properties.Resources.ManagementPeriodDays, null);
            base.Keys.Add(Properties.Resources.NumberOfAnimals, null);

            base.Keys.Add(Properties.Resources.YearlyManureMethaneRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(Properties.Resources.YearlyNitrogenExcretionRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(Properties.Resources.YearlyEntericMethaneRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(Properties.Resources.N2ODirectEmissionFactor, ImperialUnitsOfMeasurement.PoundsN2ONPerPoundN);
            base.Keys.Add(Properties.Resources.VolatilizationFraction, null);

            base.Keys.Add(Properties.Resources.DailyManureMethaneEmissionRate, null);
            base.Keys.Add(Properties.Resources.MethaneProducingCapacityOfManure, null);
            base.Keys.Add(Properties.Resources.MethaneConversionFactorOfManure, null);
            base.Keys.Add(Properties.Resources.VolatileSolids, null);
            base.Keys.Add(Properties.Resources.EmissionFactorVolatilization, null);
            base.Keys.Add(Properties.Resources.EmissionFactorLeaching, null);
            base.Keys.Add(Properties.Resources.FractionLeaching, null);
        }

        #endregion
    }
}
