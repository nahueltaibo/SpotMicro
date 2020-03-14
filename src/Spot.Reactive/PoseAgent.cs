using Microsoft.Extensions.Logging;
using Robot.MessageBus;
using Robot.MessageBus.Messages;
using Robot.Messages;
using Robot.Model.RemoteControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spot.Reactive
{
    public class PoseAgent : IPoseAgent
    {
        private readonly IList<int> _servoIds;
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<PoseAgent> _logger;
        private bool _isPosing = false;
        private CancellationTokenSource _poseRunnerToken;
        private Task _poseRunner;

        private readonly List<List<double?>> _poses = new List<List<double?>>
        {
            new List<double?> { 0, null, null, null, null, null, null, null, null, null, null, null },
            new List<double?> { -Math.PI / 8, null, null, null, null, null, null, null, null, null, null, null },
            new List<double?> { 0, null, null, null, null, null, null, null, null, null, null, null },
            new List<double?> { Math.PI / 8, null, null, null, null, null, null, null, null, null, null, null },
            new List<double?> { 0, null, null, null, null, null, null, null, null, null, null, null }
        };

        public PoseAgent(IList<int> servoIds, IMessageBroker messageBroker, ILogger<PoseAgent> logger)
        {
            _servoIds = servoIds;
            _messageBroker = messageBroker;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _messageBroker.Subscribe<RemoteControlMessage>(OnRemoteControlMessageReceived);

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private void OnRemoteControlMessageReceived(IMessage message)
        {
            var rcMessage = (RemoteControlMessage)message;

            if (rcMessage.Key == (int)RemoteControlKey.X && rcMessage.Value == 1)
            {
                // If we were already posing, we need to cancell it first
                CancellPoseRunnerIfRequired();

                _poseRunnerToken = new CancellationTokenSource();
                _poseRunner = Task.Run(() => { RunPose(); }, _poseRunnerToken.Token);
            }
            else if (rcMessage.Key == (int)RemoteControlKey.A && rcMessage.Value == 1)
            {
                foreach (var servoId in Enumerable.Range(1, 16))
                {
                    MoveServoToAngle(servoId, -Math.PI / 8);
                }
            }
            else if (rcMessage.Key == (int)RemoteControlKey.B && rcMessage.Value == 1)
            {
                foreach (var servoId in Enumerable.Range(1, 16))
                {
                    MoveServoToAngle(servoId, Math.PI / 8);
                }
            }
        }

        private void RunPose()
        {
            foreach (var list in _poses)
            {
                MoveServosToAngles(list);
                Thread.Sleep(500);
            }
        }

        private void CancellPoseRunnerIfRequired()
        {
            if (_poseRunner != null)
            {
                _poseRunnerToken.Cancel();
                try
                {
                    _poseRunner.Wait();
                }
                catch (TaskCanceledException)
                {
                    // TaskCanceledExeptions is expected, as we are cancelling the Task
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error cancelling pose runner task", ex);
                }
            }
        }

        private void MoveServosToAngles(IList<double?> angles)
        {
            // Limit the loop to the lower of both angles and _servoIds
            for (int i = 0; i < angles.Count && i < _servoIds.Count; i++)
            {
                // Only set an angle to a servo if it has a value value
                if (angles[i].HasValue)
                {
                    MoveServoToAngle(_servoIds[i], angles[i].Value);
                }
            }
        }

        private void MoveServoToAngle(int servoId, double radians)
        {
            _messageBroker.Publish(new ServoMessage
            {
                Id = servoId,
                Angle = radians
            });
        }
    }
}
