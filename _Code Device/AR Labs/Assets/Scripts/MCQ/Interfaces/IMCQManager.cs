namespace MCQ
{
    public interface IMCQManager
    {
        void Initialize(MCExerciseData initData, MediaPlayer mediaPlayer);
        void OnAnswerDeselected(int answerID);
        void OnAnswerSelected(int answerID);
    }
}