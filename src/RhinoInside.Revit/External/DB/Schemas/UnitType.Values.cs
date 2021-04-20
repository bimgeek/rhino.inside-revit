using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RhinoInside.Revit.External.DB.Schemas
{
  public partial class UnitType
  {
    public static UnitType OneToRatio => new UnitType("autodesk.unit.unit:1ToRatio-1.0.1");
    public static UnitType Acres => new UnitType("autodesk.unit.unit:acres-1.0.1");
    public static UnitType Amperes => new UnitType("autodesk.unit.unit:amperes-1.0.0");
    public static UnitType Atmospheres => new UnitType("autodesk.unit.unit:atmospheres-1.0.1");
    public static UnitType Bars => new UnitType("autodesk.unit.unit:bars-1.0.1");
    public static UnitType BritishThermalUnits => new UnitType("autodesk.unit.unit:britishThermalUnits-1.0.1");
    public static UnitType BritishThermalUnitsPerDegreeFahrenheit => new UnitType("autodesk.unit.unit:britishThermalUnitsPerDegreeFahrenheit-1.0.1");
    public static UnitType BritishThermalUnitsPerHour => new UnitType("autodesk.unit.unit:britishThermalUnitsPerHour-1.0.1");
    public static UnitType BritishThermalUnitsPerHourCubicFoot => new UnitType("autodesk.unit.unit:britishThermalUnitsPerHourCubicFoot-1.0.1");
    public static UnitType BritishThermalUnitsPerHourFootDegreeFahrenheit => new UnitType("autodesk.unit.unit:britishThermalUnitsPerHourFootDegreeFahrenheit-1.0.1");
    public static UnitType BritishThermalUnitsPerHourSquareFoot => new UnitType("autodesk.unit.unit:britishThermalUnitsPerHourSquareFoot-1.0.1");
    public static UnitType BritishThermalUnitsPerHourSquareFootDegreeFahrenheit => new UnitType("autodesk.unit.unit:britishThermalUnitsPerHourSquareFootDegreeFahrenheit-1.0.1");
    public static UnitType BritishThermalUnitsPerPound => new UnitType("autodesk.unit.unit:britishThermalUnitsPerPound-1.0.1");
    public static UnitType BritishThermalUnitsPerPoundDegreeFahrenheit => new UnitType("autodesk.unit.unit:britishThermalUnitsPerPoundDegreeFahrenheit-1.0.1");
    public static UnitType BritishThermalUnitsPerSecond => new UnitType("autodesk.unit.unit:britishThermalUnitsPerSecond-1.0.1");
    public static UnitType Calories => new UnitType("autodesk.unit.unit:calories-1.0.1");
    public static UnitType CaloriesPerSecond => new UnitType("autodesk.unit.unit:caloriesPerSecond-1.0.1");
    public static UnitType Candelas => new UnitType("autodesk.unit.unit:candelas-1.0.0");
    public static UnitType CandelasPerSquareFoot => new UnitType("autodesk.unit.unit:candelasPerSquareFoot-1.0.0");
    public static UnitType CandelasPerSquareMeter => new UnitType("autodesk.unit.unit:candelasPerSquareMeter-1.0.1");
    public static UnitType Celsius => new UnitType("autodesk.unit.unit:celsius-1.0.1");
    public static UnitType CelsiusInterval => new UnitType("autodesk.unit.unit:celsiusInterval-1.0.1");
    public static UnitType Centimeters => new UnitType("autodesk.unit.unit:centimeters-1.0.1");
    public static UnitType CentimetersPerMinute => new UnitType("autodesk.unit.unit:centimetersPerMinute-1.0.1");
    public static UnitType CentimetersToTheFourthPower => new UnitType("autodesk.unit.unit:centimetersToTheFourthPower-1.0.1");
    public static UnitType CentimetersToTheSixthPower => new UnitType("autodesk.unit.unit:centimetersToTheSixthPower-1.0.1");
    public static UnitType Centipoises => new UnitType("autodesk.unit.unit:centipoises-1.0.1");
    public static UnitType CubicCentimeters => new UnitType("autodesk.unit.unit:cubicCentimeters-1.0.1");
    public static UnitType CubicFeet => new UnitType("autodesk.unit.unit:cubicFeet-1.0.1");
    public static UnitType CubicFeetPerHour => new UnitType("autodesk.unit.unit:cubicFeetPerHour-1.0.0");
    public static UnitType CubicFeetPerKip => new UnitType("autodesk.unit.unit:cubicFeetPerKip-1.0.1");
    public static UnitType CubicFeetPerMinute => new UnitType("autodesk.unit.unit:cubicFeetPerMinute-1.0.1");
    public static UnitType CubicFeetPerMinuteCubicFoot => new UnitType("autodesk.unit.unit:cubicFeetPerMinuteCubicFoot-1.0.1");
    public static UnitType CubicFeetPerMinutePerBritishThermalUnitPerHour => new UnitType("autodesk.unit.unit:cubicFeetPerMinutePerBritishThermalUnitPerHour-1.0.0");
    public static UnitType CubicFeetPerMinuteSquareFoot => new UnitType("autodesk.unit.unit:cubicFeetPerMinuteSquareFoot-1.0.1");
    public static UnitType CubicFeetPerMinuteTonOfRefrigeration => new UnitType("autodesk.unit.unit:cubicFeetPerMinuteTonOfRefrigeration-1.0.1");
    public static UnitType CubicFeetPerPoundMass => new UnitType("autodesk.unit.unit:cubicFeetPerPoundMass-1.0.0");
    public static UnitType CubicInches => new UnitType("autodesk.unit.unit:cubicInches-1.0.1");
    public static UnitType CubicMeters => new UnitType("autodesk.unit.unit:cubicMeters-1.0.1");
    public static UnitType CubicMetersPerHour => new UnitType("autodesk.unit.unit:cubicMetersPerHour-1.0.1");
    public static UnitType CubicMetersPerHourCubicMeter => new UnitType("autodesk.unit.unit:cubicMetersPerHourCubicMeter-1.0.0");
    public static UnitType CubicMetersPerHourSquareMeter => new UnitType("autodesk.unit.unit:cubicMetersPerHourSquareMeter-1.0.0");
    public static UnitType CubicMetersPerKilogram => new UnitType("autodesk.unit.unit:cubicMetersPerKilogram-1.0.0");
    public static UnitType CubicMetersPerKilonewton => new UnitType("autodesk.unit.unit:cubicMetersPerKilonewton-1.0.1");
    public static UnitType CubicMetersPerSecond => new UnitType("autodesk.unit.unit:cubicMetersPerSecond-1.0.1");
    public static UnitType CubicMetersPerWattSecond => new UnitType("autodesk.unit.unit:cubicMetersPerWattSecond-1.0.0");
    public static UnitType CubicMillimeters => new UnitType("autodesk.unit.unit:cubicMillimeters-1.0.1");
    public static UnitType CubicYards => new UnitType("autodesk.unit.unit:cubicYards-1.0.1");
    public static UnitType Currency => new UnitType("autodesk.unit.unit:currency-1.0.0");
    public static UnitType CurrencyPerBritishThermalUnit => new UnitType("autodesk.unit.unit:currencyPerBritishThermalUnit-1.0.1");
    public static UnitType CurrencyPerBritishThermalUnitPerHour => new UnitType("autodesk.unit.unit:currencyPerBritishThermalUnitPerHour-1.0.1");
    public static UnitType CurrencyPerSquareFoot => new UnitType("autodesk.unit.unit:currencyPerSquareFoot-1.0.1");
    public static UnitType CurrencyPerSquareMeter => new UnitType("autodesk.unit.unit:currencyPerSquareMeter-1.0.1");
    public static UnitType CurrencyPerWatt => new UnitType("autodesk.unit.unit:currencyPerWatt-1.0.1");
    public static UnitType CurrencyPerWattHour => new UnitType("autodesk.unit.unit:currencyPerWattHour-1.0.1");
    public static UnitType CyclesPerSecond => new UnitType("autodesk.unit.unit:cyclesPerSecond-1.0.1");
    public static UnitType Decimeters => new UnitType("autodesk.unit.unit:decimeters-1.0.1");
    public static UnitType Degrees => new UnitType("autodesk.unit.unit:degrees-1.0.1");
    public static UnitType DegreesMinutes => new UnitType("autodesk.unit.unit:degreesMinutes-1.0.0");
    public static UnitType DekanewtonMeters => new UnitType("autodesk.unit.unit:dekanewtonMeters-1.0.1");
    public static UnitType DekanewtonMetersPerMeter => new UnitType("autodesk.unit.unit:dekanewtonMetersPerMeter-1.0.1");
    public static UnitType Dekanewtons => new UnitType("autodesk.unit.unit:dekanewtons-1.0.1");
    public static UnitType DekanewtonsPerMeter => new UnitType("autodesk.unit.unit:dekanewtonsPerMeter-1.0.1");
    public static UnitType DekanewtonsPerSquareMeter => new UnitType("autodesk.unit.unit:dekanewtonsPerSquareMeter-1.0.1");
    public static UnitType Fahrenheit => new UnitType("autodesk.unit.unit:fahrenheit-1.0.1");
    public static UnitType FahrenheitInterval => new UnitType("autodesk.unit.unit:fahrenheitInterval-1.0.1");
    public static UnitType Feet => new UnitType("autodesk.unit.unit:feet-1.0.1");
    public static UnitType FeetFractionalInches => new UnitType("autodesk.unit.unit:feetFractionalInches-1.0.0");
    public static UnitType FeetOfWater39_2DegreesFahrenheit => new UnitType("autodesk.unit.unit:feetOfWater39.2DegreesFahrenheit-1.0.1");
    public static UnitType FeetOfWater39_2DegreesFahrenheitPer100Feet => new UnitType("autodesk.unit.unit:feetOfWater39.2DegreesFahrenheitPer100Feet-1.0.1");
    public static UnitType FeetPerKip => new UnitType("autodesk.unit.unit:feetPerKip-1.0.0");
    public static UnitType FeetPerMinute => new UnitType("autodesk.unit.unit:feetPerMinute-1.0.1");
    public static UnitType FeetPerSecond => new UnitType("autodesk.unit.unit:feetPerSecond-1.0.1");
    public static UnitType FeetPerSecondSquared => new UnitType("autodesk.unit.unit:feetPerSecondSquared-1.0.1");
    public static UnitType FeetToTheFourthPower => new UnitType("autodesk.unit.unit:feetToTheFourthPower-1.0.1");
    public static UnitType FeetToTheSixthPower => new UnitType("autodesk.unit.unit:feetToTheSixthPower-1.0.1");
    public static UnitType Fixed => new UnitType("autodesk.unit.unit:fixed-1.0.1");
    public static UnitType Footcandles => new UnitType("autodesk.unit.unit:footcandles-1.0.1");
    public static UnitType Footlamberts => new UnitType("autodesk.unit.unit:footlamberts-1.0.1");
    public static UnitType FractionalInches => new UnitType("autodesk.unit.unit:fractionalInches-1.0.0");
    public static UnitType General => new UnitType("autodesk.unit.unit:general-1.0.1");
    public static UnitType Gradians => new UnitType("autodesk.unit.unit:gradians-1.0.1");
    public static UnitType Grains => new UnitType("autodesk.unit.unit:grains-1.0.1");
    public static UnitType GrainsPerHourSquareFootInchMercury => new UnitType("autodesk.unit.unit:grainsPerHourSquareFootInchMercury-1.0.1");
    public static UnitType Grams => new UnitType("autodesk.unit.unit:grams-1.0.1");
    public static UnitType Hectares => new UnitType("autodesk.unit.unit:hectares-1.0.1");
    public static UnitType Hectometers => new UnitType("autodesk.unit.unit:hectometers-1.0.1");
    public static UnitType Hertz => new UnitType("autodesk.unit.unit:hertz-1.0.1");
    public static UnitType Horsepower => new UnitType("autodesk.unit.unit:horsepower-1.0.1");
    public static UnitType Hours => new UnitType("autodesk.unit.unit:hours-1.0.1");
    public static UnitType HourSquareFootDegreesFahrenheitPerBritishThermalUnit => new UnitType("autodesk.unit.unit:hourSquareFootDegreesFahrenheitPerBritishThermalUnit-1.0.1");
    public static UnitType Inches => new UnitType("autodesk.unit.unit:inches-1.0.1");
    public static UnitType InchesOfMercury32DegreesFahrenheit => new UnitType("autodesk.unit.unit:inchesOfMercury32DegreesFahrenheit-1.0.1");
    public static UnitType InchesOfWater60DegreesFahrenheit => new UnitType("autodesk.unit.unit:inchesOfWater60DegreesFahrenheit-1.0.1");
    public static UnitType InchesOfWater60DegreesFahrenheitPer100Feet => new UnitType("autodesk.unit.unit:inchesOfWater60DegreesFahrenheitPer100Feet-1.0.1");
    public static UnitType InchesPerSecond => new UnitType("autodesk.unit.unit:inchesPerSecond-1.0.1");
    public static UnitType InchesPerSecondSquared => new UnitType("autodesk.unit.unit:inchesPerSecondSquared-1.0.1");
    public static UnitType InchesToTheFourthPower => new UnitType("autodesk.unit.unit:inchesToTheFourthPower-1.0.1");
    public static UnitType InchesToTheSixthPower => new UnitType("autodesk.unit.unit:inchesToTheSixthPower-1.0.1");
    public static UnitType InverseDegreesCelsius => new UnitType("autodesk.unit.unit:inverseDegreesCelsius-1.0.1");
    public static UnitType InverseDegreesFahrenheit => new UnitType("autodesk.unit.unit:inverseDegreesFahrenheit-1.0.1");
    public static UnitType InverseKilonewtons => new UnitType("autodesk.unit.unit:inverseKilonewtons-1.0.0");
    public static UnitType InverseKips => new UnitType("autodesk.unit.unit:inverseKips-1.0.0");
    public static UnitType Joules => new UnitType("autodesk.unit.unit:joules-1.0.1");
    public static UnitType JoulesPerGram => new UnitType("autodesk.unit.unit:joulesPerGram-1.0.1");
    public static UnitType JoulesPerGramDegreeCelsius => new UnitType("autodesk.unit.unit:joulesPerGramDegreeCelsius-1.0.1");
    public static UnitType JoulesPerKelvin => new UnitType("autodesk.unit.unit:joulesPerKelvin-1.0.1");
    public static UnitType JoulesPerKilogram => new UnitType("autodesk.unit.unit:joulesPerKilogram-1.0.1");
    public static UnitType JoulesPerKilogramDegreeCelsius => new UnitType("autodesk.unit.unit:joulesPerKilogramDegreeCelsius-1.0.1");
    public static UnitType Kelvin => new UnitType("autodesk.unit.unit:kelvin-1.0.0");
    public static UnitType KelvinInterval => new UnitType("autodesk.unit.unit:kelvinInterval-1.0.0");
    public static UnitType Kiloamperes => new UnitType("autodesk.unit.unit:kiloamperes-1.0.1");
    public static UnitType Kilocalories => new UnitType("autodesk.unit.unit:kilocalories-1.0.1");
    public static UnitType KilocaloriesPerSecond => new UnitType("autodesk.unit.unit:kilocaloriesPerSecond-1.0.1");
    public static UnitType KilogramForceMeters => new UnitType("autodesk.unit.unit:kilogramForceMeters-1.0.1");
    public static UnitType KilogramForceMetersPerMeter => new UnitType("autodesk.unit.unit:kilogramForceMetersPerMeter-1.0.1");
    public static UnitType KilogramKelvins => new UnitType("autodesk.unit.unit:kilogramKelvins-1.0.0");
    public static UnitType Kilograms => new UnitType("autodesk.unit.unit:kilograms-1.0.0");
    public static UnitType KilogramsForce => new UnitType("autodesk.unit.unit:kilogramsForce-1.0.1");
    public static UnitType KilogramsForcePerMeter => new UnitType("autodesk.unit.unit:kilogramsForcePerMeter-1.0.1");
    public static UnitType KilogramsForcePerSquareMeter => new UnitType("autodesk.unit.unit:kilogramsForcePerSquareMeter-1.0.1");
    public static UnitType KilogramsPerCubicMeter => new UnitType("autodesk.unit.unit:kilogramsPerCubicMeter-1.0.1");
    public static UnitType KilogramsPerHour => new UnitType("autodesk.unit.unit:kilogramsPerHour-1.0.0");
    public static UnitType KilogramsPerKilogramKelvin => new UnitType("autodesk.unit.unit:kilogramsPerKilogramKelvin-1.0.0");
    public static UnitType KilogramsPerMeter => new UnitType("autodesk.unit.unit:kilogramsPerMeter-1.0.1");
    public static UnitType KilogramsPerMeterHour => new UnitType("autodesk.unit.unit:kilogramsPerMeterHour-1.0.0");
    public static UnitType KilogramsPerMeterSecond => new UnitType("autodesk.unit.unit:kilogramsPerMeterSecond-1.0.0");
    public static UnitType KilogramsPerMinute => new UnitType("autodesk.unit.unit:kilogramsPerMinute-1.0.0");
    public static UnitType KilogramsPerSecond => new UnitType("autodesk.unit.unit:kilogramsPerSecond-1.0.0");
    public static UnitType KilogramsPerSquareMeter => new UnitType("autodesk.unit.unit:kilogramsPerSquareMeter-1.0.1");
    public static UnitType Kilojoules => new UnitType("autodesk.unit.unit:kilojoules-1.0.1");
    public static UnitType KilojoulesPerKelvin => new UnitType("autodesk.unit.unit:kilojoulesPerKelvin-1.0.1");
    public static UnitType Kilometers => new UnitType("autodesk.unit.unit:kilometers-1.0.1");
    public static UnitType KilometersPerHour => new UnitType("autodesk.unit.unit:kilometersPerHour-1.0.1");
    public static UnitType KilometersPerSecond => new UnitType("autodesk.unit.unit:kilometersPerSecond-1.0.1");
    public static UnitType KilometersPerSecondSquared => new UnitType("autodesk.unit.unit:kilometersPerSecondSquared-1.0.1");
    public static UnitType KilonewtonMeters => new UnitType("autodesk.unit.unit:kilonewtonMeters-1.0.1");
    public static UnitType KilonewtonMetersPerDegree => new UnitType("autodesk.unit.unit:kilonewtonMetersPerDegree-1.0.1");
    public static UnitType KilonewtonMetersPerDegreePerMeter => new UnitType("autodesk.unit.unit:kilonewtonMetersPerDegreePerMeter-1.0.1");
    public static UnitType KilonewtonMetersPerMeter => new UnitType("autodesk.unit.unit:kilonewtonMetersPerMeter-1.0.1");
    public static UnitType Kilonewtons => new UnitType("autodesk.unit.unit:kilonewtons-1.0.1");
    public static UnitType KilonewtonsPerCubicMeter => new UnitType("autodesk.unit.unit:kilonewtonsPerCubicMeter-1.0.1");
    public static UnitType KilonewtonsPerMeter => new UnitType("autodesk.unit.unit:kilonewtonsPerMeter-1.0.1");
    public static UnitType KilonewtonsPerSquareCentimeter => new UnitType("autodesk.unit.unit:kilonewtonsPerSquareCentimeter-1.0.1");
    public static UnitType KilonewtonsPerSquareMeter => new UnitType("autodesk.unit.unit:kilonewtonsPerSquareMeter-1.0.1");
    public static UnitType KilonewtonsPerSquareMillimeter => new UnitType("autodesk.unit.unit:kilonewtonsPerSquareMillimeter-1.0.1");
    public static UnitType Kilopascals => new UnitType("autodesk.unit.unit:kilopascals-1.0.1");
    public static UnitType KilovoltAmperes => new UnitType("autodesk.unit.unit:kilovoltAmperes-1.0.1");
    public static UnitType Kilovolts => new UnitType("autodesk.unit.unit:kilovolts-1.0.1");
    public static UnitType KilowattHours => new UnitType("autodesk.unit.unit:kilowattHours-1.0.1");
    public static UnitType Kilowatts => new UnitType("autodesk.unit.unit:kilowatts-1.0.1");
    public static UnitType KipFeet => new UnitType("autodesk.unit.unit:kipFeet-1.0.1");
    public static UnitType KipFeetPerDegree => new UnitType("autodesk.unit.unit:kipFeetPerDegree-1.0.1");
    public static UnitType KipFeetPerDegreePerFoot => new UnitType("autodesk.unit.unit:kipFeetPerDegreePerFoot-1.0.1");
    public static UnitType KipFeetPerFoot => new UnitType("autodesk.unit.unit:kipFeetPerFoot-1.0.1");
    public static UnitType Kips => new UnitType("autodesk.unit.unit:kips-1.0.1");
    public static UnitType KipsPerCubicFoot => new UnitType("autodesk.unit.unit:kipsPerCubicFoot-1.0.1");
    public static UnitType KipsPerCubicInch => new UnitType("autodesk.unit.unit:kipsPerCubicInch-1.0.1");
    public static UnitType KipsPerFoot => new UnitType("autodesk.unit.unit:kipsPerFoot-1.0.1");
    public static UnitType KipsPerInch => new UnitType("autodesk.unit.unit:kipsPerInch-1.0.1");
    public static UnitType KipsPerSquareFoot => new UnitType("autodesk.unit.unit:kipsPerSquareFoot-1.0.1");
    public static UnitType KipsPerSquareInch => new UnitType("autodesk.unit.unit:kipsPerSquareInch-1.0.1");
    public static UnitType Liters => new UnitType("autodesk.unit.unit:liters-1.0.1");
    public static UnitType LitersPerHour => new UnitType("autodesk.unit.unit:litersPerHour-1.0.1");
    public static UnitType LitersPerMinute => new UnitType("autodesk.unit.unit:litersPerMinute-1.0.1");
    public static UnitType LitersPerSecond => new UnitType("autodesk.unit.unit:litersPerSecond-1.0.1");
    public static UnitType LitersPerSecondCubicMeter => new UnitType("autodesk.unit.unit:litersPerSecondCubicMeter-1.0.1");
    public static UnitType LitersPerSecondKilowatt => new UnitType("autodesk.unit.unit:litersPerSecondKilowatt-1.0.1");
    public static UnitType LitersPerSecondSquareMeter => new UnitType("autodesk.unit.unit:litersPerSecondSquareMeter-1.0.1");
    public static UnitType Lumens => new UnitType("autodesk.unit.unit:lumens-1.0.1");
    public static UnitType LumensPerWatt => new UnitType("autodesk.unit.unit:lumensPerWatt-1.0.1");
    public static UnitType Lux => new UnitType("autodesk.unit.unit:lux-1.0.1");
    public static UnitType MeganewtonMeters => new UnitType("autodesk.unit.unit:meganewtonMeters-1.0.1");
    public static UnitType MeganewtonMetersPerMeter => new UnitType("autodesk.unit.unit:meganewtonMetersPerMeter-1.0.1");
    public static UnitType Meganewtons => new UnitType("autodesk.unit.unit:meganewtons-1.0.1");
    public static UnitType MeganewtonsPerMeter => new UnitType("autodesk.unit.unit:meganewtonsPerMeter-1.0.1");
    public static UnitType MeganewtonsPerSquareMeter => new UnitType("autodesk.unit.unit:meganewtonsPerSquareMeter-1.0.1");
    public static UnitType Megapascals => new UnitType("autodesk.unit.unit:megapascals-1.0.1");
    public static UnitType Meters => new UnitType("autodesk.unit.unit:meters-1.0.0");
    public static UnitType MetersCentimeters => new UnitType("autodesk.unit.unit:metersCentimeters-1.0.0");
    public static UnitType MetersOfWaterColumn => new UnitType("autodesk.unit.unit:metersOfWaterColumn-1.0.0");
    public static UnitType MetersOfWaterColumnPerMeter => new UnitType("autodesk.unit.unit:metersOfWaterColumnPerMeter-1.0.0");
    public static UnitType MetersPerKilonewton => new UnitType("autodesk.unit.unit:metersPerKilonewton-1.0.0");
    public static UnitType MetersPerSecond => new UnitType("autodesk.unit.unit:metersPerSecond-1.0.1");
    public static UnitType MetersPerSecondSquared => new UnitType("autodesk.unit.unit:metersPerSecondSquared-1.0.1");
    public static UnitType MetersToTheFourthPower => new UnitType("autodesk.unit.unit:metersToTheFourthPower-1.0.1");
    public static UnitType MetersToTheSixthPower => new UnitType("autodesk.unit.unit:metersToTheSixthPower-1.0.1");
    public static UnitType MicroinchesPerInchDegreeFahrenheit => new UnitType("autodesk.unit.unit:microinchesPerInchDegreeFahrenheit-1.0.1");
    public static UnitType MicrometersPerMeterDegreeCelsius => new UnitType("autodesk.unit.unit:micrometersPerMeterDegreeCelsius-1.0.1");
    public static UnitType Miles => new UnitType("autodesk.unit.unit:miles-1.0.1");
    public static UnitType MilesPerHour => new UnitType("autodesk.unit.unit:milesPerHour-1.0.1");
    public static UnitType MilesPerSecond => new UnitType("autodesk.unit.unit:milesPerSecond-1.0.1");
    public static UnitType MilesPerSecondSquared => new UnitType("autodesk.unit.unit:milesPerSecondSquared-1.0.1");
    public static UnitType Milliamperes => new UnitType("autodesk.unit.unit:milliamperes-1.0.1");
    public static UnitType Milligrams => new UnitType("autodesk.unit.unit:milligrams-1.0.1");
    public static UnitType Millimeters => new UnitType("autodesk.unit.unit:millimeters-1.0.1");
    public static UnitType MillimetersOfMercury => new UnitType("autodesk.unit.unit:millimetersOfMercury-1.0.1");
    public static UnitType MillimetersOfWaterColumn => new UnitType("autodesk.unit.unit:millimetersOfWaterColumn-1.0.0");
    public static UnitType MillimetersOfWaterColumnPerMeter => new UnitType("autodesk.unit.unit:millimetersOfWaterColumnPerMeter-1.0.0");
    public static UnitType MillimetersToTheFourthPower => new UnitType("autodesk.unit.unit:millimetersToTheFourthPower-1.0.1");
    public static UnitType MillimetersToTheSixthPower => new UnitType("autodesk.unit.unit:millimetersToTheSixthPower-1.0.1");
    public static UnitType Milliseconds => new UnitType("autodesk.unit.unit:milliseconds-1.0.1");
    public static UnitType Millivolts => new UnitType("autodesk.unit.unit:millivolts-1.0.1");
    public static UnitType Minutes => new UnitType("autodesk.unit.unit:minutes-1.0.1");
    public static UnitType Nanograms => new UnitType("autodesk.unit.unit:nanograms-1.0.1");
    public static UnitType NanogramsPerPascalSecondSquareMeter => new UnitType("autodesk.unit.unit:nanogramsPerPascalSecondSquareMeter-1.0.1");
    public static UnitType NewtonMeters => new UnitType("autodesk.unit.unit:newtonMeters-1.0.1");
    public static UnitType NewtonMetersPerMeter => new UnitType("autodesk.unit.unit:newtonMetersPerMeter-1.0.1");
    public static UnitType Newtons => new UnitType("autodesk.unit.unit:newtons-1.0.1");
    public static UnitType NewtonSecondsPerSquareMeter => new UnitType("autodesk.unit.unit:newtonSecondsPerSquareMeter-1.0.0");
    public static UnitType NewtonsPerMeter => new UnitType("autodesk.unit.unit:newtonsPerMeter-1.0.1");
    public static UnitType NewtonsPerSquareMeter => new UnitType("autodesk.unit.unit:newtonsPerSquareMeter-1.0.1");
    public static UnitType NewtonsPerSquareMillimeter => new UnitType("autodesk.unit.unit:newtonsPerSquareMillimeter-1.0.1");
    public static UnitType OhmMeters => new UnitType("autodesk.unit.unit:ohmMeters-1.0.1");
    public static UnitType Ohms => new UnitType("autodesk.unit.unit:ohms-1.0.1");
    public static UnitType Pascals => new UnitType("autodesk.unit.unit:pascals-1.0.1");
    public static UnitType PascalSeconds => new UnitType("autodesk.unit.unit:pascalSeconds-1.0.1");
    public static UnitType PascalsPerMeter => new UnitType("autodesk.unit.unit:pascalsPerMeter-1.0.1");
    public static UnitType Percentage => new UnitType("autodesk.unit.unit:percentage-1.0.1");
    public static UnitType PerMille => new UnitType("autodesk.unit.unit:perMille-1.0.1");
    public static UnitType Pi => new UnitType("autodesk.unit.unit:pi-1.0.0");
    public static UnitType Poises => new UnitType("autodesk.unit.unit:poises-1.0.1");
    public static UnitType PoundForceFeet => new UnitType("autodesk.unit.unit:poundForceFeet-1.0.1");
    public static UnitType PoundForceFeetPerFoot => new UnitType("autodesk.unit.unit:poundForceFeetPerFoot-1.0.1");
    public static UnitType PoundForceSecondsPerSquareFoot => new UnitType("autodesk.unit.unit:poundForceSecondsPerSquareFoot-1.0.0");
    public static UnitType PoundMassDegreesFahrenheit => new UnitType("autodesk.unit.unit:poundMassDegreesFahrenheit-1.0.0");
    public static UnitType PoundsForce => new UnitType("autodesk.unit.unit:poundsForce-1.0.1");
    public static UnitType PoundsForcePerCubicFoot => new UnitType("autodesk.unit.unit:poundsForcePerCubicFoot-1.0.1");
    public static UnitType PoundsForcePerFoot => new UnitType("autodesk.unit.unit:poundsForcePerFoot-1.0.1");
    public static UnitType PoundsForcePerSquareFoot => new UnitType("autodesk.unit.unit:poundsForcePerSquareFoot-1.0.1");
    public static UnitType PoundsForcePerSquareInch => new UnitType("autodesk.unit.unit:poundsForcePerSquareInch-1.0.1");
    public static UnitType PoundsMass => new UnitType("autodesk.unit.unit:poundsMass-1.0.1");
    public static UnitType PoundsMassPerCubicFoot => new UnitType("autodesk.unit.unit:poundsMassPerCubicFoot-1.0.1");
    public static UnitType PoundsMassPerCubicInch => new UnitType("autodesk.unit.unit:poundsMassPerCubicInch-1.0.1");
    public static UnitType PoundsMassPerFoot => new UnitType("autodesk.unit.unit:poundsMassPerFoot-1.0.1");
    public static UnitType PoundsMassPerFootHour => new UnitType("autodesk.unit.unit:poundsMassPerFootHour-1.0.1");
    public static UnitType PoundsMassPerFootSecond => new UnitType("autodesk.unit.unit:poundsMassPerFootSecond-1.0.1");
    public static UnitType PoundsMassPerHour => new UnitType("autodesk.unit.unit:poundsMassPerHour-1.0.0");
    public static UnitType PoundsMassPerMinute => new UnitType("autodesk.unit.unit:poundsMassPerMinute-1.0.0");
    public static UnitType PoundsMassPerPoundDegreeFahrenheit => new UnitType("autodesk.unit.unit:poundsMassPerPoundDegreeFahrenheit-1.0.0");
    public static UnitType PoundsMassPerSecond => new UnitType("autodesk.unit.unit:poundsMassPerSecond-1.0.0");
    public static UnitType PoundsMassPerSquareFoot => new UnitType("autodesk.unit.unit:poundsMassPerSquareFoot-1.0.1");
    public static UnitType Radians => new UnitType("autodesk.unit.unit:radians-1.0.0");
    public static UnitType RadiansPerSecond => new UnitType("autodesk.unit.unit:radiansPerSecond-1.0.1");
    public static UnitType Rankine => new UnitType("autodesk.unit.unit:rankine-1.0.1");
    public static UnitType RankineInterval => new UnitType("autodesk.unit.unit:rankineInterval-1.0.1");
    public static UnitType RatioTo1 => new UnitType("autodesk.unit.unit:ratioTo1-1.0.0");
    public static UnitType RatioTo10 => new UnitType("autodesk.unit.unit:ratioTo10-1.0.1");
    public static UnitType RatioTo12 => new UnitType("autodesk.unit.unit:ratioTo12-1.0.1");
    public static UnitType RevolutionsPerMinute => new UnitType("autodesk.unit.unit:revolutionsPerMinute-1.0.0");
    public static UnitType RevolutionsPerSecond => new UnitType("autodesk.unit.unit:revolutionsPerSecond-1.0.0");
    public static UnitType RiseDividedBy1000Millimeters => new UnitType("autodesk.unit.unit:riseDividedBy1000Millimeters-1.0.1");
    public static UnitType RiseDividedBy10Feet => new UnitType("autodesk.unit.unit:riseDividedBy10Feet-1.0.1");
    public static UnitType RiseDividedBy120Inches => new UnitType("autodesk.unit.unit:riseDividedBy120Inches-1.0.1");
    public static UnitType RiseDividedBy12Inches => new UnitType("autodesk.unit.unit:riseDividedBy12Inches-1.0.1");
    public static UnitType RiseDividedBy1Foot => new UnitType("autodesk.unit.unit:riseDividedBy1Foot-1.0.1");
    public static UnitType Seconds => new UnitType("autodesk.unit.unit:seconds-1.0.0");
    public static UnitType SlopeDegrees => new UnitType("autodesk.unit.unit:slopeDegrees-1.0.0");
    public static UnitType SquareCentimeters => new UnitType("autodesk.unit.unit:squareCentimeters-1.0.1");
    public static UnitType SquareCentimetersPerMeter => new UnitType("autodesk.unit.unit:squareCentimetersPerMeter-1.0.1");
    public static UnitType SquareFeet => new UnitType("autodesk.unit.unit:squareFeet-1.0.1");
    public static UnitType SquareFeetPer1000BritishThermalUnitsPerHour => new UnitType("autodesk.unit.unit:squareFeetPer1000BritishThermalUnitsPerHour-1.0.1");
    public static UnitType SquareFeetPerFoot => new UnitType("autodesk.unit.unit:squareFeetPerFoot-1.0.1");
    public static UnitType SquareFeetPerKip => new UnitType("autodesk.unit.unit:squareFeetPerKip-1.0.1");
    public static UnitType SquareFeetPerSecond => new UnitType("autodesk.unit.unit:squareFeetPerSecond-1.0.0");
    public static UnitType SquareFeetPerTonOfRefrigeration => new UnitType("autodesk.unit.unit:squareFeetPerTonOfRefrigeration-1.0.1");
    public static UnitType SquareHectometers => new UnitType("autodesk.unit.unit:squareHectometers-1.0.1");
    public static UnitType SquareInches => new UnitType("autodesk.unit.unit:squareInches-1.0.1");
    public static UnitType SquareInchesPerFoot => new UnitType("autodesk.unit.unit:squareInchesPerFoot-1.0.1");
    public static UnitType SquareMeterKelvinsPerWatt => new UnitType("autodesk.unit.unit:squareMeterKelvinsPerWatt-1.0.1");
    public static UnitType SquareMeters => new UnitType("autodesk.unit.unit:squareMeters-1.0.1");
    public static UnitType SquareMetersPerKilonewton => new UnitType("autodesk.unit.unit:squareMetersPerKilonewton-1.0.1");
    public static UnitType SquareMetersPerKilowatt => new UnitType("autodesk.unit.unit:squareMetersPerKilowatt-1.0.1");
    public static UnitType SquareMetersPerMeter => new UnitType("autodesk.unit.unit:squareMetersPerMeter-1.0.1");
    public static UnitType SquareMetersPerSecond => new UnitType("autodesk.unit.unit:squareMetersPerSecond-1.0.0");
    public static UnitType SquareMillimeters => new UnitType("autodesk.unit.unit:squareMillimeters-1.0.1");
    public static UnitType SquareMillimetersPerMeter => new UnitType("autodesk.unit.unit:squareMillimetersPerMeter-1.0.1");
    public static UnitType SquareYards => new UnitType("autodesk.unit.unit:squareYards-1.0.1");
    public static UnitType StandardGravity => new UnitType("autodesk.unit.unit:standardGravity-1.0.1");
    public static UnitType StationingFeet => new UnitType("autodesk.unit.unit:stationingFeet-1.0.0");
    public static UnitType StationingMeters => new UnitType("autodesk.unit.unit:stationingMeters-1.0.0");
    public static UnitType StationingSurveyFeet => new UnitType("autodesk.unit.unit:stationingSurveyFeet-1.0.0");
    public static UnitType Steradians => new UnitType("autodesk.unit.unit:steradians-1.0.0");
    public static UnitType Therms => new UnitType("autodesk.unit.unit:therms-1.0.1");
    public static UnitType ThousandBritishThermalUnitsPerHour => new UnitType("autodesk.unit.unit:thousandBritishThermalUnitsPerHour-1.0.0");
    public static UnitType TonneForceMeters => new UnitType("autodesk.unit.unit:tonneForceMeters-1.0.1");
    public static UnitType TonneForceMetersPerMeter => new UnitType("autodesk.unit.unit:tonneForceMetersPerMeter-1.0.1");
    public static UnitType Tonnes => new UnitType("autodesk.unit.unit:tonnes-1.0.1");
    public static UnitType TonnesForce => new UnitType("autodesk.unit.unit:tonnesForce-1.0.1");
    public static UnitType TonnesForcePerMeter => new UnitType("autodesk.unit.unit:tonnesForcePerMeter-1.0.1");
    public static UnitType TonnesForcePerSquareMeter => new UnitType("autodesk.unit.unit:tonnesForcePerSquareMeter-1.0.1");
    public static UnitType TonsOfRefrigeration => new UnitType("autodesk.unit.unit:tonsOfRefrigeration-1.0.1");
    public static UnitType Turns => new UnitType("autodesk.unit.unit:turns-1.0.1");
    public static UnitType UsGallons => new UnitType("autodesk.unit.unit:usGallons-1.0.1");
    public static UnitType UsGallonsPerHour => new UnitType("autodesk.unit.unit:usGallonsPerHour-1.0.1");
    public static UnitType UsGallonsPerMinute => new UnitType("autodesk.unit.unit:usGallonsPerMinute-1.0.1");
    public static UnitType UsSurveyFeet => new UnitType("autodesk.unit.unit:usSurveyFeet-1.0.0");
    public static UnitType UsTonnesForce => new UnitType("autodesk.unit.unit:usTonnesForce-1.0.1");
    public static UnitType UsTonnesMass => new UnitType("autodesk.unit.unit:usTonnesMass-1.0.1");
    public static UnitType VoltAmperes => new UnitType("autodesk.unit.unit:voltAmperes-1.0.1");
    public static UnitType Volts => new UnitType("autodesk.unit.unit:volts-1.0.1");
    public static UnitType WaterDensity4DegreesCelsius => new UnitType("autodesk.unit.unit:waterDensity4DegreesCelsius-1.0.0");
    public static UnitType Watts => new UnitType("autodesk.unit.unit:watts-1.0.1");
    public static UnitType WattsPerCubicFoot => new UnitType("autodesk.unit.unit:wattsPerCubicFoot-1.0.1");
    public static UnitType WattsPerCubicFootPerMinute => new UnitType("autodesk.unit.unit:wattsPerCubicFootPerMinute-1.0.0");
    public static UnitType WattsPerCubicMeter => new UnitType("autodesk.unit.unit:wattsPerCubicMeter-1.0.1");
    public static UnitType WattsPerCubicMeterPerSecond => new UnitType("autodesk.unit.unit:wattsPerCubicMeterPerSecond-1.0.0");
    public static UnitType WattsPerFoot => new UnitType("autodesk.unit.unit:wattsPerFoot-1.0.0");
    public static UnitType WattsPerMeter => new UnitType("autodesk.unit.unit:wattsPerMeter-1.0.0");
    public static UnitType WattsPerMeterKelvin => new UnitType("autodesk.unit.unit:wattsPerMeterKelvin-1.0.1");
    public static UnitType WattsPerSquareFoot => new UnitType("autodesk.unit.unit:wattsPerSquareFoot-1.0.1");
    public static UnitType WattsPerSquareMeter => new UnitType("autodesk.unit.unit:wattsPerSquareMeter-1.0.1");
    public static UnitType WattsPerSquareMeterKelvin => new UnitType("autodesk.unit.unit:wattsPerSquareMeterKelvin-1.0.1");
    public static UnitType Yards => new UnitType("autodesk.unit.unit:yards-1.0.1");
  }
}
