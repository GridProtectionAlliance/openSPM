//******************************************************************************************************
//  DataContext.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/01/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Reflection;
using openSPM.Attributes;
using RazorEngine.Templating;

// TODO: Move into GSF - Razor field functions should be in a Web based extension 

namespace openSPM.Models
{
    /// <summary>
    /// Defines a data context for the current model.
    /// </summary>
    public class DataContext : IDisposable
    {
        #region [ Members ]

        // Fields
        private AdoDataConnection m_connection;
        private readonly Dictionary<Type, object> m_tableOperations;
        private readonly Dictionary<string, Tuple<string, string>> m_fieldValidationParameters;
        private readonly List<Tuple<string, string>> m_fieldValueInitializers; 
        private readonly List<string> m_definedDateFields; 
        private readonly string m_settingsCategory;
        private readonly bool m_disposeConnection;
        private string m_initialFocusField;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataContext"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> to use; defaults to a new connection.</param>
        /// <param name="disposeConnection">Set to <c>true</c> to dispose the provided <paramref name="connection"/>.</param>
        public DataContext(AdoDataConnection connection = null, bool disposeConnection = false)
        {
            m_connection = connection;
            m_tableOperations = new Dictionary<Type, object>();
            m_fieldValidationParameters = new Dictionary<string, Tuple<string, string>>();
            m_fieldValueInitializers = new List<Tuple<string, string>>();
            m_definedDateFields = new List<string>();
            m_settingsCategory = "systemSettings";
            m_disposeConnection = disposeConnection || connection == null;
        }

        /// <summary>
        /// Creates a new <see cref="DataContext"/> using the specified <paramref name="settingsCategory"/>.
        /// </summary>
        /// <param name="settingsCategory">Setting category that contains the connection settings.</param>
        public DataContext(string settingsCategory)
        {
            m_tableOperations = new Dictionary<Type, object>();
            m_fieldValidationParameters = new Dictionary<string, Tuple<string, string>>();
            m_settingsCategory = settingsCategory;
            m_disposeConnection = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="AdoDataConnection"/> for this <see cref="DataContext"/>.
        /// </summary>
        public AdoDataConnection Connection => m_connection ?? (m_connection = new AdoDataConnection(m_settingsCategory));

        /// <summary>
        /// Gets validation pattern and error message for rendered fields, if any.
        /// </summary>
        public Dictionary<string, Tuple<string, string>> FieldValidationParameters => m_fieldValidationParameters;

        /// <summary>
        /// Gets field value initializers, if any.
        /// </summary>
        public List<Tuple<string, string>> FieldValueInitializers => m_fieldValueInitializers;

        /// <summary>
        /// Gets defined date fields, if any.
        /// </summary>
        public List<string> DefinedDateFields => m_definedDateFields;

        /// <summary>
        /// Gets field name designated for initial focus.
        /// </summary>
        public string InitialFocusField => m_initialFocusField;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataContext"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataContext"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_disposeConnection)
                            m_connection?.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets the table operations for the specified modeled table <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <returns>Table operations for the specified modeled table <typeparamref name="T"/>.</returns>
        public TableOperations<T> Table<T>() where T : class, new()
        {
            return m_tableOperations.GetOrAdd(typeof(T), type => new TableOperations<T>(Connection)) as TableOperations<T>;
        }

