﻿namespace Savanna.Core.Models.DTOs
{
    public record GameSaveDto(
    int Id,
    string SaveName,
    int IterationCount,
    int LivingAnimalCount,
    DateTime SaveDate
);
}
