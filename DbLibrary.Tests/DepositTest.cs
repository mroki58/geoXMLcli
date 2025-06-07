using Xunit;
using DbLibrary;

public class DepositTests
{
    [Fact]
    public void CreateDepositByXml_ParsesCorrectly()
    {
        // Arrange
        string xml = @"
        <deposit name=""TestZłoże"">
            <geology>
                <type>Złoto</type>
                <estimatedVolume>1234.5</estimatedVolume>
                <depth>50.5</depth>
                <status>Eksploatacja</status>
            </geology>
            <geography>
                <location>Złota Góra</location>
                <region>Dolny Śląsk</region>
                <latitude>51.100000</latitude>
                <longitude>16.200000</longitude>
                <radius>10.0</radius>
            </geography>
        </deposit>";

        // Act
        var deposit = Deposit.createDepositByXml(xml);

        // Assert
        Assert.Equal("TestZłoże", deposit.name);
        Assert.Equal("Złoto", deposit.geology.Type);
        Assert.Equal(1234.5, deposit.geology.EstimatedVolume);
        Assert.Equal(50.5, deposit.geology.Depth);
        Assert.Equal("Eksploatacja", deposit.geology.Status);
        Assert.Equal("Złota Góra", deposit.geography.Location);
        Assert.Equal("Dolny Śląsk", deposit.geography.Region);
        Assert.Equal(51.1, deposit.geography.Latitude);
        Assert.Equal(16.2, deposit.geography.Longitude);
        Assert.Equal(10.0, deposit.geography.Radius);
    }
}