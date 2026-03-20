using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Automation.Tests;

public class AutomationLevelConfigTests
{
    [Fact]
    public void Skapa_SkaparKonfig()
    {
        var kategoriId = AutomationCategoryId.New();
        var config = AutomationLevelConfig.Skapa(kategoriId, AutomationLevel.Notify);

        Assert.Equal(kategoriId, config.KategoriId);
        Assert.Equal(AutomationLevel.Notify, config.ValdNiva);
    }

    [Fact]
    public void AndraNiva_OverMinimum_Lyckas()
    {
        var config = AutomationLevelConfig.Skapa(AutomationCategoryId.New(), AutomationLevel.Notify);

        config.AndraNiva(AutomationLevel.Suggest, AutomationLevel.Notify);

        Assert.Equal(AutomationLevel.Suggest, config.ValdNiva);
    }

    [Fact]
    public void AndraNiva_UnderMinimum_KastarException()
    {
        var config = AutomationLevelConfig.Skapa(AutomationCategoryId.New(), AutomationLevel.Suggest);

        Assert.Throws<InvalidOperationException>(() =>
            config.AndraNiva(AutomationLevel.Notify, AutomationLevel.Suggest));
    }

    [Fact]
    public void AndraNiva_LikaMinimum_Lyckas()
    {
        var config = AutomationLevelConfig.Skapa(AutomationCategoryId.New(), AutomationLevel.Autopilot);

        config.AndraNiva(AutomationLevel.Autopilot, AutomationLevel.Autopilot);

        Assert.Equal(AutomationLevel.Autopilot, config.ValdNiva);
    }

    [Fact]
    public void AndraNiva_FranBlockTillNotify_MedBlockMinimum_KastarException()
    {
        var config = AutomationLevelConfig.Skapa(AutomationCategoryId.New(), AutomationLevel.Block);

        Assert.Throws<InvalidOperationException>(() =>
            config.AndraNiva(AutomationLevel.Notify, AutomationLevel.Block));
    }
}
