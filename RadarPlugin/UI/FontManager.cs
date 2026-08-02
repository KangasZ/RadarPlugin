using System.Collections.Generic;
using Dalamud;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace RadarPlugin.UI;

public class FontManager
{
    private readonly IDataManager dataManager;
    public IFontHandle Axis = null!;
    public IFontHandle RegularFont = null!;
    private ushort[] Ranges = [];
    private ushort[] JpRange = [];
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly Configuration.Configuration config;
    private readonly IPluginLog pluginLog;
    private DalamudAssetFontAndFamilyId baseFont;

    public static readonly HashSet<float> AxisFontSizeList =
    [
        9.6f,
        10f,
        12f,
        14f,
        16f,
        18f,
        18.4f,
        20f,
        23f,
        34f,
        36f,
        40f,
        45f,
        46f,
        68f,
        90f,
    ];

    public FontManager(
        IDalamudPluginInterface pluginInterface,
        IDataManager dataManager,
        Configuration.Configuration configuration,
        IPluginLog pluginLog
    )
    {
        this.config = configuration;
        this.pluginInterface = pluginInterface;
        this.dataManager = dataManager;
        this.pluginLog = pluginLog;
        baseFont = new DalamudAssetFontAndFamilyId(DalamudAsset.NotoSansCjkRegular);
    }

    private unsafe void SetUpRanges()
    {
        ushort[] BuildRange(IReadOnlyList<ushort>? chars, params nint[] ranges)
        {
            var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder());
            // text
            foreach (var range in ranges)
                builder.AddRanges((ushort*)range);

            // chars
            if (chars != null)
            {
                for (var i = 0; i < chars.Count; i += 2)
                {
                    if (chars[i] == 0)
                        break;

                    for (var j = (uint)chars[i]; j <= chars[i + 1]; j++)
                        builder.AddChar((ushort)j);
                }
            }

            // Ingame supported ranges
            var reader = new FdtReader(dataManager.GetFile("common/font/axis_12.fdt")!.Data);
            foreach (var c in reader.Glyphs)
                builder.AddChar(c.Char);

            // various symbols
            // French
            // Romanian
            builder.AddText(
                "←→↑↓《》■※☀★★☆♥♡ヅツッシ☀☁☂℃℉°♀♂♠♣♦♣♧®©™€$£♯♭♪✓√◎◆◇♦■□〇●△▽▼▲‹›≤≥<«“”─＼～"
            );
            builder.AddText("Œœ");
            builder.AddText("ĂăÂâÎîȘșȚț");

            // "Enclosed Alphanumerics" (partial) https://www.compart.com/en/unicode/block/U+2460
            for (var i = 0x2460; i <= 0x24B5; i++)
                builder.AddChar((char)i);

            builder.AddChar('⓪');

            return builder.BuildRangesToArray();
        }
    }

    public unsafe void BuildFonts()
    {
        SetUpRanges();
        Axis = pluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(
            new GameFontStyle(GameFontFamily.Axis, config.cfg.FontSettings.FontSize)
        );

        RegularFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(e =>
            e.OnPreBuild(tk =>
            {
                var config = new SafeFontConfig { SizePx = GetFontSize(), GlyphRanges = Ranges };
                config.MergeFont = baseFont.AddToBuildToolkit(tk, config);

                config.SizePx = GetFontSize();
                config.SizePt = SizeInPt(GetFontSize());
                config.GlyphRanges = JpRange;
                //Plugin.Config.JapaneseFontV2.FontId.AddToBuildToolkit(tk, config);

                //config.SizePt = Plugin.Config.SymbolsFontSizeV2;
                tk.AddGameSymbol(config);

                tk.Font = config.MergeFont;
            })
        );
    }

    public void RebuildFonts()
    {
        BuildFonts();
    }

    public static float SizeInPt(float px) => (float)(px * 3.0 / 4.0);

    public static float SizeInPx(float pt) => (float)(pt * 4.0 / 3.0);

    public float GetFontSize() => this.config.cfg.FontSettings.FontSize;
}
