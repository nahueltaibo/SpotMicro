using Iot.Device.Pwm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Robot.Controllers.RemoteControl;
using Robot.Drivers.RemoteControl;
using Robot.MessageBus;
using Robot.ServoMotors;
using Spot.Controllers;
using Spot.Reactive;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;

namespace Spot.Start
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<IMessageBroker, MesageBroker>();

            return services;
        }

        public static IServiceCollection AddBehavioralLayer(this IServiceCollection services)
        {

            return services;
        }

        public static IServiceCollection AddReactiveLayer(this IServiceCollection services)
        {
            services.AddHostedService<RemoteControlServoTester>();

            return services;
        }

        public static IServiceCollection AddControlLayer(this IServiceCollection services, HostBuilderContext hostContext)
        {
            // Configure the Remote Control Controller
            services.AddHostedService<RemoteControlController>(s =>
            {
                var gamepadDriver = new GamepadDriver(s.GetService<ILogger<GamepadDriver>>());

                return new RemoteControlController(
                    gamepadDriver,
                    s.GetService<IMessageBroker>(),
                    s.GetService<IOptions<RemoteControlOptions>>(),
                    s.GetService<ILogger<RemoteControlController>>());
            });

            services.AddHostedService<ServoController>((s) =>
            {
                var busId = 1;
                var selectedI2cAddress = 0b000000; // A5 A4 A3 A2 A1 A0
                var deviceAddress = Pca9685.I2cAddressBase + selectedI2cAddress;
                var i2cSettings = new I2cConnectionSettings(busId, deviceAddress);
                var device = I2cDevice.Create(i2cSettings);
                var pca9685 = new Pca9685(device);
                pca9685.PwmFrequency = 50;


                var configs = hostContext.Configuration.GetSection("sensors:sonars");
                var mappedConfigs = configs.Get<IEnumerable<PwmServoMotorDriverSettings>>();
                var sensors = mappedConfigs.Select(settings => new PwmServoMotorDriver(pca9685, settings, s.GetService<ILogger<PwmServoMotorDriver>>())).ToArray();

                return new ServoController(sensors, s.GetService<IMessageBroker>(), s.GetService<ILogger<ServoController>>());
            });

            return services;
        }
    }
}
