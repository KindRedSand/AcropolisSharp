using Playground.Mindustry.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Playground.Mindustry.Maps;

public class Rules
{
    /** Sandbox mode: Enables infinite resources, build range and build speed. */
    public bool infiniteResources;
    /** Team-specific rules. */
    public string teams = "";
    /** Whether the waves come automatically on a timer. If not, waves come when the play button is pressed. */
    public bool waveTimer = true;
    /** Whether the waves can be manually summoned with the play button. */
    public bool waveSending = true;
    /** Whether waves are spawnable at all. */
    public bool waves;
    /** Whether the game objective is PvP. Note that this enables automatic hosting. */
    public bool pvp;
    /** Whether is waiting for players enabled in PvP. */
    public bool pvpAutoPause = true;
    /** Whether to pause the wave timer until all enemies are destroyed. */
    public bool waitEnemies = false;
    /** Determines if gamemode is attack mode. */
    public bool attackMode = false;
    /** Whether this is the editor gamemode. */
    public bool editor = false;
    /** Whether blocks can be repaired by clicking them. */
    public bool derelictRepair = true;
    /** Whether a gameover can happen at all. Set this to false to implement custom gameover conditions. */
    public bool canGameOver = true;
    /** Whether cores change teams when they are destroyed. */
    public bool coreCapture = false;
    /** Whether reactors can explode and damage other blocks. */
    public bool reactorExplosions = true;
    /** Whether to allow manual unit control. */
    public bool possessionAllowed = true;
    /** Whether schematics are allowed. */
    public bool schematicsAllowed = true;
    /** Whether friendly explosions can occur and set fire/damage other blocks. */
    public bool damageExplosions = true;
    /** Whether fire (and neoplasm spread) is enabled. */
    public bool fire = true;
    /** Whether units use and require ammo. */
    public bool unitAmmo = false;
    /** EXPERIMENTAL! If true, blocks will update in units and share power. */
    public bool unitPayloadUpdate = false;
    /** Whether cores add to unit limit */
    public bool unitCapVariable = true;
    /** If true, unit spawn points are shown. */
    public bool showSpawns = false;
    /** Multiplies power output of solar panels. */
    public float solarMultiplier = 1f;
    /** How fast unit factories build units. */
    public float unitBuildSpeedMultiplier = 1f;
    /** Multiplier of resources that units take to build. */
    public float unitCostMultiplier = 1f;
    /** How much damage units deal. */
    public float unitDamageMultiplier = 1f;
    /** How much health units start with. */
    public float unitHealthMultiplier = 1f;
    /** How much damage unit crash damage deals. (Compounds with unitDamageMultiplier) */
    public float unitCrashDamageMultiplier = 1f;
    /** If true, ghost blocks will appear upon destruction, letting builder blocks/units rebuild them. */
    public bool ghostBlocks = true;
    /** Whether to allow units to build with logic. */
    public bool logicUnitBuild = true;
    /** If true, world processors no longer update. Used for testing. */
    public bool disableWorldProcessors = false;
    /** How much health blocks start with. */
    public float blockHealthMultiplier = 1f;
    /** How much damage blocks (turrets) deal. */
    public float blockDamageMultiplier = 1f;
    /** Multiplier for buildings resource cost. */
    public float buildCostMultiplier = 1f;
    /** Multiplier for building speed. */
    public float buildSpeedMultiplier = 1f;
    /** Multiplier for percentage of materials refunded when deconstructing. */
    public float deconstructRefundMultiplier = 0.5f;
    /** No-build zone around enemy core radius. */
    public float enemyCoreBuildRadius = 400f;
    /** If true, no-build zones are calculated based on the closest core. */
    public bool polygonCoreProtection = false;
    /** If true, blocks cannot be placed near blocks that are near the enemy team.*/
    public bool placeRangeCheck = false;
    /** If true, dead teams in PvP automatically have their blocks & units converted to derelict upon death. */
    public bool cleanupDeadTeams = true;
    /** If true, items can only be deposited in the core. */
    public bool onlyDepositCore = false;
    /** If true, every enemy block in the radius of the (enemy) core is destroyed upon death. Used for campaign maps. */
    public bool coreDestroyClear = false;
    /** If true, banned blocks are hidden from the build menu. */
    public bool hideBannedBlocks = false;
    /** If true, most blocks (including environmental walls) can be deconstructed. This is only meant to be used internally in sandbox/test maps. */
    public bool allowEnvironmentDeconstruct = false;
    /** If true, buildings will be constructed instantly, with no limit on blocks placed per second. This is highly experimental and may cause lag! */
    public bool instantBuild = false;
    /** If true, bannedBlocks becomes a whitelist. */
    public bool blockWhitelist = false;
    /** If true, bannedUnits becomes a whitelist. */
    public bool unitWhitelist = false;
    /** Radius around enemy wave drop zones.*/
    public float dropZoneRadius = 300f;
    /** Time between waves in ticks. */
    public float waveSpacing = 2 * 60;
    /** Starting wave spacing; if <=0, uses waveSpacing * 2. */
    public float initialWaveSpacing = 0f;
    /** Wave after which the player 'wins'. Use a value <= 0 to disable. */
    public int winWave = 0;
    /** Base unit cap. Can still be increased by blocks. */
    public int unitCap = 0;
    /** Environment drag multiplier. */
    public float dragMultiplier = 1f;
    /** Environmental flags that dictate visuals & how blocks function. */
    public Env env = Env.terrestrial | Env.spores | Env.groundOil | Env.groundWater | Env.oxygen;
    /** Attributes of the environment. */
    public string attributes = "";

