using System;

namespace AsmResolver.Workspaces.Tests.Mock
{
    public static class MockRelations
    {
        public static readonly ObjectRelation<object> Relation1 = new(
            "Relation1",
            new Guid("1562D293-8CE6-4525-B3BF-6AB634393687"));

        public static readonly ObjectRelation<object> Relation2 = new(
            "Relation2",
            new Guid("4B30626F-4B14-4565-A25B-4D93DBB351E1"));

    }
}