        /// <summary>
        /// Queries a new modeled table record using the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="primaryKeys"/>.</returns>
        public T QueryRecord<T>(params object[] primaryKeys) where T : class, new()
        {
            return Table<T>().LoadRecord(primaryKeys);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified sql statement and parameters.
        /// </summary>
        /// <param name="sqlFormat">SQL expression to query.</param>
        /// <param name="parameters">Parameters for query, if any.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        public IEnumerable<T> QueryRecords<T>(string sqlFormat, params object[] parameters) where T : class, new()
        {
            try
            {
                TableOperations<T> tableOperations = Table<T>();
                return m_connection.RetrieveData(sqlFormat, parameters).AsEnumerable().Select(row => tableOperations.LoadRecord(tableOperations.GetPrimaryKeys(row)));
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record query for {typeof(T).Name} \"{sqlFormat}{ParamList(parameters)}\": {ex.Message}", ex));
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Gets the field name targeted as the primary label for the modeled table.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <returns>Field name targeted as the primary label for the modeled table.</returns>
        public string GetPrimaryLabelField<T>() where T : class, new()
        {
            PrimaryLabelAttribute primaryLabelAttribute;

            if (typeof(T).TryGetAttribute(out primaryLabelAttribute) && !string.IsNullOrEmpty(primaryLabelAttribute.FieldName))
                return primaryLabelAttribute.FieldName;

            string[] fieldNames = Table<T>().GetFieldNames();
            return fieldNames.Length > 0 ? fieldNames[0] : "";
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="DataHub"/>.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>Typically used in paged view model scenarios and invoked by controller prior to view rendering.</remarks>
        public void EstablishUserRolesForPage<T>(dynamic viewBag) where T : class, new()
        {
            // Get any authorized roles for key records operations of modeled table
            Tuple<string, string>[] recordOperations = DataHub.GetRecordOperations<T>();
            viewBag.EditRoles = recordOperations[(int)RecordOperation.UpdateRecord].Item2;
            viewBag.AddNewRoles = recordOperations[(int)RecordOperation.AddNewRecord].Item2;
            viewBag.DeleteRoles = recordOperations[(int)RecordOperation.DeleteRecord].Item2;
        }

        /// <summary>
        /// Renders client-side configuration script for paged view model.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="defaultSortField">Default sort field, defaults to first primary key field.</param>
        /// <returns>Rendered paged view model configuration script.</returns>
        public string RenderViewModelConfiguration<T>(string defaultSortField = null) where T : class, new()
        {
            StringBuilder javascript = new StringBuilder();
            string[] primaryKeyFields = Table<T>().GetPrimaryKeyFieldNames();

            Func<string, string> fixForwardSpacing = newScript =>
            {
                Tuple<string, int>[] linesAndLengths = newScript
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(line => new Tuple<string, int>(line, line.Length - line.TrimStart(' ').Length))
                    .ToArray();

                int minLength = linesAndLengths
                    .Select(lineAndLength => lineAndLength.Item2)
                    .Where(length => length > 0)
                    .Min();

                return linesAndLengths
                    .Select(lineAndLength => lineAndLength.Item2 > 0 ?
                        $"    {(lineAndLength.Item2 > minLength ? lineAndLength.Item1.Substring(minLength) : lineAndLength.Item1)}" :
                        lineAndLength.Item1.ToNonNullNorEmptyString())
                    .ToDelimitedString(Environment.NewLine);
            };

            javascript.Append(fixForwardSpacing($@"// Configure view model
                viewModel.defaultSortField = ""{defaultSortField ?? primaryKeyFields[0]}"";
                viewModel.labelField = ""{GetPrimaryLabelField<T>()}"";
                viewModel.primaryKeyFields = [{primaryKeyFields.Select(fieldName => $"\"{fieldName}\"").ToDelimitedString(", ")}];
            "));

            // Get method names for records operations of modeled table
            Tuple<string, string>[] recordOperations = DataHub.GetRecordOperations<T>();
            string queryRecordCountMethod = recordOperations[(int)RecordOperation.QueryRecordCount].Item1;
            string queryRecordsMethod = recordOperations[(int)RecordOperation.QueryRecords].Item1;
            string deleteRecordMethod = recordOperations[(int)RecordOperation.DeleteRecord].Item1;
            string createNewRecordMethod = recordOperations[(int)RecordOperation.CreateNewRecord].Item1;
            string addNewRecordMethod = recordOperations[(int)RecordOperation.AddNewRecord].Item1;
            string updateMethod = recordOperations[(int)RecordOperation.UpdateRecord].Item1;

            Func<string, string> toCamelCase = methodName => $"{char.ToLower(methodName[0])}{methodName.Substring(1)}";

            if (!string.IsNullOrWhiteSpace(queryRecordCountMethod))
                javascript.Append(fixForwardSpacing($@"
                    viewModel.setQueryRecordCount(function () {{
                        return dataHub.{toCamelCase(queryRecordCountMethod)}();
                    }});
                "));

            if (!string.IsNullOrWhiteSpace(queryRecordsMethod))
                javascript.Append(fixForwardSpacing($@"
                    viewModel.setQueryRecords(function (sortField, ascending, page, pageSize) {{
                        return dataHub.{toCamelCase(queryRecordsMethod)}(sortField, ascending, page, pageSize);
                    }});
                "));

            if (!string.IsNullOrWhiteSpace(deleteRecordMethod))
                javascript.Append(fixForwardSpacing($@"
                    viewModel.setDeleteRecord(function (keyValues) {{
                        return dataHub.{toCamelCase(deleteRecordMethod)}({Enumerable.Range(0, Table<T>().GetPrimaryKeyFieldNames().Length).Select(index => $"keyValues[{index}]").ToDelimitedString(", ")});
                    }});
                "));

            if (!string.IsNullOrWhiteSpace(createNewRecordMethod))
                javascript.Append(fixForwardSpacing($@"
                    viewModel.setNewRecord(function () {{
                        return dataHub.{toCamelCase(createNewRecordMethod)}();
                    }});
                "));

            if (!string.IsNullOrWhiteSpace(addNewRecordMethod))
                javascript.Append(fixForwardSpacing($@"
                    viewModel.setAddNewRecord(function (record) {{
                        return dataHub.{toCamelCase(addNewRecordMethod)}(record);
                    }});
                "));

            if (!string.IsNullOrWhiteSpace(updateMethod))
                javascript.Append(fixForwardSpacing($@"
                    viewModel.setUpdateRecord(function (record) {{
                        return dataHub.{toCamelCase(updateMethod)}(record);
                    }});
                "));

            return javascript.ToString();
        }

        /// <summary>
        /// Adds a new pattern based validation and option error message to a field.
        /// </summary>
        /// <param name="observableFieldReference">Observable field reference (from JS view model).</param>
        /// <param name="validationPattern">Regex based validation pattern.</param>
        /// <param name="errorMessage">Optional error message to display when pattern fails.</param>
        public void AddFieldValidation(string observableFieldReference, string validationPattern, string errorMessage = null)
        {
            m_fieldValidationParameters[observableFieldReference] = new Tuple<string, string>(validationPattern, errorMessage);
        }

        /// <summary>
        /// Adds a new field value initializer.
        /// </summary>
        /// <param name="fieldName">Field name (as defined in model).</param>
        /// <param name="initialValue">Javascript based initial value for field.</param>
        public void AddFieldValueInitializer(string fieldName, string initialValue = null)
        {
            m_fieldValueInitializers.Add(new Tuple<string, string>(fieldName, initialValue ?? "\"\""));
        }

        /// <summary>
        /// Adds a new field value initializer based on modeled table field attributes.
        /// </summary>
        /// <param name="fieldName">Field name (as defined in model).</param>
        public void AddFieldValueInitializer<T>(string fieldName) where T : class, new()
        {
            InitialValueAttribute initialValueAttribute;

            if (Table<T>().TryGetFieldAttribute(fieldName, out initialValueAttribute))
                AddFieldValueInitializer(fieldName, initialValueAttribute.InitialValue);
        }

        /// <summary>
        /// Generates template based input date field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input date field.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input date field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to date + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new date based input field based on modeled table field attributes.</returns>
        public string AddDateField<T>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where T : class, new()
        {
            TableOperations<T> tableOperations = Table<T>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if (!string.IsNullOrEmpty(regularExpressionAttribute?.ErrorMessage))
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage);
            }

            AddFieldValueInitializer<T>(fieldName);

            return AddDateField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based input date field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for input date field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum input field length.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input date field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to date + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new date based input field based on specified parameters.</returns>
        public string AddDateField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<CSharp> addDateFieldTemplate = new RazorView<CSharp>(AddDateFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addDateFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"date{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            m_definedDateFields.Add(fieldName);

            return addDateFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based input text field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input text field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new input field based on modeled table field attributes.</returns>
        public string AddInputField<T>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where T : class, new()
        {
            TableOperations<T> tableOperations = Table<T>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(inputType))
            {
                Type fieldType = tableOperations.GetFieldType(fieldName);

                if (IsNumericType(fieldType))
                    inputType = "number";
                else if (fieldType == typeof(DateTime))
                    inputType = "date";
            } 

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if (!string.IsNullOrEmpty(regularExpressionAttribute?.ErrorMessage))
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage);
            }

            AddFieldValueInitializer<T>(fieldName);

            return AddInputField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based input text field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum input field length.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input text field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new input field based on specified parameters.</returns>
        public string AddInputField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<CSharp> addInputFieldTemplate = new RazorView<CSharp>(AddInputFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addInputFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"input{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addInputFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based text area field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for text area field.</param>
        /// <param name="rows">Number of rows for text area.</param>
        /// <param name="fieldLabel">Label name for text area field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new text area field based on modeled table field attributes.</returns>
        public string AddTextAreaField<T>(string fieldName, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where T : class, new()
        {
            TableOperations<T> tableOperations = Table<T>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if (!string.IsNullOrEmpty(regularExpressionAttribute?.ErrorMessage))
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage);
            }

            AddFieldValueInitializer<T>(fieldName);

            return AddTextAreaField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, rows, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based text area field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for text area field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum text area field length.</param>
        /// <param name="rows">Number of rows for text area.</param>
        /// <param name="fieldLabel">Label name for text area field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new text area field based on specified parameters.</returns>
        public string AddTextAreaField(string fieldName, bool required, int maxLength = 0, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<CSharp> addTextAreaTemplate = new RazorView<CSharp>(AddTextAreaFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addTextAreaTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"text{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("Rows", rows);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addTextAreaTemplate.Execute();
        }

        /// <summary>
        /// Generates template based select field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TSelect">Modeled table for select field.</typeparam>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to <paramref name="optionLabelFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Generated HTML for new text field based on modeled table field attributes.</returns>
        public string AddSelectField<TSelect, TOption>(string fieldName, string optionValueFieldName, string optionLabelFieldName = null, string optionSortFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false, RecordRestriction restriction = null) where TSelect : class, new() where TOption : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<TSelect>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            AddFieldValueInitializer<TSelect>(fieldName);

            return AddSelectField<TOption>(fieldName, Table<TSelect>().FieldHasAttribute<RequiredAttribute>(fieldName),
                optionValueFieldName, optionLabelFieldName, optionSortFieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus, restriction);
        }

        /// <summary>
        /// Generates template based select field based on specified parameters.
        /// </summary>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to <paramref name="optionLabelFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Generated HTML for new text field based on specified parameters.</returns>
        public string AddSelectField<TOption>(string fieldName, bool required, string optionValueFieldName, string optionLabelFieldName = null, string optionSortFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false, RecordRestriction restriction = null) where TOption : class, new()
        {
            RazorView<CSharp> addSelectFieldTemplate = new RazorView<CSharp>(AddSelectFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addSelectFieldTemplate.ViewBag;
            TableOperations<TOption> optionTableOperations = Table<TOption>();
            Dictionary<string, string> options = new Dictionary<string, string>();
            string optionTableName = typeof(TOption).Name;

            optionLabelFieldName = optionLabelFieldName ?? optionValueFieldName;
            optionSortFieldName = optionSortFieldName ?? optionLabelFieldName;
            fieldLabel = fieldLabel ?? optionTableName;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"select{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("FieldLabel", fieldLabel);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            if (restriction == null)
            {
                foreach (TOption record in QueryRecords<TOption>($"SELECT * FROM {optionTableName} ORDER BY {optionSortFieldName}"))
                {
                    if (record != null)
                        options.Add(optionTableOperations.GetFieldValue(record, optionValueFieldName).ToString(), optionTableOperations.GetFieldValue(record, optionLabelFieldName).ToNonNullString(fieldLabel));
                }
            }
            else
            {
                foreach (TOption record in QueryRecords<TOption>($"SELECT * FROM {optionTableName} WHERE {restriction.FilterExpression} ORDER BY {optionSortFieldName}", restriction.Parameters))
                {
                    if (record != null)
                        options.Add(optionTableOperations.GetFieldValue(record, optionValueFieldName).ToString(), optionTableOperations.GetFieldValue(record, optionLabelFieldName).ToNonNullString(fieldLabel));
                }
            }

            viewBag.AddValue("Options", options);

            return addSelectFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based check box field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for value of check box field.</param>
        /// <param name="fieldLabel">Label name for check box field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for check box field; defaults to check + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new check box field based on modeled table field attributes.</returns>
        public string AddCheckBoxField<T>(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where T : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<T>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            AddFieldValueInitializer<T>(fieldName);

            return AddCheckBoxField(fieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based check box field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for value of check box field.</param>
        /// <param name="fieldLabel">Label name for check box field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for check box field; defaults to check + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new check box field based on specified parameters.</returns>
        public string AddCheckBoxField(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView<CSharp> addCheckBoxFieldTemplate = new RazorView<CSharp>(AddCheckBoxFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addCheckBoxFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"check{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addCheckBoxFieldTemplate.Execute();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static string s_addDateFieldTemplate;
        private static string s_addInputFieldTemplate;
        private static string s_addTextAreaFieldTemplate;
        private static string s_addSelectFieldTemplate;
        private static string s_addCheckBoxFieldTemplate;

        // Static Properties

        /// <summary>
        /// Gets the date field razor template file name.
        /// </summary>
        public static string AddDateFieldTemplate => s_addDateFieldTemplate ?? (s_addDateFieldTemplate = $"{RazorView<CSharp>.TemplatePath}AddDateField.cshtml");

        /// <summary>
        /// Gets the input field razor template file name.
        /// </summary>
        public static string AddInputFieldTemplate => s_addInputFieldTemplate ?? (s_addInputFieldTemplate = $"{RazorView<CSharp>.TemplatePath}AddInputField.cshtml");

        /// <summary>
        /// Gets the text area field razor template file name.
        /// </summary>
        public static string AddTextAreaFieldTemplate => s_addTextAreaFieldTemplate ?? (s_addTextAreaFieldTemplate = $"{RazorView<CSharp>.TemplatePath}AddTextAreaField.cshtml");

        /// <summary>
        /// Gets the select field razor template file name.
        /// </summary>
        public static string AddSelectFieldTemplate => s_addSelectFieldTemplate ?? (s_addSelectFieldTemplate = $"{RazorView<CSharp>.TemplatePath}AddSelectField.cshtml");

        /// <summary>
        /// Gets the check box field razor template file name.
        /// </summary>
        public static string AddCheckBoxFieldTemplate => s_addCheckBoxFieldTemplate ?? (s_addCheckBoxFieldTemplate = $"{RazorView<CSharp>.TemplatePath}AddCheckBoxField.cshtml");

        // Static Methods
        private static string ParamList(IReadOnlyList<object> parameters)
        {
            StringBuilder delimitedString = new StringBuilder();


            for (int i = 0; i < parameters.Count; i++)
                delimitedString.AppendFormat(", {0}:{1}", i, parameters[i]);

            return delimitedString.ToString();
        }

        private static bool IsNumericType(Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(Int24) ||
                type == typeof(UInt24) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

        #endregion
    }
}
