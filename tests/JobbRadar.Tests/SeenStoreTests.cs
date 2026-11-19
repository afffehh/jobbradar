using JobbRadar.Services;

namespace JobbRadar.Tests;

public class SeenStoreTests
{
    private static string TempFile() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

    [Fact]
    public void ReturnsEmptyWhenFileIsMissing() =>
        Assert.Empty(new SeenStore(TempFile()).Load());

    [Fact]
    public void SavesAndLoadsIdsWithoutDuplicates()
    {
        var path = TempFile();
        var store = new SeenStore(path);

        store.Save(["2", "1", "2"]);

        Assert.Equal(["1", "2"], store.Load().Order());
        File.Delete(path);
    }
}