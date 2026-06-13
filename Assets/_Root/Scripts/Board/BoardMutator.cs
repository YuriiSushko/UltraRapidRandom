public class BoardMutator
{
    public void Apply(BoardController board, BoardMutationData mutationData)
    {
        if (board == null || mutationData == null || !mutationData.HasMutations)
        {
            return;
        }

        for (int i = 0; i < mutationData.Mutations.Count; i++)
        {
            board.ApplyMutation(mutationData.Mutations[i]);
        }
    }
}
