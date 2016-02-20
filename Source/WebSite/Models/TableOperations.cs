﻿//******************************************************************************************************
//  TableOperations.cs - Gbtc
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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Reflection;

// TODO: Move into GSF

// ReSharper disable StaticMemberInGenericType
namespace openSPM.Models
{
    /// <summary>
    /// Defines a record restriction that can be applied to queries.
    /// </summary>
    public class RecordRestriction
    {
        /// <summary>
        /// Defines filter SQL expression for restriction - does not include WHERE.
        /// </summary>
        public string FilterExpression;

        /// <summary>
        /// Defines restriction parameter values.
        /// </summary>
        public object[] Parameters;
    }

    /// <summary>
    /// Defines database operations for a modeled table.
    /// </summary>
    /// <typeparam name="T">Modeled table.</typeparam>
    public class TableOperations<T> where T : class, new()
    {
        #region [ Members ]

        // Constants
        private const string CountSqlFormat = "SELECT COUNT(*) FROM {0}";
        private const string OrderBySqlFormat = "SELECT {0} FROM {1} ORDER BY {{0}}";
        private const string OrderByWhereSqlFormat = "SELECT {0} FROM {1} WHERE {{0}} ORDER BY {{1}}";
        private const string SelectSqlFormat = "SELECT * FROM {0} WHERE {1}";
        private const string AddNewSqlFormat = "INSERT INTO {0}({1}) VALUES ({2})";
        private const string UpdateSqlFormat = "UPDATE {0} SET {1} WHERE {2}";
        private const string DeleteSqlFormat = "DELETE FROM {0} WHERE {1}";

