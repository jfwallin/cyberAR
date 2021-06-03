namespace MCQ
{
    public interface IMCQManager
    {
        void Initialize(MCExerciseData initData, MediaPlayer player);
        void OnAnswerDeselected(int answerID);
        void OnAnswerSelected(int answerID);
    }
}