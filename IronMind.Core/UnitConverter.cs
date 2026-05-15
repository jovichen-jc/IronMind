namespace IronMind.Core;

public static class UnitConverter
{
    public static float KgToLbs(float kg) => MathF.Round(kg * 2.20462f, 1);
    public static float LbsToKg(float lbs) => MathF.Round(lbs / 2.20462f, 1);
    public static float KmToMiles(float km) => MathF.Round(km * 0.621371f, 2);
    public static float MilesToKm(float miles) => MathF.Round(miles / 0.621371f, 2);
    public static float MlToOz(float ml) => MathF.Round(ml * 0.033814f, 1);
    public static float OzToMl(float oz) => MathF.Round(oz / 0.033814f, 1);
    public static float CmToInches(float cm) => MathF.Round(cm * 0.393701f, 1);
}
