﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using ScriptDom = Microsoft.SqlServer.TransactSql.ScriptDom;

using LowSums;
using TSqlStrong.TypeSystem;
using TSqlStrong.Symbols;
using TSqlStrongSpecifications.Builders;

namespace TSqlStrongSpecifications
{
#pragma warning disable IDE1006 // Naming Styles
    class describe_DataTypes : NSpec.nspec
    {
        #region descriptions

        public void describe_CanCompareWith()
        {
            context["varchar"] = () =>
            {
                context["with a known set"] = () =>
                {
                    GoodTypeComparison(SqlDataTypeWithKnownSet.VarChar("apples", "oranges"), SqlDataType.VarChar);
                    GoodTypeComparison(SqlDataTypeWithKnownSet.VarChar("apples", "oranges"), SqlDataTypeWithKnownSet.VarChar("apples"));
                    GoodTypeComparison(SqlDataTypeWithKnownSet.VarChar("apples"), SqlDataTypeWithKnownSet.VarChar("apples", "oranges"));

                    BadTypeComparison(SqlDataTypeWithKnownSet.VarChar("apples"), SqlDataTypeWithKnownSet.VarChar("oranges"));
                    BadTypeComparison(SqlDataTypeWithKnownSet.VarChar("apples", "bananas"), SqlDataTypeWithKnownSet.VarChar("oranges", "grapes"));
                };
            };

            context["integers"] = () =>
            {
                context["without a domain"] = () =>
                {
                    GoodTypeComparison(SqlDataType.Int, new SqlDataType(ScriptDom.SqlDataTypeOption.Int));
                    GoodTypeComparison(SqlDataType.Int, SqlDataTypeWithDomain.Int("X"));
                    GoodTypeComparison(SqlDataType.Int, SqlDataTypeWithKnownSet.Int(1, 2));

                    BadTypeComparison(SqlDataType.Int, SqlDataType.VarChar);
                    BadTypeComparison(SqlDataType.Int, new RowDataType());
                };

                context["with a domain"] = () =>
                {
                    GoodTypeComparison(SqlDataTypeWithDomain.Int("X"), SqlDataType.Int);
                    GoodTypeComparison(SqlDataTypeWithDomain.Int("X"), SqlDataTypeWithDomain.Int("X"));

                    BadTypeComparison(SqlDataTypeWithDomain.Int("X"), SqlDataTypeWithDomain.Int("Y"));
                    GoodTypeComparison(SqlDataTypeWithDomain.Int("X"), SqlDataTypeWithKnownSet.Int(1, 2));
                };

                context["with a known set"] = () =>
                {
                    GoodTypeComparison(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataType.Int);
                    GoodTypeComparison(SqlDataTypeWithKnownSet.Int(1), SqlDataTypeWithKnownSet.Int(1, 2), "you can compare two sets so long as one is a subset of the other");
                    GoodTypeComparison(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataTypeWithKnownSet.Int(1), "you can compare two sets so long as one is a subset of the other");

                    GoodTypeComparison(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataTypeWithDomain.Int("X"), "cannot compare a known fixed set with a named domain");
                    BadTypeComparison(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataTypeWithKnownSet.Int(3, 4), "cannot compare two entirely disjoint sets");
                };
            };

            context["ColumDataType"] = () =>
            {
                GoodTypeComparison(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataType.Int), SqlDataType.Int);
                GoodTypeComparison(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataType.VarChar), SqlDataType.VarChar);
                GoodTypeComparison(new ColumnDataType(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), SqlDataType.Int), SqlDataType.Int);
                GoodTypeComparison(new ColumnDataType(new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive), SqlDataType.Int), SqlDataType.Int);
                GoodTypeComparison(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataTypeWithKnownSet.VarChar("x", "y", "z")), SqlDataTypeWithKnownSet.VarChar("x"));

                BadTypeComparison(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataType.VarChar), SqlDataType.Int);
                BadTypeComparison(new ColumnDataType(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), SqlDataType.VarChar), SqlDataType.Int);
                BadTypeComparison(new ColumnDataType(new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive), SqlDataType.VarChar), SqlDataType.Int);
                BadTypeComparison(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataTypeWithKnownSet.VarChar("x", "y", "z")), SqlDataTypeWithKnownSet.VarChar("q"));
            };
        }

        public void describe_IsAssignableTo()
        {
            context["VarChar"] = () =>
            {
                context["without a domain"] = () =>
                {
                    GoodTypeAssignment(SqlDataType.VarChar, SqlDataType.VarChar);
                    GoodTypeAssignment(SqlDataType.VarChar, SqlDataType.NVarChar, "An NVarChar can hold the representation of a VarChar");

                    BadTypeAssignment(SqlDataType.VarChar, SqlDataType.Int);
                };
            };

            context["NVarChar"] = () =>
            {
                context["without a domain"] = () =>
                {
                    GoodTypeAssignment(SqlDataType.NVarChar, SqlDataType.NVarChar);

                    BadTypeAssignment(SqlDataType.NVarChar, SqlDataType.VarChar);
                    BadTypeAssignment(SqlDataType.NVarChar, SqlDataType.Int);
                };
            };

            context["Integers"] = () =>
            {
                context["without a domain"] = () =>
                {
                    GoodTypeAssignment(SqlDataType.Int, new SqlDataType(ScriptDom.SqlDataTypeOption.Int));

                    BadTypeAssignment(SqlDataType.Int, SqlDataType.VarChar);
                    BadTypeAssignment(SqlDataType.Int, SqlDataTypeWithDomain.Int("X"), because: "there is no way to vouch for a domain");
                    BadTypeAssignment(SqlDataType.Int, SqlDataTypeWithKnownSet.Int(1, 2, 3));                    
                    BadTypeAssignment(SqlDataType.Int, new RowDataType());
                };

                context["with a domain"] = () =>
                {
                    GoodTypeAssignment(SqlDataTypeWithDomain.Int("X"), SqlDataType.Int);
                    GoodTypeAssignment(SqlDataTypeWithDomain.Int("X"), SqlDataTypeWithDomain.Int("X"));

                    BadTypeAssignment(SqlDataTypeWithDomain.Int("X"), SqlDataTypeWithKnownSet.Int(1, 2));
                    BadTypeAssignment(SqlDataTypeWithDomain.Int("X"), SqlDataTypeWithDomain.Int("Y"));
                };

                context["with a known set"] = () =>
                {
                    GoodTypeAssignment(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataType.Int);
                    GoodTypeAssignment(SqlDataTypeWithKnownSet.Int(1), SqlDataTypeWithKnownSet.Int(1, 2), because: "you can assign a subset to a superset");

                    BadTypeAssignment(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataTypeWithDomain.Int("X"));
                    BadTypeAssignment(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataTypeWithKnownSet.Int(1), because: "cannot assign a super set to a subset");
                    BadTypeAssignment(SqlDataTypeWithKnownSet.Int(1, 2), SqlDataTypeWithKnownSet.Int(3, 4), because: "cannot assign sets with no common elements");
                };
            };

            context["Nullable<T>"] = () =>
            {
                GoodTypeAssignment(SqlDataType.Int, SqlDataType.Int.ToNullable());
                GoodTypeAssignment(SqlDataTypeWithDomain.Int("x"), SqlDataTypeWithDomain.Int("x").ToNullable());
                BadTypeAssignment(SqlDataType.Int, SqlDataType.VarChar.ToNullable());

                BadTypeAssignment(SqlDataType.Int.ToNullable(), SqlDataType.Int, "A null cannot fit inside of a non-null");
            };

            context["ColumDataType"] = () =>
            {
                GoodTypeAssignment(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataType.Int), SqlDataType.Int);
                GoodTypeAssignment(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataType.VarChar), SqlDataType.VarChar);
                GoodTypeAssignment(new ColumnDataType(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), SqlDataType.Int), SqlDataType.Int);
                GoodTypeAssignment(new ColumnDataType(new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive), SqlDataType.Int), SqlDataType.Int);

                BadTypeAssignment(new ColumnDataType(ColumnDataType.ColumnName.Anonymous.Instance, SqlDataType.VarChar), SqlDataType.Int);
                BadTypeAssignment(new ColumnDataType(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), SqlDataType.VarChar), SqlDataType.Int);
                BadTypeAssignment(new ColumnDataType(new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive), SqlDataType.VarChar), SqlDataType.Int);
            };

            context["RowDataType"] = () =>
            {
                var anon = String.Empty;

                var rowWithIDAndCountSchemaNames = RowBuilder
                    .WithSchemaNamedColumn("id", SqlDataType.Int)
                    .AndSchemaNamedColumn("count", SqlDataType.Int)
                    .CreateRow();

                var rowWithCountAndIDSchemaNames = RowBuilder
                    .WithSchemaNamedColumn("count", SqlDataType.Int)
                    .AndSchemaNamedColumn("id", SqlDataType.Int)
                    .CreateRow();

                var rowWithIDAndCountAliased = RowBuilder
                    .WithAliasedColumn("id", SqlDataType.Int)
                    .AndAliasedColumn("count", SqlDataType.Int)
                    .CreateRow();

                var rowWithCountAndIDAliased = RowBuilder
                    .WithAliasedColumn("count", SqlDataType.Int)
                    .AndAliasedColumn("id", SqlDataType.Int)
                    .CreateRow();

                var rowWithAnonymousIntInt = RowBuilder
                    .WithAnonymousColumn(SqlDataType.Int)
                    .AndAnonymousColumn(SqlDataType.Int)
                    .CreateRow();

                var rowWithAnonymousIntVarChar = RowBuilder
                    .WithAnonymousColumn(SqlDataType.Int)
                    .AndAnonymousColumn(SqlDataType.VarChar)
                    .CreateRow();

                var rowWithAnonymousVarCharInt = RowBuilder
                    .WithAnonymousColumn(SqlDataType.VarChar)
                    .AndAnonymousColumn(SqlDataType.Int)
                    .CreateRow();

                // empty rows
                GoodTypeAssignment(new RowDataType(), new RowDataType());
                GoodTypeAssignment(RowBuilder.EmptyRow, rowWithIDAndCountSchemaNames);
                GoodTypeAssignment(rowWithIDAndCountSchemaNames, RowBuilder.EmptyRow);

                GoodTypeAssignment(rowWithAnonymousIntVarChar, rowWithAnonymousIntVarChar);
                GoodTypeAssignment(rowWithIDAndCountSchemaNames, rowWithAnonymousIntInt);
                GoodTypeAssignment(rowWithAnonymousIntInt, rowWithIDAndCountSchemaNames);
                GoodTypeAssignment(rowWithIDAndCountSchemaNames, rowWithCountAndIDSchemaNames, because: "Schema names do not have to match because a user could be selecting from one schema object into another");

                BadTypeAssignment(rowWithAnonymousVarCharInt, rowWithAnonymousIntVarChar, because: "The types do not align");
                BadTypeAssignment(rowWithAnonymousVarCharInt, rowWithIDAndCountSchemaNames, because: "The types do not align");
                BadTypeAssignment(rowWithAnonymousVarCharInt, rowWithIDAndCountSchemaNames, because: "The types do not align");

                BadTypeAssignment(rowWithIDAndCountAliased, rowWithCountAndIDAliased, because: "Aliases are explicitly applied by author and therefore must match the destination");
                BadTypeAssignment(rowWithIDAndCountAliased, rowWithCountAndIDSchemaNames, because: "Aliases are explicitly applied by author and therefore must match the destination");
                BadTypeAssignment(rowWithCountAndIDAliased, rowWithIDAndCountAliased, because: "Aliases are explicitly applied by author and therefore must match the destination");
                BadTypeAssignment(rowWithCountAndIDAliased, rowWithIDAndCountSchemaNames, because: "Aliases are explicitly applied by author and therefore must match the destination");
            };

            context["ColumnName"] = () =>
            {
                GoodColumnNameAssignment(ColumnDataType.ColumnName.Anonymous.Instance, ColumnDataType.ColumnName.Anonymous.Instance);
                GoodColumnNameAssignment(ColumnDataType.ColumnName.Anonymous.Instance, new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive));
                GoodColumnNameAssignment(ColumnDataType.ColumnName.Anonymous.Instance, new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive));

                GoodColumnNameAssignment(new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive), new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive));
                GoodColumnNameAssignment(new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive), new ColumnDataType.ColumnName.Schema("y", CaseSensitivity.CaseInsensitive), because: "Schema names do not have to match");

                GoodColumnNameAssignment(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive));
                GoodColumnNameAssignment(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), new ColumnDataType.ColumnName.Schema("x", CaseSensitivity.CaseInsensitive));
                GoodColumnNameAssignment(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), ColumnDataType.ColumnName.Anonymous.Instance);
                BadColumnNameAssignment(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), new ColumnDataType.ColumnName.Aliased("y", CaseSensitivity.CaseInsensitive), because: "an alias in the source expressing an _intention_ and so must match the dest");
                BadColumnNameAssignment(new ColumnDataType.ColumnName.Aliased("x", CaseSensitivity.CaseInsensitive), new ColumnDataType.ColumnName.Schema("y", CaseSensitivity.CaseInsensitive), because: "an alias in the source expressing an _intention_ and so must match the dest");
            };
        }

        #endregion

        #region assertion helpers

        private readonly static ITry<Unit> success = Try.SuccessUnit;

        private void BadColumnNameAssignment(ColumnDataType.ColumnName.Base source, ColumnDataType.ColumnName.Base dest, string because = "")
        {
            it[$"BAD: {source.ToString()} => {dest.ToString()}"] = () =>
                source.IsAssignableTo(dest).Should().NotBe(success, because: because);
        }

        private void GoodColumnNameAssignment(ColumnDataType.ColumnName.Base source, ColumnDataType.ColumnName.Base dest, string because = "")
        {
            it[$"GOOD: {source.ToString()} => {dest.ToString()}"] = () =>
                source.IsAssignableTo(dest).Should().Be(success, because: because);
        }

        private void BadTypeAssignment(DataType source, DataType dest, string because = "")
        {
            it[$"BAD: {source.ToString()} => {dest.ToString()}"] = () =>
                source.IsAssignableTo(dest).Should().NotBe(success, because: because);
        }

        private void GoodTypeAssignment(DataType source, DataType dest, string because = "")
        {
            it[$"GOOD: {source.ToString()} => {dest.ToString()}"] = () =>
                source.IsAssignableTo(dest).Should().Be(success, because: because);
        }

        private void GoodTypeComparison(DataType left, DataType right, string because = "")
        {
            it[$"GOOD: {left.ToString()} = {right.ToString()}"] = () =>
                left.CanCompareWith(right)
                .Should().Be(success, because: because);
        }

        private void BadTypeComparison(DataType left, DataType right, string because = "")
        {
            it[$"BAD: {left.ToString()} = {right.ToString()}"] = () =>
                left.CanCompareWith(right)                                
                .Should().NotBe(success, because);
        }

        #endregion
    }
#pragma warning restore IDE1006 // Naming Styles
}
