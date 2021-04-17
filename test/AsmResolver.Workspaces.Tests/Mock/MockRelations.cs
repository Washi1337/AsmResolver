using System;

namespace AsmResolver.Workspaces.Tests.Mock
{
    public static class MockRelations
    {
        public static readonly ObjectRelation<object, object> Relation1 = new(
            "Relation1",
            new Guid("1562D293-8CE6-4525-B3BF-6AB634393687"));

        public static readonly ObjectRelation<object, object> Relation2 = new(
            "Relation2",
            new Guid("4B30626F-4B14-4565-A25B-4D93DBB351E1"));

        public static readonly ObjectRelation<object, object> Relation3 = new(
            "Relation3",
            new Guid("08062a53-4fb1-4f27-98cb-62cdce05d752"));

        public static readonly ObjectRelation<object, object> Relation4 = new(
            "Relation4",
            new Guid("3130d32a-bb54-49ec-b630-5cd5ff5ee5a3"));

        public static readonly ObjectRelation<object, string> Relation5 = new(
            "Relation5",
            new Guid("804ffd37-0259-4c19-9e3a-cbeae7cb5ff9"));
    }
}
