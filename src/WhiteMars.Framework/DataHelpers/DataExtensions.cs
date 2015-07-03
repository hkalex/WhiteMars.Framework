using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using WhiteMars.Framework;

namespace WhiteMars.Framework
{
    /// <summary>
    /// This is a Utility class providing many useful database connection methods
    /// </summary>
    public static class DataExtensions
    {
        public static readonly int DEFAULT_TIMEOUT;

        static DataExtensions()
        {
            var timeoutString = System.Configuration.ConfigurationManager.AppSettings["CommonadTimeout"];
            int timeout = 120;
            if (int.TryParse(timeoutString, out timeout))
            {
                DEFAULT_TIMEOUT = timeout;
            }
            else
            {
                DEFAULT_TIMEOUT = 120;
            }
        }


        #region ToXXX

        /// <summary>
        /// Convert reader to DataTable. This will convert XML columns to XElement with "root" as the root element.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this DbDataReader reader)
        {
            var xmlColumns = new List<string>();

            // read the field first, just in case the Load() method would close the reader.
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var dataType = reader.GetDataTypeName(i);
                if (string.Compare(dataType, "xml", true) == 0)
                {
                    xmlColumns.Add(name);
                }
            }

            var result = new DataTable();
            result.Load(reader);

            // handle XML columns
            if (xmlColumns.Any())
            {
                foreach (var xc in xmlColumns)
                {
                    var oldColumn = result.Columns[xc];
                    var newColumn = new DataColumn(xc, typeof(XElement));

                    oldColumn.ColumnName = xc + "__String";

                    newColumn.AllowDBNull = oldColumn.AllowDBNull;
                    newColumn.AutoIncrement = oldColumn.AutoIncrement;
                    newColumn.AutoIncrementSeed = oldColumn.AutoIncrementSeed;
                    newColumn.AutoIncrementStep = oldColumn.AutoIncrementStep;
                    newColumn.Caption = oldColumn.Caption;
                    newColumn.ColumnMapping = oldColumn.ColumnMapping;
                    //newColumn.ColumnName = oldColumn.ColumnName;
                    //newColumn.Container = oldColumn.Container;
                    //newColumn.DataType = oldColumn.DataType;
                    newColumn.DateTimeMode = oldColumn.DateTimeMode;
                    newColumn.DefaultValue = oldColumn.DefaultValue;
                    //newColumn.DesignMode = oldColumn.DesignMode;
                    newColumn.Expression = oldColumn.Expression;
                    //newColumn.ExtendedProperties = oldColumn.ExtendedProperties;
                    if (oldColumn.MaxLength >= 0 && oldColumn.MaxLength != int.MaxValue)
                    {
                        newColumn.MaxLength = oldColumn.MaxLength;
                    }
                    newColumn.Namespace = oldColumn.Namespace;
                    //newColumn.Ordinal = oldColumn.Ordinal;
                    newColumn.Prefix = oldColumn.Prefix;
                    newColumn.ReadOnly = oldColumn.ReadOnly;
                    newColumn.Site = oldColumn.Site;
                    //newColumn.Table = oldColumn.Table;
                    newColumn.Unique = oldColumn.Unique;


                    result.Columns.Add(newColumn);
                }

                foreach (DataRow row in result.Rows)
                {
                    foreach (var xc in xmlColumns)
                    {
                        var value = row[xc + "__String"];
                        if (value != null && value != DBNull.Value)
                        {
                            var xml = XElement.Parse("<root>" + value.ToString() + "</root>");
                            row[xc] = xml;
                        }
                    }
                }

                // remove the XML column in string type
                foreach (var xc in xmlColumns)
                {
                    result.Columns.Remove(xc + "__String");
                }
            }

            return result;
        }

        public static DataSet ToDataSet(this DbDataReader reader, params string[] tableNames)
        {
            var result = new DataSet();
            result.Load(reader, LoadOption.Upsert, tableNames);
            return result;
        }

