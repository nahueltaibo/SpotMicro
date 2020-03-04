﻿using Iot.Device.Pwm;
using MessageBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Robot.Controllers;
using Robot.Controllers.RemoteControl;
using Robot.Drivers.RemoteControl;
using Robot.Drivers.ServoMotors;
using Spot.Reactive;
using System.Collections.Generic;
using System.Device.I2c;

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

        public static IServiceCollection AddControlLayer(this IServiceCollection services)
        {
            services.Configure<RemoteControlOptions>(options =>
            {
                options.GamepadKeyThrottle = RemoteControlKeys.Throttle;
                options.GamepadKeyYaw = RemoteControlKeys.Yaw;

            });

            // Configure the Remote Control Controller
            services.AddHostedService(s => new RemoteControlController(
                    new GamepadDriver(s.GetService<ILogger<GamepadDriver>>()),
                    s.GetService<IMessageBroker>(),
                    s.GetService<IOptions<RemoteControlOptions>>(),
                    s.GetService<ILogger<RemoteControlController>>()));

            var busId = 1;
            var selectedI2cAddress = 0b000000; // A5 A4 A3 A2 A1 A0
            var deviceAddress = Pca9685.I2cAddressBase + selectedI2cAddress;
            var settings = new I2cConnectionSettings(busId, deviceAddress);
            var device = I2cDevice.Create(settings);
            var pca9685 = new Pca9685(device);
            pca9685.PwmFrequency = 50;

            services.AddHostedService((s) => new ServoController(new List<IServo>
            {
                new ServoDriver(pca9685, 0),
                new ServoDriver(pca9685, 1),
                new ServoDriver(pca9685, 2),
                new ServoDriver(pca9685, 3),
                new ServoDriver(pca9685, 4),
                new ServoDriver(pca9685, 5),
                new ServoDriver(pca9685, 6),
                new ServoDriver(pca9685, 7),
                new ServoDriver(pca9685, 8),
                new ServoDriver(pca9685, 9),
                new ServoDriver(pca9685, 10),
                new ServoDriver(pca9685, 11)
            }, s.GetService<IMessageBroker>(), s.GetService<ILogger<ServoController>>()));

            return services;
        }
    }
}
