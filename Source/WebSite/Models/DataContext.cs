﻿//******************************************************************************************************
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
using System.Web.Hosting;
using GSF;
using GSF.Collections;
using GSF.Data;
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
        private readonly string m_settingsCategory;
        private readonly bool m_disposeConnection;
        private string m_addInputFieldTemplate;
        private string m_addTextAreaFieldTemplate;
        private string m_addSelectFieldTemplate;
        private string m_addCheckBoxFieldTemplate;
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
        /// Gets the input field razor template file name.
        /// </summary>
        public string AddInputFieldTemplate => m_addInputFieldTemplate ?? (m_addInputFieldTemplate = HostingEnvironment.MapPath("~/Views/Shared/AddInputField.cshtml"));

        /// <summary>
        /// Gets the text area field razor template file name.
        /// </summary>
        public string AddTextAreaFieldTemplate => m_addTextAreaFieldTemplate ?? (m_addTextAreaFieldTemplate = HostingEnvironment.MapPath("~/Views/Shared/AddTextAreaField.cshtml"));

        /// <summary>
        /// Gets the select field razor template file name.
        /// </summary>
        public string AddSelectFieldTemplate => m_addSelectFieldTemplate ?? (m_addSelectFieldTemplate = HostingEnvironment.MapPath("~/Views/Shared/AddSelectField.cshtml"));

        /// <summary>
        /// Gets the check box field razor template file name.
        /// </summary>
        public string AddCheckBoxFieldTemplate => m_addCheckBoxFieldTemplate ?? (m_addCheckBoxFieldTemplate = HostingEnvironment.MapPath("~/Views/Shared/AddCheckBoxField.cshtml"));

        /// <summary>
        /// Gets validation pattern and error message for rendered fields, if any.
        /// </summary>
        public Dictionary<string, Tuple<string, string>> FieldValidationParameters => m_fieldValidationParameters;

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
        /// Generates template based input text field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input text field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <returns>Generated HTML for new input field based on modeled table field attributes.</returns>
        public string AddInputField<T>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null) where T : class, new()
        {
            TableOperations<T> tableOperations = Table<T>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(inputType) && IsNumericType(tableOperations.GetFieldType(fieldName)))
                inputType = "number";

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

            return AddInputField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip);
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
        /// <returns>Generated HTML for new input field based on specified parameters.</returns>
        public string AddInputField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null)
        {
            RazorView<CSharp> addInputFieldTemplate = new RazorView<CSharp>(AddInputFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addInputFieldTemplate.ViewBag;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID ?? $"input{fieldName}");
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
        /// <param name="fieldLabel">Label name for text area field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <returns>Generated HTML for new text area field based on modeled table field attributes.</returns>
        public string AddTextAreaField<T>(string fieldName, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null) where T : class, new()
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

            return AddTextAreaField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, rows, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip);
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
        /// <returns>Generated HTML for new text area field based on specified parameters.</returns>
        public string AddTextAreaField(string fieldName, bool required, int maxLength = 0, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null)
        {
            RazorView<CSharp> addTextAreaTemplate = new RazorView<CSharp>(AddTextAreaFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addTextAreaTemplate.ViewBag;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("Rows", rows);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID ?? $"text{fieldName}");
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
        /// <param name="fieldLabel">Label name for select field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Generated HTML for new text field based on modeled table field attributes.</returns>
        public string AddSelectField<TSelect, TOption>(string fieldName, string optionValueFieldName, string optionLabelFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, RecordRestriction restriction = null) where TSelect : class, new() where TOption : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<TSelect>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            return AddSelectField<TOption>(fieldName, Table<TSelect>().FieldHasAttribute<RequiredAttribute>(fieldName),
                optionValueFieldName, optionLabelFieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip, restriction);
        }

        /// <summary>
        /// Generates template based select field based on specified parameters.
        /// </summary>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Generated HTML for new text field based on specified parameters.</returns>
        public string AddSelectField<TOption>(string fieldName, bool required, string optionValueFieldName, string optionLabelFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, RecordRestriction restriction = null) where TOption : class, new()
        {
            RazorView<CSharp> addSelectFieldTemplate = new RazorView<CSharp>(AddSelectFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addSelectFieldTemplate.ViewBag;
            TableOperations<TOption> optionTableOperations = Table<TOption>();
            Dictionary<string, string> options = new Dictionary<string, string>();
            string optionTableName = typeof(TOption).Name;

            optionLabelFieldName = optionLabelFieldName ?? optionValueFieldName;
            fieldLabel = fieldLabel ?? optionTableName;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("FieldLabel", fieldLabel);
            viewBag.AddValue("FieldID", fieldID ?? $"select{fieldName}");
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            if (restriction == null)
                foreach (TOption record in QueryRecords<TOption>($"SELECT {optionValueFieldName} FROM {optionTableName} ORDER BY {optionLabelFieldName}"))
                    options.Add(optionTableOperations.GetFieldValue(record, optionValueFieldName).ToString(), optionTableOperations.GetFieldValue(record, optionLabelFieldName).ToNonNullString(fieldLabel));
            else
                foreach (TOption record in QueryRecords<TOption>($"SELECT {optionValueFieldName} FROM {optionTableName} WHERE {restriction.FilterExpression} ORDER BY {optionLabelFieldName}", restriction.Parameters))
                    options.Add(optionTableOperations.GetFieldValue(record, optionValueFieldName).ToString(), optionTableOperations.GetFieldValue(record, optionLabelFieldName).ToNonNullString(fieldLabel));

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
        /// <returns>Generated HTML for new check box field based on modeled table field attributes.</returns>
        public string AddCheckBoxField<T>(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null) where T : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<T>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            return AddCheckBoxField(fieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip);
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
        /// <returns>Generated HTML for new check box field based on specified parameters.</returns>
        public string AddCheckBoxField(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null)
        {
            RazorView<CSharp> addCheckBoxFieldTemplate = new RazorView<CSharp>(AddCheckBoxFieldTemplate, MvcApplication.DefaultModel);
            DynamicViewBag viewBag = addCheckBoxFieldTemplate.ViewBag;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID ?? $"check{fieldName}");
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addCheckBoxFieldTemplate.Execute();
        }

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