        public static dynamic ToDynamic(this DbDataReader reader)
        {
            var result = new DynamicDictionary() as IDictionary<string, object>;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetProperValue(i);
                if (value is XElement)
                {
                    result.Add(reader.GetName(i), (value as XElement).ToDynamic());
                }
                else
                {
                    result.Add(reader.GetName(i), value);
                }
            }
            return result;
        }

        /// <summary>
        /// Convert the current row pointing by DbDataReader to any strong-typed class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T ToObject<T>(this DbDataReader reader) where T : new()
        {
            var result = new T();

            var properties = typeof(T).GetProperties().ToList();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetProperValue(i);
                if (value == DBNull.Value) value = null;
                var prop = properties.FirstOrDefault(c => c.Name == name);
                if (prop != null)
                {
                    prop.SetValue(result, value);
                }
            }

            return result;
        }

        ///// <summary>
        ///// Convert a DbDataReader to ConvertedObject type. ConvertedObject type contains a strong-typed Data and a dictionary for those fields that does not match (case-sensitive) any property names.
        ///// </summary>
        ///// <typeparam name="T">The strong-type class that must contains a default constructor</typeparam>
        ///// <param name="reader"></param>
        ///// <returns></returns>
        //public static ConvertedObject<T> ToConvertedObject<T>(this DbDataReader reader) where T : new()
        //{
        //    var result = new ConvertedObject<T>();

        //    var properties = typeof(T).GetProperties().ToList();

        //    for (var i = 0; i < reader.FieldCount; i++)
        //    {
        //        var name = reader.GetName(i);
        //        var value = reader.GetProperValue(i);
        //        if (value == DBNull.Value) value = null;
        //        var prop = properties.FirstOrDefault(c => c.Name == name);
        //        if (prop != null)
        //        {
        //            prop.SetValue(result.Data, value);
        //        }
        //        else
        //        {
        //            result.Others[name] = value;
        //        }
        //    }

        //    return result;
        //}

        /// <summary>
        /// Convert to a strong-typed object list.
        /// Before this method, the psotion of the reader should be before the first record. After this method, the position of the reader will be the end of last record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IList<T> ToObjects<T>(this DbDataReader reader) where T : new()
        {
            var result = new List<T>();
            while (reader.Read())
            {
                result.Add(ToObject<T>(reader));
            }
            return result;
        }

        ///// <summary>
        ///// Convert to a strong-typed object list. ConvertedObject has Data property for the strong-typed and a dictionary for those fields does not match with any property names.
        ///// Before this method, the psotion of the reader should be before the first record. After this method, the position of the reader will be the end of last record.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="reader"></param>
        ///// <returns></returns>
        //public static IList<ConvertedObject<T>> ToConvertedObjects<T>(this DbDataReader reader) where T : new()
        //{
        //    var result = new List<ConvertedObject<T>>();
        //    while (reader.Read())
        //    {
        //        result.Add(ToConvertedObject<T>(reader));
        //    }
        //    return result;
        //}

        /// <summary>
        /// Convert to a strong-typed object list.
        /// Before this method, the psotion of the reader should be before the first record. After this method, the position of the reader will be the end of last record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IList<T> ToObjects<T>(this DataTable table) where T : new()
        {
            var result = new List<T>();

            var properties = typeof(T).GetProperties();

            foreach (DataRow row in table.Rows)
            {
                var obj = new T();
                foreach (DataColumn col in table.Columns)
                {
                    var value = row[col];
                    if (value == DBNull.Value) value = null;
                    var prop = properties.FirstOrDefault(c => c.Name == col.ColumnName);
                    if (prop != null)
                    {
                        prop.SetValue(obj, value);
                    }
                }
                result.Add(obj);
            }

            return result;
        }

        ///// <summary>
        ///// Convert to a strong-typed object list.
        ///// ConvertedObject has Data property for the strong-typed and a dictionary for those fields does not match with any property names.
        ///// Before this method, the psotion of the reader should be before the first record. After this method, the position of the reader will be the end of last record.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="table"></param>
        ///// <returns></returns>
        //public static IList<ConvertedObject<T>> ToConvertedObjects<T>(this DataTable table) where T : new()
        //{
        //    var result = new List<ConvertedObject<T>>();

        //    var properties = typeof(T).GetProperties();

        //    foreach (DataRow row in table.Rows)
        //    {
        //        var obj = new ConvertedObject<T>();
        //        foreach (DataColumn col in table.Columns)
        //        {
        //            var value = row[col];
        //            if (value == DBNull.Value) value = null;
        //            var prop = properties.FirstOrDefault(c => c.Name == col.ColumnName);
        //            if (prop != null)
        //            {
        //                prop.SetValue(obj.Data, value);
        //            }
        //            else
        //            {
        //                obj.Others[col.ColumnName] = value;
        //            }
        //        }
        //        result.Add(obj);
        //    }

        //    return result;
        //}

        /// <summary>
        /// Convert XElement to dynamic object
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this XElement node)
        {
            if (node == null) return null;
            var result = new DynamicDictionary();
            ParseXmlToDynamic(result, node);
            return result;
        }

        private static void ParseXmlToDynamic(dynamic parent, XElement node)
        {
            if (node.HasElements)
            {
                var firstNode = node.Elements().First();
                var isListAttribute = firstNode.Attribute("__IsList");
                if (node.Elements(firstNode.Name.LocalName).Count() > 1 || (isListAttribute != null && isListAttribute.Value == "1"))
                {
                    //list
                    var item = new DynamicDictionary();
                    var list = new List<dynamic>();
                    foreach (var element in node.Elements())
                    {
                        ParseXmlToDynamic(list, element);
                    }

                    AddProperty(item, node.Elements().First().Name.LocalName, list);
                    AddProperty(parent, node.Name.ToString(), item);
                }
                else
                {
                    var item = new DynamicDictionary();

                    foreach (var attribute in node.Attributes())
                    {
                        AddProperty(item, attribute.Name.ToString(), attribute.Value.Trim());
                    }

                    //element
                    foreach (var element in node.Elements())
                    {
                        ParseXmlToDynamic(item, element);
                    }

                    AddProperty(parent, node.Name.ToString(), item);
                }
            }
            else
            {
                AddProperty(parent, node.Name.ToString(), node.Value.Trim());
            }
        }

        private static void AddProperty(dynamic parent, string name, object value)
        {
            if (parent is List<dynamic>)
            {
                (parent as List<dynamic>).Add(value);
            }
            else
            {
                (parent as IDictionary<String, object>)[name] = value;
            }
        }

        /// <summary>
        /// Get property value from a reader. The XML value will be parsed to XElement.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static object GetProperValue(this DbDataReader reader, int ordinal)
        {
            var dataType = reader.GetDataTypeName(ordinal);
            if (string.Compare(dataType, "xml", true) == 0)
            {
                var value = reader.GetValue(ordinal);
                if (value != null && value != DBNull.Value)
                {
                    var stringValue = value.ToString();

                    return XElement.Parse("<root>" + stringValue + "</root>");
                }
                else
                {
                    return XElement.Parse("<root></root>");
                }
            }
            else
            {
                return reader.GetValue(ordinal);
            }
        }

        #endregion

        #region ExecuteSQLReader

        /// <summary>
        /// Execute a sql without parameter like 'select * from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static DbDataReader ExecuteSQLReader(this DbConnection conn, string sql, int? timeout)
        {
            return internalExecuteSQLReader(conn, sql, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <returns></returns>
        public static DbDataReader ExecuteSQLReader(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            return internalExecuteSQLReader(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <returns></returns>
        public static DbDataReader ExecuteSQLReader(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            return internalExecuteSQLReader(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <returns></returns>
        public static DbDataReader ExecuteSQLReader(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            return internalExecuteSQLReader(conn, sql, CommandBehavior.Default, timeout, parameters);
        }


        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DbDataReader ExecuteSQLReader(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            return internalExecuteSQLReader(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        #endregion

        #region ExecuteSQLReaderAsync

        /// <summary>
        /// Execute a sql without parameter like 'select * from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<DbDataReader> ExecuteSQLReaderAsync(this DbConnection conn, string sql, int? timeout)
        {
            return await internalExecuteSQLReaderAsync(conn, sql, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <returns></returns>
        public static async Task<DbDataReader> ExecuteSQLReaderAsync(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            return await internalExecuteSQLReaderAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <returns></returns>
        public static async Task<DbDataReader> ExecuteSQLReaderAsync(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            return await internalExecuteSQLReaderAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <returns></returns>
        public static async Task<DbDataReader> ExecuteSQLReaderAsync(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            return await internalExecuteSQLReaderAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }


        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<DbDataReader> ExecuteSQLReaderAsync(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            return await internalExecuteSQLReaderAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        #endregion

        #region ExecuteSQL

        /// <summary>
        /// Execute a sql without parameter like 'select * from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static DataTable ExecuteSQL(this DbConnection conn, string sql, int? timeout)
        {
            return internalExecuteSQL(conn, sql, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <returns></returns>
        public static DataTable ExecuteSQL(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            return internalExecuteSQL(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <returns></returns>
        public static DataTable ExecuteSQL(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            return internalExecuteSQL(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <returns></returns>
        public static DataTable ExecuteSQL(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            return internalExecuteSQL(conn, sql, CommandBehavior.Default, timeout, parameters);
        }


        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = {0} and Name like {1}'. parameters can be new object[] {123, 'Sydney%'}.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <remarks>This is NOT SQL injection. If that's string operation, no single-quote is needed. Because {0} and {1} will be replaced by auto-generated parameter name.</remarks>
        /// <returns></returns>
        public static DataTable ExecuteSQL(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            return internalExecuteSQL(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        #endregion

        #region ExecuteSQLAsync

        /// <summary>
        /// Execute a sql without parameter like 'select * from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<DataTable> ExecuteSQLAsync(this DbConnection conn, string sql, int? timeout)
        {
            return await internalExecuteSQLAsync(conn, sql, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <returns></returns>
        public static async Task<DataTable> ExecuteSQLAsync(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            return await internalExecuteSQLAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <returns></returns>
        public static async Task<DataTable> ExecuteSQLAsync(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            return await internalExecuteSQLAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <returns></returns>
        public static async Task<DataTable> ExecuteSQLAsync(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            return await internalExecuteSQLAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }


        /// <summary>
        /// Execute a SQL like 'select * from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<DataTable> ExecuteSQLAsync(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            return await internalExecuteSQLAsync(conn, sql, CommandBehavior.Default, timeout, parameters);
        }

        #endregion

        #region ExecuteSQLScalar

        /// <summary>
        /// Execute a sql without parameter like 'select Id from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static object ExecuteSQLScalar(this DbConnection conn, string sql, int? timeout)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static object ExecuteSQLScalar(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static object ExecuteSQLScalar(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static object ExecuteSQLScalar(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }


        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static object ExecuteSQLScalar(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a sql without parameter like 'select Id from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static T ExecuteSQLScalar<T>(this DbConnection conn, string sql, int? timeout)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static T ExecuteSQLScalar<T>(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static T ExecuteSQLScalar<T>(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static T ExecuteSQLScalar<T>(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }


        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static T ExecuteSQLScalar<T>(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            var dt = internalExecuteSQL(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)(dt.Rows[0][0]) : default(T);
        }

        #endregion

        #region ExecuteSQLScalarAsync

        /// <summary>
        /// Execute a sql without parameter like 'select Id from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<object> ExecuteSQLScalarAsync(this DbConnection conn, string sql, int? timeout)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<object> ExecuteSQLScalarAsync(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<object> ExecuteSQLScalarAsync(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<object> ExecuteSQLScalarAsync(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }


        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<object> ExecuteSQLScalarAsync(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? dt.Rows[0][0] : null;
        }

        /// <summary>
        /// Execute a sql without parameter like 'select Id from Customer where Id=123 and Name like 'Syndey%'.
        /// This method is not recommanded to have parameters due to SQL Injection.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<T> ExecuteSQLScalarAsync<T>(this DbConnection conn, string sql, int? timeout)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">A list of database parameters</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<T> ExecuteSQLScalarAsync<T>(this DbConnection conn, string sql, int? timeout, DbParameter[] parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name'. The parameters is a dictionary {Id:123, name:'Sydney%'}
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters">The key contains 'Id' and the value is going to pass to the database</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<T> ExecuteSQLScalarAsync<T>(this DbConnection conn, string sql, int? timeout, Dictionary<string, object> parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = @id and Name like @name ' and the parameters can be new { id = 123, name = "Sydney%" }
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameter">Loop through all properties. This can be anonymous class.</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<T> ExecuteSQLScalarAsync<T>(this DbConnection conn, string sql, int? timeout, object parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }


        /// <summary>
        /// Execute a SQL like 'select Id from Customer where Id = {0} and Name like {1}'. parameters can be [123, 'Sydney%']
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns></returns>
        public static async Task<T> ExecuteSQLScalarAsync<T>(this DbConnection conn, string sql, int? timeout, object[] parameters)
        {
            var dt = await internalExecuteSQLAsync(conn, sql, CommandBehavior.SingleResult, timeout, parameters);
            return (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0) ? (T)dt.Rows[0][0] : default(T);
        }

        #endregion

        #region internalExecuteSQL


        private static DataTable internalExecuteSQL(this DbConnection conn, string sql, CommandBehavior beh, int? timeout)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;
                using (var reader = cmd.ExecuteReader(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        private static DataTable internalExecuteSQL(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, DbParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = cmd.ExecuteReader(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        private static DataTable internalExecuteSQL(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, Dictionary<string, object> parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = cmd.ExecuteReader(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        private static DataTable internalExecuteSQL(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = cmd.ExecuteReader(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }


        private static DataTable internalExecuteSQL(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = cmd.ExecuteReader(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        #endregion

        #region internalExecuteSQLAsync


        private static async Task<DataTable> internalExecuteSQLAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                using (var reader = await cmd.ExecuteReaderAsync(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        private static async Task<DataTable> internalExecuteSQLAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, DbParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = await cmd.ExecuteReaderAsync(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        private static async Task<DataTable> internalExecuteSQLAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, Dictionary<string, object> parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = await cmd.ExecuteReaderAsync(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }

        private static async Task<DataTable> internalExecuteSQLAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = await cmd.ExecuteReaderAsync(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }


        private static async Task<DataTable> internalExecuteSQLAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                using (var reader = await cmd.ExecuteReaderAsync(beh))
                {
                    return reader.ToDataTable();
                }
            }
        }




        #endregion

        #region internalExecuteSQLReader


        private static DbDataReader internalExecuteSQLReader(this DbConnection conn, string sql, CommandBehavior beh, int? timeout)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;
                return cmd.ExecuteReader(beh);
            }
        }

        private static DbDataReader internalExecuteSQLReader(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, DbParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return cmd.ExecuteReader(beh);
            }
        }

        private static DbDataReader internalExecuteSQLReader(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, Dictionary<string, object> parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return cmd.ExecuteReader(beh);
            }
        }

        private static DbDataReader internalExecuteSQLReader(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return cmd.ExecuteReader(beh);
            }
        }


        private static DbDataReader internalExecuteSQLReader(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return cmd.ExecuteReader(beh);
            }
        }

        #endregion

        #region internalExecuteSQLReaderAsync


        private static async Task<DbDataReader> internalExecuteSQLReaderAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;
                return await cmd.ExecuteReaderAsync(beh);
            }
        }

        private static async Task<DbDataReader> internalExecuteSQLReaderAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, DbParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return await cmd.ExecuteReaderAsync(beh);
            }
        }

        private static async Task<DbDataReader> internalExecuteSQLReaderAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, Dictionary<string, object> parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return await cmd.ExecuteReaderAsync(beh);
            }
        }

        private static async Task<DbDataReader> internalExecuteSQLReaderAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return await cmd.ExecuteReaderAsync(beh);
            }
        }


        private static async Task<DbDataReader> internalExecuteSQLReaderAsync(this DbConnection conn, string sql, CommandBehavior beh, int? timeout, object[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : DEFAULT_TIMEOUT;

                cmd.SetParametersToCommand(parameters);

                return await cmd.ExecuteReaderAsync(beh);
            }
        }

        #endregion

        #region SetParametersToCommand

        const string PARAM_PREFIX = "@";

        private static void SetParametersToCommand(this DbCommand cmd, DbParameter[] parameters)
        {
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
        }

        private static void SetParametersToCommand(this DbCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = PARAM_PREFIX + kv.Key;
                    param.Value = ConvertSQLAcceptedDataType(kv.Value);
                    cmd.Parameters.Add(param);
                }
            }
        }

        private static void SetParametersToCommand(this DbCommand cmd, object parameters)
        {
            if (parameters != null)
            {
                foreach (var p in parameters.GetType().GetProperties())
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = PARAM_PREFIX + p.Name;
                    var value = p.GetValue(parameters);
                    param.Value = ConvertSQLAcceptedDataType(value);
                    cmd.Parameters.Add(param);
                }
            }
        }

        private static void SetParametersToCommand(this DbCommand cmd, object[] parameters)
        {
            var replacement = new List<string>();
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    var name = PARAM_PREFIX + "__pp" + replacement.Count.ToString() + "__";
                    var param = cmd.CreateParameter();
                    param.ParameterName = name;
                    param.Value = ConvertSQLAcceptedDataType(p);

                    cmd.Parameters.Add(param);
                    replacement.Add(name);
                }
            }

            cmd.CommandText = string.Format(cmd.CommandText, replacement.ToArray());
        }

        private static object ConvertSQLAcceptedDataType(object value)
        {
            if (value == null) return DBNull.Value;
            if (value is XElement)
            {
                var doc = new XmlDocument();
                doc.Load(((XElement)value).CreateReader());
                return doc.DocumentElement;
            }
            else
            {
                return value;
            }
        }

        #endregion

    }
}
