using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace AfsprakenbeheerPsycholoog.Data
{
    public class CustomSqliteMigrationsSqlGenerator : SqliteMigrationsSqlGenerator
    {
        public CustomSqliteMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            IRelationalAnnotationProvider migrationsAnnotations)
            : base(dependencies, migrationsAnnotations)
        {
        }

        public override IReadOnlyList<MigrationCommand> Generate(
            IReadOnlyList<MigrationOperation> operations,
            IModel? model = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            foreach (var op in operations)
            {
                FixTypes(op);
            }
            return base.Generate(operations, model, options);
        }

        protected override string? GetColumnType(
            string? schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel? model)
        {
            var columnType = base.GetColumnType(schema, table, name, operation, model);
            return TranslateType(columnType);
        }

        protected override void ColumnDefinition(
            string? schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            FixColumnOperation(operation);

            if (operation.ColumnType == null)
            {
                var resolvedType = GetColumnType(schema, table, name, operation, model);
                if (resolvedType != null)
                {
                    operation.ColumnType = TranslateType(resolvedType);
                }
            }

            base.ColumnDefinition(schema, table, name, operation, model, builder);
        }

        private void FixTypes(MigrationOperation operation)
        {
            if (operation is CreateTableOperation createTableOperation)
            {
                foreach (var column in createTableOperation.Columns)
                {
                    FixColumnOperation(column);
                }
            }
            else if (operation is AddColumnOperation addColumnOperation)
            {
                FixColumnOperation(addColumnOperation);
            }
            else if (operation is AlterColumnOperation alterColumnOperation)
            {
                FixColumnOperation(alterColumnOperation);
                if (alterColumnOperation.OldColumn != null)
                {
                    FixColumnOperation(alterColumnOperation.OldColumn);
                }
            }
        }

        private void FixColumnOperation(ColumnOperation column)
        {
            if (column.ColumnType != null)
            {
                column.ColumnType = TranslateType(column.ColumnType);
            }
        }

        private string? TranslateType(string? columnType)
        {
            if (columnType != null)
            {
                if (columnType.Contains("nvarchar(max)"))
                {
                    columnType = columnType.Replace("nvarchar(max)", "TEXT");
                }
                if (columnType.Contains("nvarchar"))
                {
                    columnType = columnType.Replace("nvarchar", "TEXT");
                }
                if (columnType.Contains("datetimeoffset"))
                {
                    columnType = columnType.Replace("datetimeoffset", "TEXT");
                }
                if (columnType.Equals("int", System.StringComparison.OrdinalIgnoreCase) ||
                    columnType.Equals("bigint", System.StringComparison.OrdinalIgnoreCase) ||
                    columnType.Equals("smallint", System.StringComparison.OrdinalIgnoreCase) ||
                    columnType.Equals("tinyint", System.StringComparison.OrdinalIgnoreCase))
                {
                    columnType = "INTEGER";
                }
            }
            return columnType;
        }
    }
}
