namespace Playground.Mindustry.Maps;

public class WeatherEntry
{
    /** The type of weather used. */
    public string weather; // TODO: Check type
    /** Minimum and maximum spacing between weather events. Does not include the time of the event itself. */
    public float minFrequency, maxFrequency, minDuration, maxDuration;
    /** Cooldown time before the next weather event takes place This is *state*, not configuration. */
    public float cooldown;
    /** Intensity of the weather produced. */
    public float intensity = 1f;
    /** If true, this weather is always active. */
    public bool always = false;
}