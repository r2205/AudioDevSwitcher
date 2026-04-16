using AudioDevSwitcher.Core.Models;
using Xunit;

namespace AudioDevSwitcher.Core.Tests.Models;

public class AudioDeviceTypeTests
{
    [Fact]
    public void AudioDeviceType_HasExpectedValues()
    {
        Assert.Equal(0, (int)AudioDeviceType.Output);
        Assert.Equal(1, (int)AudioDeviceType.Input);
    }

    [Fact]
    public void DefaultDeviceChangedEventArgs_PropertiesWork()
    {
        var args = new DefaultDeviceChangedEventArgs
        {
            DeviceId = "test-id",
            DeviceType = AudioDeviceType.Output,
        };

        Assert.Equal("test-id", args.DeviceId);
        Assert.Equal(AudioDeviceType.Output, args.DeviceType);
    }

    [Fact]
    public void DeviceStateChangedEventArgs_PropertiesWork()
    {
        var args = new DeviceStateChangedEventArgs
        {
            DeviceId = "test-id",
            IsActive = true,
        };

        Assert.Equal("test-id", args.DeviceId);
        Assert.True(args.IsActive);
    }
}