    /** Spawn layout. */
    public List<string> spawns = new ();
    /** Starting items put in cores. */
    public List<ItemStack> loadout = new (){new ItemStack(Items.items["copper"], 100)};
    /** Weather events that occur here. */
    public List<WeatherEntry> weather = new ();
    /** Blocks that cannot be placed. */
    public List<Block> bannedBlocks = new ();
    /** Units that cannot be built. */
    public List<string> bannedUnits = new ();//TODO: Check type
    /** Reveals blocks normally hidden by build visibility. */
    public List<Block> revealedBlocks = new ();
    /** Unlocked content names. Only used in multiplayer when the campaign is enabled. */
    // public List<string> researched = new ();
    /** Block containing these items as requirements are hidden. */
    public List<string> hiddenBuildItems = Items.erekirOnlyItems;
    /** In-map objective executor. */
    //public MapObjectives objectives = new MapObjectives();
    /** Flags set by objectives. Used in world processors. */
    public List<string> objectiveFlags = new ();
    /** If true, fog of war is enabled. Enemy units and buildings are hidden unless in radar view. */
    public bool fog = false;
    /** If fog = true, this is whether static (black) fog is enabled. */
    public bool staticFog = true;
    /** Color for static, undiscovered fog of war areas. */
    public Color staticColor = new Color(new Argb32(0f, 0f, 0f, 1f));
    /** Color for discovered but un-monitored fog of war areas. */
    public Color dynamicColor = new Color(new Argb32(0f, 0f, 0f, 0.5f));
    /** Whether ambient lighting is enabled. */
    public bool lighting = false;
    /** Ambient light color, used when lighting is enabled. */
    public Color ambientLight = new Color(new Argb32(0.01f, 0.01f, 0.04f, 0.99f));
    /** team of the player by default. */
    public Team defaultTeam = Team.sharded;
    /** team of the enemy in waves/sectors. */
    public Team waveTeam = Team.crux;
    /** color of clouds that is displayed when the player is landing */
    public Color cloudColor = new Color(new Argb32(0f, 0f,0f,0f));
    /** name of the custom mode that this ruleset describes, or null. */
    public string modeName;
    /** Mission string displayed instead of wave/core counter. Null to disable. */
    public string mission;
    /** Whether cores incinerate items when full, just like in the campaign. */
    public bool coreIncinerates = false;
    /** If false, borders fade out into darkness. Only use with custom backgrounds!*/
    public bool borderDarkness = true;
    /** If true, the map play area is cropped based on the rectangle below. */
    public bool limitMapArea = false;
    /** Map area limit rectangle. */
    public int limitX, limitY, limitWidth = 1, limitHeight = 1;
    /** If true, blocks outside the map area are disabled. */
    public bool disableOutsideArea = true;
    /** special tags for additional info. */
    public Dictionary<string, string> tags = new Dictionary<string, string>();
    /** Name of callback to call for background rendering in mods; see Renderer#addCustomBackground. Runs last. */
    public string? customBackgroundCallback;
    /** path to background texture with extension (e.g. "sprites/space.png")*/
    public string? backgroundTexture;
    /** background texture move speed scaling - bigger numbers mean slower movement. 0 to disable. */
    public float backgroundSpeed = 27000f;
    /** background texture scaling factor */
    public float backgroundScl = 1f;
    /** background UV offsets */
    public float backgroundOffsetX = 0.1f, backgroundOffsetY = 0.1f;

    /** Rules from this planet are applied. If it's {@code sun}, mixed tech is enabled. */
    public Planets planet = Planets.serpulo;

    /** Copies this ruleset exactly. Not efficient at all, do not use often. */
    // public Rules copy(){
    //     return JsonIO.copy(this);
    // }

    /** Returns the gamemode that best fits these rules. */
    public Gamemode mode(){
        if(pvp){
            return Gamemode.pvp;
        }else if(editor){
            return Gamemode.editor;
        }else if(attackMode){
            return Gamemode.attack;
        }else if(infiniteResources){
            return Gamemode.sandbox;
        }else{
            return Gamemode.survival;
        }
    }

    public bool hasEnv(Env env){
        return (this.env & env) != 0;
    }
}

public enum Gamemode
{
    pvp, editor, attack, sandbox, survival,
}

public enum Planets {
    serpulo, erekir
}

public enum Team
{
    derelict, sharded, crux, malis,
    green, blue, neoplastic
}

[Flags]
public enum Env
{
    //is on a planet
    terrestrial = 1,
    //is in space, no atmosphere
    space = 1 << 1,
    //is underwater, on a planet
    underwater = 1 << 2,
    //has a spores
    spores = 1 << 3,
    //has a scorching env effect
    scorching = 1 << 4,
    //has oil reservoirs
    groundOil = 1 << 5,
    //has water reservoirs
    groundWater = 1 << 6,
    //has oxygen in the atmosphere
    oxygen = 1 << 7,
    //all attributes combined, only used for bitmasking purposes
    any = terrestrial | space | underwater | spores | scorching | groundOil | groundWater | oxygen,
    //no attributes (0)
    none = 0,
}