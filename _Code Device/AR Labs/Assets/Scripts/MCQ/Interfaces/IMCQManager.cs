namespace MCQ
{
    public interface IMCQManager
    {
        void Initialize(MCExerciseData initData, AudioPlayer player);
        void OnAnswerDeselected(int answerID);
        void OnAnswerSelected(int answerID);
    }
}