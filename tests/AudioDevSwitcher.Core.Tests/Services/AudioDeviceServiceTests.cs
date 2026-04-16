using AudioDevSwitcher.Core.Models;
using AudioDevSwitcher.Core.Services;
using NSubstitute;
using Xunit;

namespace AudioDevSwitcher.Core.Tests.Services;

public class AudioDeviceServiceTests
{
    [Fact]
    public void CycleDevice_WithMockService_ReturnsNextDevice()
    {
        // Arrange
        var service = Substitute.For<IAudioDeviceService>();
        var devices = new List<AudioDevice>
        {
            new() { Id = "dev1", Name = "Speakers", Type = AudioDeviceType.Output, IsDefault = true, IsActive = true },
            new() { Id = "dev2", Name = "Headphones", Type = AudioDeviceType.Output, IsDefault = false, IsActive = true },
        };
        service.GetDevices(AudioDeviceType.Output).Returns(devices);
        service.CycleDevice(AudioDeviceType.Output).Returns(devices[1]);

        // Act
        var result = service.CycleDevice(AudioDeviceType.Output);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dev2", result.Id);
        Assert.Equal("Headphones", result.Name);
    }

    [Fact]
    public void GetDevices_WithMockService_ReturnsBothTypes()
    {
        // Arrange
        var service = Substitute.For<IAudioDeviceService>();
        var outputs = new List<AudioDevice>
        {
            new() { Id = "out1", Name = "Speakers", Type = AudioDeviceType.Output, IsDefault = true, IsActive = true },
        };
        var inputs = new List<AudioDevice>
        {
            new() { Id = "in1", Name = "Microphone", Type = AudioDeviceType.Input, IsDefault = true, IsActive = true },
        };
        service.GetDevices(AudioDeviceType.Output).Returns(outputs);
        service.GetDevices(AudioDeviceType.Input).Returns(inputs);

        // Act
        var outputResult = service.GetDevices(AudioDeviceType.Output);
        var inputResult = service.GetDevices(AudioDeviceType.Input);

        // Assert
        Assert.Single(outputResult);
        Assert.Single(inputResult);
        Assert.Equal(AudioDeviceType.Output, outputResult[0].Type);
        Assert.Equal(AudioDeviceType.Input, inputResult[0].Type);
    }

    [Fact]
    public void SetDefaultDevice_WithMockService_CallsCorrectId()
    {
        // Arrange
        var service = Substitute.For<IAudioDeviceService>();

        // Act
        service.SetDefaultDevice("dev1");

        // Assert
        service.Received(1).SetDefaultDevice("dev1");
    }

    [Fact]
    public void AudioDevice_ToString_ShowsNameAndType()
    {
        var device = new AudioDevice
        {
            Id = "test",
            Name = "Speakers",
            Type = AudioDeviceType.Output,
            IsDefault = true,
            IsActive = true,
        };

        Assert.Equal("Speakers (Output, Default)", device.ToString());
    }

    [Fact]
    public void AudioDevice_ToString_OmitsDefaultWhenNotDefault()
    {
        var device = new AudioDevice
        {
            Id = "test",
            Name = "Headphones",
            Type = AudioDeviceType.Output,
            IsDefault = false,
            IsActive = true,
        };

        Assert.Equal("Headphones (Output)", device.ToString());
    }
}
