﻿using H.Infrastructure;

namespace H.Core.Models.Animals.OtherAnimals
{
    public class HorsesComponent : AnimalComponentBase
    {
        public HorsesComponent()
        {
            this.ComponentNameDisplayString = ComponentType.Horses.GetDescription();
            this.ComponentCategory = ComponentCategory.OtherLivestock;
            this.ComponentType = ComponentType.Horses;
        }
    }
}