        // Fields
        private readonly AdoDataConnection m_connection;
        private IEnumerable<DataRow> m_primaryKeyCache;
        private string m_lastSortField;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TableOperations{T}"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> instance to use for database operations.</param>
        public TableOperations(AdoDataConnection connection)
        {
            m_connection = connection;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Queries database and returns modeled table records for the specified sorting and paging parameters.
        /// </summary>
        /// <param name="sortField">Field name to order-by.</param>
        /// <param name="ascending">Sort ascending flag; set to <c>false</c> for descending.</param>
        /// <param name="page">Page number of records to return.</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        public IEnumerable<T> QueryRecords(string sortField, bool ascending, int page, int pageSize, RecordRestriction restriction = null)
        {
            try
            {
                if ((object)m_primaryKeyCache == null || string.Compare(sortField, m_lastSortField, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    string orderByExpression = $"{sortField}{(ascending ? "" : " DESC")}";

                    if ((object)restriction == null)
                        m_primaryKeyCache = m_connection.RetrieveData(string.Format(s_orderBySql, orderByExpression)).AsEnumerable();
                    else
                        m_primaryKeyCache = m_connection.RetrieveData(string.Format(s_orderByWhereSql, restriction.FilterExpression, orderByExpression), restriction.Parameters).AsEnumerable();

                    m_lastSortField = sortField;
                }

                return m_primaryKeyCache.ToPagedList(page, pageSize).Select(row => LoadRecord(row.ItemArray)).Where(record => record != null);
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record query for {typeof(T).Name} \"{s_orderBySql}, 0:{sortField}, 1:{ascending}\": {ex.Message}", ex));
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Gets the total record count for the modeled table.
        /// </summary>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Total record count for the modeled table.</returns>
        public int QueryRecordCount(RecordRestriction restriction = null)
        {
            try
            {
                if ((object)restriction == null)
                    return m_connection.ExecuteScalar<int>(s_countSql);

                return m_connection.ExecuteScalar<int>($"{s_countSql} WHERE {restriction.FilterExpression}", restriction.Parameters);
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record count query for {typeof(T).Name} \"{s_countSql}\": {ex.Message}", ex));
                return -1;
            }
        }

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="primaryKeys"/>.</returns>
        public T LoadRecord(params object[] primaryKeys)
        {
            try
            {
                T record = new T();
                DataRow row = m_connection.RetrieveRow(string.Format(s_selectSql, primaryKeys));

                // Make sure record exists, return null instead of a blank record
                if (GetPrimaryKeys(row).All(Common.IsDefaultValue))
                    return null;

                foreach (PropertyInfo property in s_properties.Values)
                {
                    try
                    {
                        property.SetValue(record, row.ConvertField(property.Name, property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        MvcApplication.LogException(new InvalidOperationException($"Exception during record load field assignment for \"{typeof(T).Name}.{property.Name} = {row[property.Name]}\": {ex.Message}", ex));
                    }
                }

                return record;
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record load for {typeof(T).Name} \"{s_selectSql}, {KeyList(primaryKeys)}\": {ex.Message}", ex));
                return null;
            }
        }

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(params object[] primaryKeys)
        {
            try
            {
                int affectedRecords = m_connection.ExecuteNonQuery(s_deleteSql, primaryKeys);

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record delete for {typeof(T).Name} \"{s_deleteSql}, {KeyList(primaryKeys)}\": {ex.Message}", ex));
                return 0;
            }
        }

        /// <summary>
        /// Updates the database with the specified modeled table <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record to update.</param>
        /// <returns>Number of rows affected.</returns>
        public int UpdateRecord(T record)
        {
            List<object> values = new List<object>();

            try
            {
                foreach (PropertyInfo property in s_updateProperties)
                    values.Add(property.GetValue(record));

                foreach (PropertyInfo property in s_primaryKeyProperties)
                    values.Add(property.GetValue(record));

                return m_connection.ExecuteNonQuery(s_updateSql, values.ToArray());
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record update for {typeof(T).Name} \"{s_updateSql}, {KeyList(values)}\": {ex.Message}", ex));
                return 0;
            }
        }

        /// <summary>
        /// Adds the specified modeled table <paramref name="record"/> to the database.
        /// </summary>
        /// <param name="record">Record to add.</param>
        /// <returns>Number of rows affected.</returns>
        public int AddNewRecord(T record)
        {
            List<object> values = new List<object>();

            try
            {
                foreach (PropertyInfo property in s_addNewProperties)
                    values.Add(property.GetValue(record));

                int affectedRecords = m_connection.ExecuteNonQuery(s_addNewSql, values.ToArray());

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception during record insert for {typeof(T).Name} \"{s_addNewSql}, {KeyList(values)}\": {ex.Message}", ex));
                return 0;
            }
        }

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data.</param>
        /// <returns>Primary key values from the specified <paramref name="row"/>.</returns>
        public object[] GetPrimaryKeys(DataRow row)
        {
            try
            {
                List<object> values = new List<object>();

                foreach (PropertyInfo property in s_primaryKeyProperties)
                    values.Add(row[property.Name]);

                return values.ToArray();
            }
            catch (Exception ex)
            {
                MvcApplication.LogException(new InvalidOperationException($"Exception loading primary key fields for {typeof(T).Name} \"{s_primaryKeyProperties.Select(property => property.Name).ToDelimitedString(", ")}\": {ex.Message}", ex));
                return new object[0];
            }
        }

        /// <summary>
        /// Attempts to get the specified <paramref name="attribute"/> for a field.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to attempt to get.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attribute">Type of attribute to lookup.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        public bool TryGetFieldAttribute<TAttribute>(string fieldName, out TAttribute attribute) where TAttribute : Attribute
        {
            PropertyInfo property;

            if (s_properties.TryGetValue(fieldName, out property) && property.TryGetAttribute(out attribute))
                return true;

            attribute = default(TAttribute);
            return false;
        }

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to search for.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        public bool FieldHasAttribute<TAttribute>(string fieldName) where TAttribute : Attribute
        {
            PropertyInfo property;
            HashSet<Type> attributes;

            if (s_properties.TryGetValue(fieldName, out property) && s_attributes.TryGetValue(property, out attributes))
                return attributes.Contains(typeof(TAttribute));

           return false;
        }

        /// <summary>
        /// Gets the value for the specified field.
        /// </summary>
        /// <param name="record">Modeled table record.</param>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field value or <c>null</c> if field is not found.</returns>
        public object GetFieldValue(T record, string fieldName)
        {
            PropertyInfo property;

            if (s_properties.TryGetValue(fieldName, out property))
                return property.GetValue(record);

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the specified field.
        /// </summary>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field <see cref="Type"/> or <c>null</c> if field is not found.</returns>
        public Type GetFieldType(string fieldName)
        {
            PropertyInfo property;

            if (s_properties.TryGetValue(fieldName, out property))
                return property.PropertyType;

            return null;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Dictionary<string, PropertyInfo> s_properties;
        private static readonly Dictionary<PropertyInfo, HashSet<Type>> s_attributes;
        private static readonly PropertyInfo[] s_addNewProperties;
        private static readonly PropertyInfo[] s_updateProperties;
        private static readonly PropertyInfo[] s_primaryKeyProperties;
        private static readonly string s_countSql;
        private static readonly string s_orderBySql;
        private static readonly string s_orderByWhereSql;
        private static readonly string s_selectSql;
        private static readonly string s_addNewSql;
        private static readonly string s_updateSql;
        private static readonly string s_deleteSql;

        // Static Constructor
        static TableOperations()
        {
            StringBuilder addNewFields = new StringBuilder();
            StringBuilder addNewFormat = new StringBuilder();
            StringBuilder updateFormat = new StringBuilder();
            StringBuilder whereFormat = new StringBuilder();
            StringBuilder primaryKeyFields = new StringBuilder();
            List<PropertyInfo> addNewProperties = new List<PropertyInfo>();
            List<PropertyInfo> updateproperties = new List<PropertyInfo>();
            List<PropertyInfo> primaryKeyProperties = new List<PropertyInfo>();
            string tableName = typeof(T).Name;
            int primaryKeyIndex = 0;
            int addNewFieldIndex = 0;
            int updateFieldIndex = 0;

            s_properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite)
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            s_attributes = new Dictionary<PropertyInfo, HashSet<Type>>();

            foreach (PropertyInfo property in s_properties.Values)
            {
                PrimaryKeyAttribute primaryKeyAttribute;
                property.TryGetAttribute(out primaryKeyAttribute);

                if ((object)primaryKeyAttribute != null)
                {
                    if (!primaryKeyAttribute.IsIdentity)
                    {
                        addNewFields.Append($"{(addNewFields.Length > 0 ? ", " : "")}{property.Name}");
                        addNewFormat.Append($"{(addNewFormat.Length > 0 ? ", " : "")}{{{addNewFieldIndex++}}}");
                        addNewProperties.Add(property);
                    }

                    whereFormat.Append($"{(whereFormat.Length > 0 ? "AND " : "")}{property.Name}={{{primaryKeyIndex++}}}");
                    primaryKeyFields.Append($"{(primaryKeyFields.Length > 0 ? ", " : "")}{property.Name}");
                    primaryKeyProperties.Add(property);
                }
                else
                {
                    addNewFields.Append($"{(addNewFields.Length > 0 ? ", " : "")}{property.Name}");
                    addNewFormat.Append($"{(addNewFormat.Length > 0 ? ", " : "")}{{{addNewFieldIndex++}}}");
                    updateFormat.Append($"{(updateFormat.Length > 0 ? ", " : "")}{property.Name}={{{updateFieldIndex++}}}");
                    addNewProperties.Add(property);
                    updateproperties.Add(property);
                }

                s_attributes.Add(property, new HashSet<Type>(property.CustomAttributes.Select(attributeData => attributeData.AttributeType)));
            }

            List<object> updateWhereOffsets = new List<object>();

            for (int i = 0; i < primaryKeyIndex; i++)
                updateWhereOffsets.Add($"{{{updateFieldIndex + i}}}");

            s_countSql = string.Format(CountSqlFormat, tableName);
            s_orderBySql = string.Format(OrderBySqlFormat, primaryKeyFields, tableName);
            s_orderByWhereSql = string.Format(OrderByWhereSqlFormat, primaryKeyFields, tableName);
            s_selectSql = string.Format(SelectSqlFormat, tableName, whereFormat);
            s_addNewSql = string.Format(AddNewSqlFormat, tableName, addNewFields, addNewFormat);
            s_updateSql = string.Format(UpdateSqlFormat, tableName, updateFormat, string.Format(whereFormat.ToString(), updateWhereOffsets.ToArray()));
            s_deleteSql = string.Format(DeleteSqlFormat, tableName, whereFormat);

            s_addNewProperties = addNewProperties.ToArray();
            s_updateProperties = updateproperties.ToArray();
            s_primaryKeyProperties = primaryKeyProperties.ToArray();
        }

        // Static Methods
        private static string KeyList(IReadOnlyList<object> primaryKeys)
        {
            StringBuilder delimitedString = new StringBuilder();


            for (int i = 0; i < primaryKeys.Count; i++)
            {
                if (delimitedString.Length > 0)
                    delimitedString.Append(", ");

                delimitedString.AppendFormat("{0}:{1}", i, primaryKeys[i]);
            }

            return delimitedString.ToString();
        }

        #endregion
    }
}
