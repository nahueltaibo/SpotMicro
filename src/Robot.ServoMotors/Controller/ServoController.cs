using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Robot.MessageBus;
using Robot.Messages;
using Robot.ServoMotors;
using Spot.Model.Posing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spot.Controllers
{
    public class ServoController : IServoController
    {
        private readonly IList<IServoMotor> _servos;
        private readonly PoseSettings _poseSettings;
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<ServoController> _log;
        private CancellationTokenSource _poseRunnerToken;
        private Task _poseRunner;
        private IList<double?> _currentState;

        public ServoController(IList<IServoMotor> servos, PoseSettings poseSettings, IMessageBroker messageBroker, ILogger<ServoController> logger)
        {
            _servos = servos;
            _poseSettings = poseSettings;
            _messageBroker = messageBroker;
            _log = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _messageBroker.Subscribe<ServoMessage>(OnServoChanged);

            _messageBroker.Subscribe<PoseMessage>(OnPoseChanged);

            // We need to asume a starting position, so we start from Sleep
            _currentState = _poseSettings.Poses.First(p => p.Name == PoseNames.Sleep).Values;

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var servo in _servos)
            {
                CancellPoseRunnerIfNeeded();

                try
                {
                    servo.Dispose();
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error disposing servo with ID {servo.Id}", ex);
                }

                _log.LogDebug($"{nameof(ServoController)} Stopped");
            }
            await Task.CompletedTask;
        }

        private void OnServoChanged(IMessage message)
        {
            try
            {
                var servoMessage = (ServoMessage)message;
                _log.LogDebug($"ServoMessage: id={servoMessage.Id}; pulse={servoMessage.PulseWidth}; angle={servoMessage.Angle}");

                var servo = _servos.FirstOrDefault(s => s.Id == servoMessage.Id);

                if (servo == null)
                {
                    _log.LogError($"Invalid servo id ({servoMessage.Id})");
                    return;
                }

                if (servoMessage.PulseWidth.HasValue)
                {
                    servo.SetPulseWidth(servoMessage.PulseWidth.Value);
                }
                else if (servoMessage.Angle.HasValue)
                {
                    servo.SetAngle(servoMessage.Angle.Value);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error processing ServoMessage {JsonConvert.SerializeObject(message)}", ex);
            }
        }

        private void OnPoseChanged(IMessage message)
        {
            var poseMessage = (PoseMessage)message;

            switch (poseMessage.Name)
            {
                case PoseNames.Sit:
                    RunPose(_poseSettings.Poses.First(p => p.Name == PoseNames.Sit));
                    break;
                case PoseNames.Sleep:
                    RunPose(_poseSettings.Poses.First(p => p.Name == PoseNames.Sleep));
                    break;
                case PoseNames.Stand:
                    RunPose(_poseSettings.Poses.First(p => p.Name == PoseNames.Stand));
                    break;
            }
        }

        private void RunPose(Pose pose)
        {
            _log.LogInformation($"Running pose {pose.Name}...");

            // If we were already posing, we need to cancell it first
            CancellPoseRunnerIfNeeded();

            _poseRunnerToken = new CancellationTokenSource();

            _poseRunner = Task.Run(() => { RunPoseTask(pose, _poseRunnerToken.Token); }, _poseRunnerToken.Token);
        }

        private void RunPoseTask(Pose pose, CancellationToken token)
        {
            var states = CalculateStatesToPose(pose);

            foreach (var state in states)
            {
                token.ThrowIfCancellationRequested();

                MoveServosToAngles(state);

                _currentState = state;

                Thread.Sleep(Convert.ToInt32(1000 / _poseSettings.JointRefreshFrequency));
            }
        }

        private IList<IList<double?>> CalculateStatesToPose(Pose pose)
        {
            try
            {
                var states = new List<IList<double?>>();

                // Calculate how many intermediate states should we create considering
                // the servo update frequency and the duration of this pose
                int statesCount = Convert.ToInt32(Math.Ceiling(pose.Duration * _poseSettings.JointRefreshFrequency));

                for (int stateNumber = 0; stateNumber < statesCount; stateNumber++)
                {
                    var state = new List<double?>();

                    // For each joint calculate what the value should be at the current state
                    for (int joint = 0; joint < pose.Values.Count; joint++)
                    {
                        var endState = pose.Values[joint];
                        var startState = _currentState[joint];
                        var delta = endState > startState ? (endState - startState) : -(startState - endState);
                        var deltaPerState = delta / statesCount;

                        // Divide the range of motion we need to do equaly between the number of intermediate states
                        var jointState = startState + (deltaPerState * (stateNumber + 1));

                        state.Add(jointState);
                    }
                    states.Add(state);

                    LogState(state);
                }

                return states;
            }
            catch (Exception ex)
            {
                _log.LogError("Error calculating intermediate states", ex);
                throw;
            }
        }

        private void LogState(List<double?> state)
        {
            var temp = new StringBuilder();

            temp.Append("[");
            for (int i = 0; i < state.Count; i++)
            {
                temp.Append(String.Format("{0}{1}", i == 0 ? "" : ", ", state[i]));
            }
            temp.Append("]");

            _log.LogDebug(temp.ToString());
        }

        private void CancellPoseRunnerIfNeeded()
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
                    if (!ex.Message.Contains("A task was canceled."))
                    {
                        _log.LogWarning($"Error cancelling pose runner task: {ex.Message}", ex);
                    }
                }
            }
        }

        private void MoveServosToAngles(IList<double?> states)
        {
            for (int i = 0; i < states.Count && i < _servos.Count; i++)
            {
                // Only set an angle to a servo if it has a value value
                if (states[i].HasValue)
                {
                    _servos[i].SetAngle(states[i].Value);
                }
            }
        }
    }
}
