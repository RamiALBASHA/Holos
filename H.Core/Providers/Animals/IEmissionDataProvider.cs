﻿using H.Core.Enumerations;
using H.Core.Models;

namespace H.Core.Providers.Animals
{
    public interface IEmissionDataProvider
    {
        IEmissionData GetByHandlingSystem(ManureStateType manureStateType, ComponentCategory componentCategory,
                                          double meanAnnualPrecipitation, double meanAnnualTemperature,
                                          double meanAnnualEvapotranspiration, double beddingRate,
                                          AnimalType animalType);
    }
}