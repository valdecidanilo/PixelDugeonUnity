using System;

namespace Utils
{
    public class Checklist
    {
        public int RequiredSteps { get; private set; }
        public int CurrentSteps { get; private set; }

        public float PercentDone => (float)CurrentSteps / RequiredSteps;
        public bool IsDone => CurrentSteps >= RequiredSteps;
        public event Action<float> OnProgress;
        public event Action OnCompleted;

        public Checklist(int stepsNeeded)
        {
            CurrentSteps = 0;
            RequiredSteps = stepsNeeded;
        }

        public void AddStep(int amount = 1) => RequiredSteps += amount;

        public void FinishStep() => FinishSteps();

        public void FinishSteps(int stepAmount = 1)
        {
            CurrentSteps += stepAmount;
            CurrentSteps = Math.Min(CurrentSteps, RequiredSteps);
            OnProgress?.Invoke(PercentDone);
            if (IsDone) OnCompleted?.Invoke();
        }

        public void ResetChecklist() => CurrentSteps = 0;
    }
}