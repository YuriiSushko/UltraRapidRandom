using System.Collections.Generic;

public class BoardMutationData
{
    public static readonly BoardMutationData Empty = new BoardMutationData(
        new List<BoardMutation>()
    );

    public List<BoardMutation> Mutations { get; }
    public bool HasMutations => Mutations.Count > 0;

    public BoardMutationData(List<BoardMutation> mutations)
    {
        Mutations = mutations != null
            ? mutations
            : new List<BoardMutation>();
    }
}
